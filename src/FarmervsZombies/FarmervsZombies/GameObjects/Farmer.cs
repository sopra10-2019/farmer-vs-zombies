using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace FarmervsZombies.GameObjects
{
    internal sealed class Farmer : BMovableGameObject, IAttackable, ICollidable
    {
        private readonly Animation mWalkDownAnimation;
        private readonly Animation mWalkUpAnimation;
        private readonly Animation mWalkLeftAnimation;
        private readonly Animation mWalkRightAnimation;
        private readonly Animation mStandDownAnimation;
        private readonly Animation mStandUpAnimation;
        private readonly Animation mStandLeftAnimation;
        private readonly Animation mStandRightAnimation;
        private readonly Animation mAttack1DownAnimation;
        private readonly Animation mAttack1UpAnimation;
        private readonly Animation mAttack1LeftAnimation;
        private readonly Animation mAttack1RightAnimation;
        private readonly Animation mAttack2DownAnimation;
        private readonly Animation mAttack2UpAnimation;
        private readonly Animation mAttack2LeftAnimation;
        private readonly Animation mAttack2RightAnimation;
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -1;

        public bool Team { get; } = true;

        private float AttackDamage { get; } = 40f;
        private float AttackRange { get; } = 1.2f;
        private float AttackCooldown { get; } = 1f;
        private float mTimeSinceAttack;

        private IAttackable mTarget;
        private Vector2 mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        private bool HasTarget => Target != null;

        private IAttackable Target
        {
            get => mTarget;
            set
            {
                if (value == this || value != null && value.Team == Team) return;
                mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                mTarget = value;
            }
        }

        public Farmer(float positionX, float positionY) : base(TextureManager.GetTexture("farmer_stand", 32, 64, 2),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            Defense = 175;

            InputManager.AnyActionEvent += MoveFarmer;
            InputManager.AnyActionEvent += SetTarget;

            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerWalkDown, this);
            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerWalkUp, this);
            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerWalkLeft, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerWalkRight, this);

            mStandDownAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerStandDown, this);
            mStandUpAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerStandUp, this);
            mStandLeftAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerStandLeft, this);
            mStandRightAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerStandRight, this);

            mAttack1DownAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack1Down, this);
            mAttack1UpAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack1Up, this);
            mAttack1LeftAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack1Left, this);
            mAttack1RightAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack1Right, this);

            mAttack2DownAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack2Down, this);
            mAttack2UpAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack2Up, this);
            mAttack2LeftAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack2Left, this);
            mAttack2RightAnimation = AnimationManager.GetAnimation(AnimationManager.FarmerAttack2Right, this);

            mAttack1UpAnimation.AnimationFinished = true;
            mAttack1RightAnimation.AnimationFinished = true;
            mAttack1DownAnimation.AnimationFinished = true;
            mAttack1LeftAnimation.AnimationFinished = true;

            mAttack2UpAnimation.AnimationFinished = true;
            mAttack2RightAnimation.AnimationFinished = true;
            mAttack2DownAnimation.AnimationFinished = true;
            mAttack2LeftAnimation.AnimationFinished = true;

            Selected = true;
            ObjectManager.Instance.AddSelected(this);
            mDrawLiveBarMySelf = true;
        }

        private void SetTarget(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.SetTarget)) return;
            if (!Selected) return;
            var pos = inputState.mMouseWorldPosition;
            Target = null;
            foreach (var current in ObjectManager.Instance.TargetsInRange(pos, 1))
            {
                if (Team == current.Team) continue;
                Target = current;
                break;
            }
        }

        private void Attack()
        {
            mAttack2UpAnimation.ResetOnce();
            mAttack2RightAnimation.ResetOnce();
            mAttack2DownAnimation.ResetOnce();
            mAttack2LeftAnimation.ResetOnce();
            SoundManager.PlaySoundWithCooldown("hit2", 2, 0.8f);
            Target.Damage(AttackDamage);
        }

        protected override void Death()
        {
            Game1.FarmerDied = true;
            Game1.sStatistics.Lost();
            base.Death();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (HasTarget)
            {
                if (Vector2.Distance(CollisionBoxCenter, TargetPosition(Target)) <= AttackRange + Target.CollisionBoxSize.Length() / 2 && mTimeSinceAttack >= AttackCooldown)
                {
                    if (Target.Health > 0)
                    {
                        Attack();
                    }
                    else
                    {
                        Target = null;
                        DeletePath();
                    }
                    mTimeSinceAttack = 0;
                }
                else if (Vector2.Distance(CollisionBoxCenter, TargetPosition(Target)) > AttackRange + Target.CollisionBoxSize.Length() / 2)
                {
                    if (Vector2.Distance(TargetPosition(Target), mLastTargetPosition) >= 5f
                        || Vector2.Distance(CollisionBoxCenter, mLastTargetPosition) < AttackRange)
                    {
                        RequestPath(TargetPosition(Target), true);
                        mLastTargetPosition = TargetPosition(Target);
                    }
                    else if (!HasPath)
                    {
                        if (Pathfinding.Pathfinder.ExistsPath(CollisionBoxCenter, TargetPosition(Target)))
                            RequestPath(TargetPosition(Target), true);
                        else
                            Target = null;
                    }
                }
            }

            if (Velocity == Vector2.Zero)
            {
                switch (mDirection)
                {
                    case 0:
                        mTexture = mStandUpAnimation.GetTexture();
                        TextureOffset = mStandUpAnimation.TextureOffset;
                        Width = mStandUpAnimation.Width;
                        break;
                    case 1:
                        mTexture = mStandRightAnimation.GetTexture();
                        TextureOffset = mStandRightAnimation.TextureOffset;
                        Width = mStandRightAnimation.Width;
                        break;
                    case 2:
                        mTexture = mStandDownAnimation.GetTexture();
                        TextureOffset = mStandDownAnimation.TextureOffset;
                        Width = mStandDownAnimation.Width;
                        break;
                    case 3:
                        mTexture = mStandLeftAnimation.GetTexture();
                        TextureOffset = mStandLeftAnimation.TextureOffset;
                        Width = mStandLeftAnimation.Width;
                        break;
                }
            }
            else
            {
                switch (mDirection)
                {
                    case 0:
                        mTexture = mWalkUpAnimation.GetTexture();
                        TextureOffset = mWalkUpAnimation.TextureOffset;
                        Width = mStandUpAnimation.Width;
                        break;
                    case 1:
                        mTexture = mWalkRightAnimation.GetTexture();
                        TextureOffset = mWalkRightAnimation.TextureOffset;
                        Width = mStandRightAnimation.Width;
                        break;
                    case 2:
                        mTexture = mWalkDownAnimation.GetTexture();
                        TextureOffset = mWalkDownAnimation.TextureOffset;
                        Width = mStandDownAnimation.Width;
                        break;
                    case 3:
                        mTexture = mWalkLeftAnimation.GetTexture();
                        TextureOffset = mWalkLeftAnimation.TextureOffset;
                        Width = mStandLeftAnimation.Width;
                        break;
                }
            }

            if (!mAttack2UpAnimation.AnimationFinished && !mAttack2RightAnimation.AnimationFinished &&
                !mAttack2DownAnimation.AnimationFinished && !mAttack2LeftAnimation.AnimationFinished)
            {

                // Attack animation
                switch (mDirection)
                {
                    case 0:
                        mTexture = mAttack2UpAnimation.GetTextureOnce();
                        TextureOffset = mAttack2UpAnimation.TextureOffset;
                        Width = mAttack2UpAnimation.Width;
                        break;
                    case 1:
                        mTexture = mAttack2RightAnimation.GetTextureOnce();
                        TextureOffset = mAttack2RightAnimation.TextureOffset;
                        Width = mAttack2RightAnimation.Width;
                        break;
                    case 2:
                        mTexture = mAttack2DownAnimation.GetTextureOnce();
                        TextureOffset = mAttack2DownAnimation.TextureOffset;
                        Width = mAttack2DownAnimation.Width;
                        break;
                    case 3:
                        mTexture = mAttack2LeftAnimation.GetTextureOnce();
                        TextureOffset = mAttack2LeftAnimation.TextureOffset;
                        Width = mAttack2LeftAnimation.Width;
                        break;
                }
            }
            else
            {
                mAttack2UpAnimation.AnimationFinished = true;
                mAttack2RightAnimation.AnimationFinished = true;
                mAttack2DownAnimation.AnimationFinished = true;
                mAttack2LeftAnimation.AnimationFinished = true;
            }

            mTimeSinceAttack += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        private static Vector2 TargetPosition(IAttackable target)
        {
            return target.CollisionBoxCenter;
        }

        private void MoveFarmer(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.MoveFarmer)) return;
            var movement = Vector2.Zero;
            if (inputState.IsKeyPressed(Keys.A))
            {
                movement -= new Vector2(0.05f, 0);
                mDirection = (int)Directions.Left;
                FarmerQueueManager.Instance.EmptyFQueue();
            }
            if (inputState.IsKeyPressed(Keys.D))
            {
                movement += new Vector2(0.05f, 0);
                mDirection = (int)Directions.Right;
                FarmerQueueManager.Instance.EmptyFQueue();
            }
            if (inputState.IsKeyPressed(Keys.W))
            {
                movement -= new Vector2(0, 0.05f);
                mDirection = (int)Directions.Up;
                FarmerQueueManager.Instance.EmptyFQueue();
            }
            if (inputState.IsKeyPressed(Keys.S))
            {
                movement += new Vector2(0, 0.05f);
                mDirection = (int)Directions.Down;
                FarmerQueueManager.Instance.EmptyFQueue();
            }

            Position += movement;
            if (CurrentQuadTree == null || !CurrentQuadTree.UpdateGameObjectPosition(this))
            {
                Position -= movement;
            }
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Farmer");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Money", EconomyManager.Instance.GoldAmount.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("SeedOne", EconomyManager.Instance.SeedAmount1.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("SeedTwo", EconomyManager.Instance.SeedAmount2.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("SeedThree", EconomyManager.Instance.SeedAmount3.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("points", Game1.sStatistics.Points.ToString());
            element.SetAttribute("time", Game1.mTime.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
