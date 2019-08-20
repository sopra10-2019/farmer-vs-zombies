using FarmervsZombies.AI;
using FarmervsZombies.Managers;
using FarmervsZombies.Pathfinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;
using System.Xml;


namespace FarmervsZombies.GameObjects
{
    internal class Zombie : BMovableGameObject, IAttackable, ITauntable, ICollidable
    {
        private readonly Animation mStandLeftAnimation;
        private readonly Animation mStandRightAnimation;
        protected readonly Animation mWalkLeftAnimation;
        protected readonly Animation mWalkRightAnimation;
        private readonly Animation mWalkUpAnimation;
        private readonly Animation mWalkDownAnimation;
        private readonly Animation mDeadRightAnimation;
        private readonly Animation mDeadLeftAnimation;
        private readonly Animation mAttackRightAnimation;
        private readonly Animation mAttackLeftAnimation;
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -1;

        private const int DefaultMaxHealth = 50;
         

        protected float AttackDamage { private get; set; } = 15;
        protected float AttackRange { private get; set; } = 1f;
        private const float AttackCooldown = 1;
        private const float TauntDuration = 5;
        private const float AttackNextCoolDown = 0.5f;
        private const float AggroRange = 7.5f;
        private IAttackable mTarget;
        private float mTimeSinceAttack;
        private float mTimeSinceTaunt;
        private float mTimeSinceAttackNextLookup;
        private const double SpoilChance = 0.5;
        private Vector2 mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

        public bool Team { get; } = false;
        public bool HasTarget => Target != null;
        private bool Taunted { get; set; }
        public Type PreferredTargetType { private get; set; }
        public bool Aggressive { private get; set; }
        public IAttackable Target
        {
            protected get => mTarget;
            set
            {
                if (value == this || value == mTarget || Taunted || value != null && value.Team == Team) return;
                mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                mTarget = value;
            }
        }

        public Zombie(float positionX, float positionY, bool randomWalk = false, float randomWalkDistance = 1, int loadhealth = DefaultMaxHealth)
            : this(positionX, positionY, AnimationManager.ZombieWalkLeft, AnimationManager.ZombieWalkRight, null, null, true, randomWalk, randomWalkDistance)
        {
            MaxHealth = DefaultMaxHealth;
            Health = loadhealth;
            Mass = 1.2f;
        }

        protected Zombie(float positionX, float positionY, Animation left, Animation right, Animation up, Animation down, bool leftRight, bool randomWalk = false, float randomWalkDistance = 1)
            : base(TextureManager.GetTexture("zombie_stand", 32, 64, 1),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            AttackDamage *= Game1.Difficulty == 3 ? 2 : 1;
            Defense = 22.22f + 11.11f * Game1.Difficulty;
            Speed = 1.5f;

            mStandLeftAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieStandLeft, this);
            mStandRightAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieStandRight, this);

