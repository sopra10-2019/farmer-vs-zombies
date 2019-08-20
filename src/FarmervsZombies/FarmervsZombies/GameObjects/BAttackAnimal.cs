using System;
using FarmervsZombies.Managers;
using FarmervsZombies.Pathfinding;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal abstract class BAttackAnimal : BAnimal
    {
        public float AttackDamage { get; protected set; } = 15f;
        protected float AttackRange { get; set; } = 1f;
        protected float AttackCooldown { get; set; } = 1f;
        protected float AggroRange { private get; set; } = 5f;
        private float mTimeSinceAttack;

        private IAttackable mTarget;
        private Vector2 mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        private bool HasTarget => Target != null;
        private const float AttackNextCoolDown = 0.5f;
        private float mTimeSinceAttackNextLookup;

        public override bool LevelUp { get; } = false;

        protected IAttackable Target
        {
            get => mTarget;
            private set
            {
                if (value == this || (value != null && value.Team == Team)) return;
                mLastTargetPosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
                mTarget = value;
            }
        }

        public bool Aggressive { get; set; } = true;

        protected BAttackAnimal(Texture2D texture,
            float positionX,
            float positionY,
            float textureOffsetX,
            float textureOffsetY)
            : base(texture, positionX, positionY, textureOffsetX, textureOffsetY)
        {
            mRandomWalk = false;
            InputManager.AnyActionEvent += SetTarget;
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

        protected virtual void AttackNext(bool needsPath = true)
        {
            if (mTimeSinceAttackNextLookup < AttackNextCoolDown) return;
            mTimeSinceAttackNextLookup = 0f;

            IAttackable current = null;
            foreach (var target in ObjectManager.Instance.TargetsInRange(CollisionBoxCenter, AggroRange))
            {
                if (Team == target.Team) continue;
                if (current != null && Vector2.Distance(CollisionBoxCenter, TargetPosition(target)) >
                    Vector2.Distance(CollisionBoxCenter, current.CollisionBoxCenter)) continue;
                if (needsPath && !Pathfinder.ExistsPath(CollisionBoxCenter, TargetPosition(target))) continue;

                current = target;
            }

            Target = current;
        }

        protected virtual void Attack()
        {
            // Attack animation
            SoundManager.PlaySoundWithCooldown("hit2", 2, 0.8f);
            Target.Damage(AttackDamage);
        }

        protected override void GenerateResources(bool sound = true)
        {

        }

        protected override void Eat()
        {
            if (HasTarget) return;
            base.Eat();
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
                    }
                    DeletePath();
                    mTimeSinceAttack = 0;
                }
                else if (Vector2.Distance(CollisionBoxCenter, TargetPosition(Target)) > AttackRange + Target.CollisionBoxSize.Length() / 2)
                {
                    var offset = TargetPosition(Target) - Position;
                    if (offset != Vector2.Zero) offset.Normalize();
                    offset *= AttackRange - 0.5f;
                    if (Math.Abs(AttackRange - 1.5f) < 0.001f) offset = Vector2.Zero;
                    if (Vector2.Distance(TargetPosition(Target), mLastTargetPosition) >= 5f
                        || Vector2.Distance(CollisionBoxCenter, mLastTargetPosition) < AttackRange)
                    {
                        RequestPath(TargetPosition(Target) - offset, true);
                        mLastTargetPosition = TargetPosition(Target);
                    }
                    else if (!HasPath)
                    {
                        if (Pathfinder.ExistsPath(CollisionBoxCenter, TargetPosition(Target)))
                            RequestPath(TargetPosition(Target) - offset, true);
                        else
                            Target = null;
                    }
                }
            }
            else if (Aggressive && !HasPath)
            {
                AttackNext();
            }

            mTimeSinceAttack += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            mTimeSinceAttackNextLookup += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        private static Vector2 TargetPosition(IAttackable target)
        {
            return target.CollisionBoxCenter;
        }
    }
}
