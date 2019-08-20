using FarmervsZombies.Pathfinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal abstract class BMovableGameObject : BGameObject
    {
        public float Speed { get; protected set; } = 2;
        private UnitPath mPath;
        public virtual bool HasPath => mPath != null;
        public bool PathEmpty => HasPath && mPath.Empty();
        protected enum Directions { Up, Right, Down, Left }
        protected bool mOnlyLeftRightDirection = false;
        protected int mDirection = 2;
        protected static readonly Random sRandom = new Random();
        private const float RandomWalkChance = 0.004f;
        protected bool mRandomWalk;
        protected float mRandomWalkDistance = 1;
        private readonly SteeringManager mSteering;
        public Vector2 Velocity { get; private set; } = Vector2.Zero;
        private readonly Vector2[] mLastVelocities = new Vector2[10];
        private int mCurrentLastVelocitiesIndex;
        private readonly Animation mBloodAnimation;
        private Texture2D mBlood;
        private float mLastHealth;
        private bool mLastHealthInitialized;
        private bool mShowBlood;
        private float mBloodRotation;
        private Vector2 mBloodOffset;
        private float mBloodScale;
        private readonly List<ICollidable> mCollidingObjects = new List<ICollidable>();
        private List<ICollidable> mNeighborObjects = new List<ICollidable>();

        protected BMovableGameObject(Texture2D texture, float positionX, float positionY, float textureOffsetX, float textureOffsetY, float collisionBoxSizeX = 0.7f, float collisionBoxSizeY = 0.7f)
            : base(texture, positionX, positionY, textureOffsetX, textureOffsetY, collisionBoxSizeX, collisionBoxSizeY)
        {
            InputManager.AnyActionEvent += MoveToTarget;
            mSteering = new SteeringManager(this);
            mBloodAnimation = AnimationManager.GetAnimation(AnimationManager.Blood, this);
            mBloodAnimation.AnimationFinished = true;
            mBlood = TextureManager.GetTexture("blood", 32, 32, 0);
            InputManager.AnyActionEvent += StopMovement;
            Mass = 1;
            Position += QuadTree.ClampPosition(this);
        }

        public void Move(GameTime gameTime)
        {
            if (Health <= 0) return;
            var movement = Velocity * gameTime.ElapsedGameTime.Milliseconds / 1000;
            mLastVelocities[mCurrentLastVelocitiesIndex] = Velocity;
            mCurrentLastVelocitiesIndex++;
            mCurrentLastVelocitiesIndex %= mLastVelocities.Length;
            var averageVelocity = mLastVelocities.Aggregate(Vector2.Zero, (current, velocity) => current + velocity);

            // If collision happens, don't change texture direction
            if (mCollidingObjects.Any()) averageVelocity = Vector2.Zero;
            Position += movement;
            if (CurrentQuadTree == null)
            {
                Position -= movement;
            }
            else
            {
                Position += QuadTree.ClampPosition(this);
                CurrentQuadTree.UpdateGameObjectPosition(this);
            }

            if (Math.Abs(averageVelocity.Y) > 2 * Math.Abs(averageVelocity.X) && !mOnlyLeftRightDirection)
            {
                if (averageVelocity.Y > 0) mDirection = (int)Directions.Down;
                else mDirection = (int)Directions.Up;
            }
            else if (2 * Math.Abs(averageVelocity.Y) < Math.Abs(averageVelocity.X))
            {
                if (averageVelocity.X > 0) mDirection = (int)Directions.Right;
                else mDirection = (int)Directions.Left;
            }
            else
            {
                if (mDirection == (int) Directions.Down && averageVelocity.Y < 0 ||
                    mDirection == (int) Directions.Up && averageVelocity.Y > 0 ||
                    mDirection == (int) Directions.Right && averageVelocity.X < 0 ||
                    mDirection == (int) Directions.Left && averageVelocity.X > 0)
                {
                    mDirection = (int) GetDirection(averageVelocity);
                }
            }
        }

        private Directions GetDirection(Vector2 velocity)
        {
            if (Math.Abs(velocity.Y) > Math.Abs(velocity.X) && !mOnlyLeftRightDirection)
            {
                return velocity.Y > 0 ? Directions.Down : Directions.Up;
            }

            return velocity.X > 0 ? Directions.Right : Directions.Left;
        }

        protected internal void RequestPath(Vector2 target, bool hasToReachTarget = false, bool loose = false)
        {
            target.X = Math.Min(Math.Max(target.X, 0), Game1.MapWidth - 0.1f);
            target.Y = Math.Min(Math.Max(target.Y, 0), Game1.MapHeight - 0.1f);
            mPath = Pathfinder.GetPath(CollisionBoxCenter, target, hasToReachTarget);

            if (loose) return;
            if (Vector2.Distance(CollisionBoxCenter, target) > 0.5f && mPath.Empty())
            {
                mPath = Pathfinder.GetPath(CollisionBoxCenter, target, hasToReachTarget, false);
            }
        }

        private bool FollowPath()
        {
            if (!HasPath) return false;

            if (!mPath.Valid)
            {
                RequestPath(mPath.Target);
            }

            var nextPathPosition = mPath.Next(CollisionBoxCenter);
            mSteering.UpdateSteering(nextPathPosition, mNeighborObjects);

            if (!mPath.TargetReached(CollisionBoxCenter)) return true;

            mPath = null;
            Velocity = Vector2.Zero;
            return false;
        }

        protected void DeletePath()
        {
            mPath = null;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            mCollidingObjects.Clear();
            mNeighborObjects = CurrentQuadTree.GetCollidingObjects(this, 1.5f);
            foreach (var neighbor in mNeighborObjects)
            {
                if (neighbor is BGameObject obj && QuadTree.AreGameObjectsColliding(this, obj))
                    mCollidingObjects.Add(neighbor);
            }

            if (mRandomWalk && !HasPath && sRandom.NextDouble() < RandomWalkChance)
            {
                var distance = 0.5 + mRandomWalkDistance * sRandom.NextDouble();
                var angle = sRandom.NextDouble() * Math.PI * 2;
                var target = Position + new Vector2((float) (Math.Cos(angle) * distance),
                                     (float) (Math.Sin(angle) * distance));
                if (GetCollidingObjects(target).Count == 0)
                {
                    RequestPath(CollisionBoxCenter + new Vector2((float)(Math.Cos(angle) * distance), (float)(Math.Sin(angle) * distance)), loose: true);
                }
            }

            if (!FollowPath())
            {
                mSteering.UpdateSteering(CollisionBoxCenter, mNeighborObjects);
            }
            Velocity += mSteering.GetSteering();
            float collisionFactor = mCollidingObjects.Any() || GetCollidingWaterTiles().Any() ? 2 : 1;
            if (Velocity.Length() > Speed * collisionFactor)
            {
                if (Velocity != Vector2.Zero) Velocity = Vector2.Normalize(Velocity);
                Velocity *= Speed * collisionFactor;
            }

            if (!mLastHealthInitialized)
            {
                mLastHealth = Health;
                mLastHealthInitialized = true;
            } 
            if (mLastHealth - Health > 2 && !mShowBlood)
            {
                mBloodAnimation.Reset();
                mShowBlood = true;
                mBloodRotation = (float)(sRandom.NextDouble() * 2 * Math.PI);
                mBloodOffset = new Vector2(Width / 64.0f, (float)sRandom.NextDouble() * Height / 32.0f);
                mBloodScale = (float)sRandom.NextDouble() + 0.6f;
            }

            if (mBloodAnimation.AnimationFinished) mShowBlood = false;

            mBlood = mBloodAnimation.GetTextureOnce();
            mLastHealth = Health;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {

            base.Draw(spriteBatch, camTransform);
            if (mShowBlood)
            {
                spriteBatch.Draw(mBlood,
                    new Vector2((Position.X + TextureOffset.X + mBloodOffset.X) * 32, (Position.Y + TextureOffset.Y + mBloodOffset.Y) * 32),
                    null,
                    mBackgroundColour,
                    mBloodRotation,
                    new Vector2(16, 16), 
                    mBloodScale,
                    SpriteEffects.None,
                    0);
            }
        }

        protected void MoveToTarget(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.MoveToTarget)) return;
            if (!Selected) return;
            if (ObjectManager.Instance.CheckTile(Position) == null) return;

            var worldPosition = inputState.mMouseWorldPosition;

            RequestPath(worldPosition);
        }

        public void InvalidatePath()
        {
            mPath?.Invalidate();
        }

        public List<ICollidable> GetCollidingObjects()
        {
            return mCollidingObjects;
        }

        private List<ICollidable> GetCollidingObjects(Vector2 position)
        {
            return CurrentQuadTree.GetCollidingObjects(this, position);
        }

        public IEnumerable<Vector2> GetCollidingWaterTiles()
        {
            return QuadTree.GetCollidingWaterTiles(this);
        }

        private void StopMovement(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.StopMovement)) return;
            if (!Selected) return;
            Velocity = Vector2.Zero;
            mPath = null;
        }
    }
}