            mWalkLeftAnimation = AnimationManager.GetAnimation(left, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(right, this);
            mWalkUpAnimation = AnimationManager.GetAnimation(up, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(down, this);

            mDeadLeftAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieDeadLeft, this);
            mDeadRightAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieDeadRight, this);
            mAttackLeftAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieAttackLeft, this);
            mAttackRightAnimation = AnimationManager.GetAnimation(AnimationManager.ZombieAttackRight, this);

            mAttackLeftAnimation.AnimationFinished = true;
            mAttackRightAnimation.AnimationFinished = true;

            mOnlyLeftRightDirection = leftRight;
            mRandomWalk = randomWalk;
            mRandomWalkDistance = randomWalkDistance;
            mDrawLiveBarMySelf = true;
        }

        private void AttackNext(Type type = null)
        {
            if (mTimeSinceAttackNextLookup < AttackNextCoolDown) return;

            IAttackable current = null;
            foreach (var target in ObjectManager.Instance.TargetsInRange(CollisionBoxCenter, AggroRange))
            {
                if (Team == target.Team) continue;
                if (type != null && !type.IsInstanceOfType(target)) continue;
                if (current != null && Vector2.Distance(CollisionBoxCenter, TargetPosition(target)) >
                                       Vector2.Distance(CollisionBoxCenter, current.CollisionBoxCenter)) continue;
                if (!Pathfinder.ExistsPath(CollisionBoxCenter, TargetPosition(target))) continue;

                current = target;
            }

            Target = current;
            if (Target == null && type != null) AttackNext();
            mTimeSinceAttackNextLookup = 0f;
        }

        public void Taunt(IAttackable target)
        {
            Target = target;
            Taunted = true;
            mTimeSinceTaunt = 0;
        }

        public void Charge(Vector2 target)
        {
            RequestPath(target);
            Target = null;
            Aggressive = true;
        }

        public override void Damage(float damage)
        {
            base.Damage(damage);

            if (HasTarget && Vector2.Distance(CollisionBoxCenter, Target.CollisionBoxCenter) > AggroRange)
            {
                AttackNext();
            }
        }

        protected override void Death()
        {
            if (sRandom.NextDouble() < SpoilChance)
                Game1.sTileMap.PlantWasteland((int)CollisionBoxCenter.X, (int)CollisionBoxCenter.Y);
            SoundManager.PlaySoundWithCooldown("zombie_death", 4);
            Ai.RemoveZombie(this);
        }

        protected virtual Texture2D GetStandingTexture()
        {
            switch (mDirection)
            {
                case 0:
                case 1:
                    TextureOffset = mStandRightAnimation.TextureOffset;
                    return mStandRightAnimation.GetTexture();
                case 2:
                case 3:
                    TextureOffset = mStandLeftAnimation.TextureOffset;
                    return mStandLeftAnimation.GetTexture();
            }

            throw new InvalidOperationException("Invalid direction for zombie.");
        }

        // Zombies won't be selected
        public override void Select()
        {

        }

        protected virtual void MakeSound(Farmer farmer)
        {
            // Random zombie noises
            if (farmer != null && Vector2.Distance(Position, farmer.Position) < 30 && sRandom.NextDouble() < 0.001)
            {
                SoundManager.PlaySoundWithCooldown("zombie", 4, Vector2.Distance(Position, farmer.Position) < 10 ? 1f : 0.5f);
            }
            else if (farmer != null && Vector2.Distance(Position, farmer.Position) > 30 && sRandom.NextDouble() < 0.001)
            {
                SoundManager.PlaySoundWithCooldown("zombie", 4, Vector2.Distance(Position, farmer.Position) < 10 ? 0.5f : 0f);
            }
            else if (farmer != null && Vector2.Distance(Position, farmer.Position) < 30 && sRandom.NextDouble() > 0.95)
            {
                SoundManager.PlaySoundWithCooldown("chains", 8, Vector2.Distance(Position, farmer.Position) < 10 ? 1f : 0.5f);
            }
            else if (farmer != null && Vector2.Distance(Position, farmer.Position) > 30 && sRandom.NextDouble() > 0.95)
            {
                SoundManager.PlaySoundWithCooldown("chains", 8, Vector2.Distance(Position, farmer.Position) < 10 ? 0.5f : 0f);
            }
            else if (farmer != null && Vector2.Distance(Position, farmer.Position) < 30 && sRandom.NextDouble() > 0.002 && sRandom.NextDouble() < 0.004)
            {
                SoundManager.PlaySoundWithCooldown("zombie_moan",
                    4,
                    Vector2.Distance(Position, farmer.Position) < 10 ? 1f : 0.5f);
            }
            else if (farmer != null && Vector2.Distance(Position, farmer.Position) > 30 && sRandom.NextDouble() > 0.002 && sRandom.NextDouble() < 0.004)
            {
                SoundManager.PlaySoundWithCooldown("zombie_moan", 4, Vector2.Distance(Position, farmer.Position) < 10 ? 0.5f : 0f);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var farmer = ObjectManager.Instance.GetFarmer();

            MakeSound(farmer);

            if (Velocity == Vector2.Zero)
            {
                mTexture = GetStandingTexture();
            }
            else
            {
                switch (mDirection)
                {
                    case 0:
                        if (mWalkUpAnimation != null)
                        {
                            mTexture = mWalkUpAnimation.GetTexture();
                            TextureOffset = mWalkUpAnimation.TextureOffset;
                        }
                        else
                        {
                            mTexture = mWalkRightAnimation.GetTexture();
                            TextureOffset = mWalkRightAnimation.TextureOffset;
                        }
                        break;
                    case 1:
                        mTexture = mWalkRightAnimation.GetTexture();
                        TextureOffset = mWalkRightAnimation.TextureOffset;
                        break;
                    case 2:
                        if (mWalkDownAnimation != null)
                        {
                            mTexture = mWalkDownAnimation.GetTexture();
                            TextureOffset = mWalkDownAnimation.TextureOffset;
                        }
                        else
                        {
                            mTexture = mWalkLeftAnimation.GetTexture();
                            TextureOffset = mWalkLeftAnimation.TextureOffset;
                        }
                        break;
                    case 3:
                        mTexture = mWalkLeftAnimation.GetTexture();
                        TextureOffset = mWalkLeftAnimation.TextureOffset;
                        break;
                }
            }

            if (!mAttackRightAnimation.AnimationFinished && !mAttackLeftAnimation.AnimationFinished)
            {
                if (!(this is Necromancer)) // Remove this after presentation
                {
                    switch (mDirection)
                    {
                        case 1:
                            mTexture = mAttackRightAnimation.GetTextureOnce();
                            TextureOffset = mAttackRightAnimation.TextureOffset;
                            break;
                        case 3:
                            mTexture = mAttackLeftAnimation.GetTextureOnce();
                            TextureOffset = mAttackLeftAnimation.TextureOffset;
                            break;
                    }
                }
            }
            else
            {
                mAttackLeftAnimation.AnimationFinished = true;
                mAttackRightAnimation.AnimationFinished = true;
            }

            if (Health <= 0)
            {
                switch (mDirection)
                {
                    case 1:
                        mTexture = mDeadRightAnimation.GetTextureOnce();
                        TextureOffset = mDeadRightAnimation.TextureOffset;
                        break;
                    case 3:
                        mTexture = mDeadLeftAnimation.GetTextureOnce();
                        TextureOffset = mDeadLeftAnimation.TextureOffset;
                        break;
                }

                if (mDeadLeftAnimation.AnimationFinished || mDeadRightAnimation.AnimationFinished)
                {
                    base.Death();
                }
            }

            Width = mTexture.Width;
            Height = mTexture.Height;

            if (farmer != null && Vector2.Distance(Position, farmer.Position) <= 5f)
                AttackNext(typeof(Farmer));

            if (HasTarget)
            {
                if (Vector2.Distance(CollisionBoxCenter, TargetPosition(Target)) <= AttackRange + Target.CollisionBoxSize.Length() / 2 && mTimeSinceAttack >= AttackCooldown)
                {
                    if (Target.Health > 0)
                    {
                        // Attack animation
                        SoundManager.PlaySoundWithCooldown("hit2", 2, 0.8f);
                        Target.Damage(AttackDamage);
                        mAttackRightAnimation.ResetOnce();
                        mAttackLeftAnimation.ResetOnce();
                    }
                    else
                    {
                        Target = null;
                    }
                    DeletePath();
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
                        if (Pathfinder.ExistsPath(CollisionBoxCenter, TargetPosition(Target)))
                            RequestPath(TargetPosition(Target), true);
                        else
                            Target = null;
                    }
                }
            }
            else if (Aggressive && !HasPath)
            {
                AttackNext(PreferredTargetType);
            }

            Taunted = mTimeSinceTaunt <= TauntDuration;

            mTimeSinceAttack += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            mTimeSinceTaunt += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            mTimeSinceAttackNextLookup += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        private static Vector2 TargetPosition(IAttackable target)
        {
            return target.CollisionBoxCenter;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            if (!(Health > 0)) return;
            var element = doc.CreateElement("Zombie");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            var health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}

