using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class AttackChicken : BAttackAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = 0;

        private const float ToggleCooldown = 1;
        private float mTimeSinceToggle = 1;
        public bool RapidFire { get; private set; }
        public List<Projectile> Projectiles { get; } = new List<Projectile>();
        private static readonly Texture2D sProjectileTexture = TextureManager.GetTexture("icons", 32, 32, 8);
        private const int MaxProjectiles = 3;

        public override bool HasPath => !RapidFire && base.HasPath;

        private const int DefaultMaxHealth = 125;

        public AttackChicken(float positionX, float positionY, int loadhealth = DefaultMaxHealth, bool team = true) : base(TextureManager.GetTexture("attack_chicken_walk", 32, 32, 12),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            AttackRange = 3f;
            Mass = 0.5f;
            MaxHealth = DefaultMaxHealth;

            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenWalkLeft, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenWalkRight, this);
            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenWalkUp, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenWalkDown, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackChickenEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("attack_chicken_walk", 32, 32, 0);
            mStandTextureList[1] = TextureManager.GetTexture("attack_chicken_walk", 32, 32, 12);
            mStandTextureList[2] = TextureManager.GetTexture("attack_chicken_walk", 32, 32, 8);
            mStandTextureList[3] = TextureManager.GetTexture("attack_chicken_walk", 32, 32, 4);
            Health = loadhealth;
            Team = team;
            mFeedAction = "ActionPlantWheat1";
            mStarveNotificationText = "Ein Hühnchen hungert!";
        }

        public override void Slaughter()
        {
            EconomyManager.Instance.GoldIncrease("ActionSlaughterAttackChicken");
            base.Slaughter();
        }

        public void SwitchAttackMode()
        {
            if (mTimeSinceToggle < ToggleCooldown) return;
            RapidFire = !RapidFire;
            AttackRange *= RapidFire ? 1.75f : 0.75f;
            AttackCooldown *= RapidFire ? 0.75f : 1.75f;
            AggroRange = RapidFire ? 6f : 5f;

            if (RapidFire)
            {
                InputManager.AnyActionEvent -= MoveToTarget;
                DeletePath();
            }
            else
            {
                InputManager.AnyActionEvent += MoveToTarget;
            }
        }

        protected override void AttackNext(bool needsPath = true)
        {
            base.AttackNext(!RapidFire);
        }

        protected override void Attack()
        {
            var rotation = -Math.Atan2(Target.Position.Y - CenterPosition.Y, Target.Position.X - CenterPosition.X);
            var nextDirection = new Vector2((float)Math.Cos(rotation), -(float)Math.Sin(rotation));
            Projectiles.Add(new Projectile(sProjectileTexture, CenterPosition.X, CenterPosition.Y, nextDirection));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            foreach (var projectile in Projectiles)
            {
                projectile.Update(gameTime);
            }

            if (Projectiles.Count > MaxProjectiles)
            {
                Projectiles.RemoveAt(0);
            }

            mTimeSinceToggle += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            base.Draw(spriteBatch, camTransform);

            foreach (var projectile in Projectiles)
            {
                projectile.Draw(spriteBatch, camTransform);
            }

            if (Selected && RapidFire)
            {
                var texture = TextureManager.GetTexture("tower_radius_green");
                spriteBatch.Draw(texture,
                    new Rectangle((int)((Position.X - AttackRange + Width / 64.0f) * 32), (int)((Position.Y - AttackRange) * 32), (int)AttackRange * 64, (int)AttackRange * 64),
                    null,
                    mBackgroundColour,
                    0.0f,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0.5f);
            }
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("AttackChicken");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}