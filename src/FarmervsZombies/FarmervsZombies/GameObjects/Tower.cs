using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Tower : BGameObject, IBuildable, IPathCollidable, IAttackable
    {
        private const int MaxProjectiles = 25;

        public int Level { get; private set; } = 1;
        private const float RadiusLevel1 = 5.0f;
        private const float FireRateLevel1 = 4;
        private const float ProjectileDamageLevel1 = 4.5f;
        private const float MaxHealthLevel1 = 100;
        private const float RadiusLevel2 = 7.5f;
        private const float FireRateLevel2 = 4.5f;
        private const float ProjectileDamageLevel2 = 5.5f;
        private const float MaxHealthLevel2 = 120;


        public float Radius
        {
            get
            {
                if (Level == 1) return RadiusLevel1;
                if (Level == 2) return RadiusLevel2;
                return RadiusLevel1;
            }
        }

        private float FireRate
        {
            get
            {
                if (Level == 1) return FireRateLevel1;
                if (Level == 2) return FireRateLevel2;
                return FireRateLevel1;
            }
        }

        public float ProjectileDamage
        {
            get
            {
                if (Level == 1) return ProjectileDamageLevel1;
                if (Level == 2) return ProjectileDamageLevel2;
                return ProjectileDamageLevel1;
            }
        }

        private readonly float mProjectileSpawnCooldownInSeconds;
        private float mTimeSinceLastProjectileSpawn;
        public List<Projectile> Projectiles { get; }
        private readonly Texture2D mProjectileTexture;
        private BGameObject mTarget;
        private Vector2 mTargetPosition;
        private bool mTargetToFar = true;
        private bool mForceTargets;
        private List<BGameObject> mForcedTargets = new List<BGameObject>();

        public enum FireMode
        {
            Closest,
            Weakest,
            Strongest
        }

        private FireMode Mode { get; set; } = FireMode.Closest;

        public bool Team { get; } = true;

        public bool Upgradeable => Level == 1;

        public Tower(float positionX, float positionY) : base(TextureManager.GetTexture("tower"), positionX, positionY, 0, -1)
        {
            mProjectileTexture = TextureManager.GetTexture("icons", 32, 32, 29);
            Projectiles = new List<Projectile>();
            Height = 64;
            mProjectileSpawnCooldownInSeconds = 1 / FireRate;
            switch (Level)
            {
                case 1:
                    MaxHealth = MaxHealthLevel1;
                    break;
                case 2:
                    MaxHealth = MaxHealthLevel2;
                    break;
            }
        }

        public Tower(float positionX, float positionY, int level, int loadhealth) : this(positionX, positionY)
        {
            if (level == 2) Upgrade();
            Health = loadhealth;
        }

        public override void Update(GameTime gameTime)
        {
            var removeQueue = new List<BGameObject>();
            foreach (var target in mForcedTargets)
            {
                if (target == null || target.Health < 0) removeQueue.Add(target);
            }

            foreach (var target in removeQueue)
            {
                mForcedTargets.Remove(target);
            }

            if (mForcedTargets.Count == 0)
            {
                mForceTargets = false;
            }

            // Create projectiles
            mTimeSinceLastProjectileSpawn += gameTime.ElapsedGameTime.Milliseconds;
            if (mTimeSinceLastProjectileSpawn > mProjectileSpawnCooldownInSeconds * 1000 && !mTargetToFar && mTarget != null)
            {
                var shootPosition = new Vector2
                    {
                        X = Position.X + (Width / 2f / 32) + TextureOffset.X,
                        Y = Position.Y + (Height / 2f / 32) + TextureOffset.Y
                    };
                    var rotation = -Math.Atan2(mTargetPosition.Y - shootPosition.Y, mTargetPosition.X - shootPosition.X);

                    var nextDirection = new Vector2((float)Math.Cos(rotation), -(float)Math.Sin(rotation));
                    Projectiles.Add(new Projectile(mProjectileTexture, shootPosition.X, shootPosition.Y, nextDirection));
                    mTimeSinceLastProjectileSpawn = 0;
            }

            foreach (var projectile in Projectiles)
            {
                projectile.Update(gameTime);
            }

            if (Projectiles.Count > MaxProjectiles)
            {
                Projectiles.RemoveAt(0);
            }

            if (mForceTargets)
            {
                SetTargets(mForcedTargets);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            base.Draw(spriteBatch, camTransform);
            foreach (var projectile in Projectiles)
            {
                projectile.Draw(spriteBatch, camTransform);
            }

            // Draw radius circle
            if (Selected)
            {
                Texture2D texture = null;
                switch (Mode)
                {
                    case FireMode.Closest:
                        texture = TextureManager.GetTexture("tower_radius_red");
                        break;
                    case FireMode.Weakest:
                        texture = TextureManager.GetTexture("tower_radius_green");
                        break;
                    case FireMode.Strongest:
                        texture = TextureManager.GetTexture("tower_radius_blue");
                        break;
                }

                if (texture == null) return;
                spriteBatch.Draw(texture,
                    new Rectangle((int)((Position.X - Radius + Width/64.0f) * 32), (int)((Position.Y - Radius) * 32), (int)Radius*64, (int)Radius*64),
                    null,
                    mBackgroundColour,
                    0.0f,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0.5f);
            }
        }

        public void NextMode()
        {
            Mode = GetNextMode();
        }

        public FireMode GetNextMode()
        {
            switch (Mode)
            {
                case FireMode.Closest:
                    return FireMode.Weakest;
                case FireMode.Weakest:
                    return FireMode.Strongest;
                case FireMode.Strongest:
                     return FireMode.Closest;
                default:
                    return FireMode.Closest;
            }

        }

        public void SetTargets(IEnumerable<BGameObject> objects, bool force = false)
        {
            if (force)
            {
                mForceTargets = true;
                mForcedTargets = (List<BGameObject>)objects;
                if (mForcedTargets.Count == 0) mForceTargets = false;
            }

            IEnumerable<BGameObject> enemies;
            var anyForcedInRange = false;
            foreach (var obj in mForcedTargets)
            {
                if (Vector2.Distance(new Vector2(obj.Position.X + obj.Width / 2.0f / 32 + obj.TextureOffset.X,
                            obj.Position.Y + obj.Height / 2.0f / 32 + obj.TextureOffset.Y),
                        new Vector2(Position.X + Width / 2.0f / 32, Position.Y)) <= Radius)
                {
                    anyForcedInRange = true;
                    break;
                }
            }

            if (mForceTargets && anyForcedInRange) enemies = mForcedTargets;
            else enemies = objects;
            switch (Mode)
            {
                case FireMode.Closest:
                    BGameObject closestObject = null;
                    var closestDistance = float.PositiveInfinity;
                    foreach (var obj in enemies)
                    {
                        var position = new Vector2(obj.Position.X + obj.Width / 2.0f / 32 + obj.TextureOffset.X, obj.Position.Y + obj.Height / 2.0f / 32 + obj.TextureOffset.Y);
                        var centerPosition = new Vector2(Position.X + Width / 2.0f / 32, Position.Y);
                        var distance = Vector2.Distance(centerPosition, position);
                        if (distance < closestDistance && obj.Health > 0)
                        {
                            closestObject = obj;
                            closestDistance = distance;
                        }
                    }
                    SetTarget(closestObject);
                    break;
                case FireMode.Weakest:
                    BGameObject weakestObject = null;
                    var weakestHealth = float.PositiveInfinity;
                    foreach (var obj in enemies)
                    {
                        if (obj.Health < weakestHealth && obj.Health > 0)
                        {
                            weakestObject = obj;
                            weakestHealth = obj.Health;
                        }
                    }
                    SetTarget(weakestObject);
                    break;
                case FireMode.Strongest:
                    BGameObject strongestObject = null;
                    var strongestHealth = float.NegativeInfinity;
                    foreach (var obj in enemies)
                    {
                        if (obj.Health > strongestHealth && obj.Health > 0)
                        {
                            strongestObject = obj;
                            strongestHealth = obj.Health;
                        }
                    }
                    SetTarget(strongestObject);
                    break;
            }
        }

        public void Upgrade()
        {
            if (Level == 2) return;
            Level = 2;
            MaxHealth = MaxHealthLevel2;
            Health = Health + MaxHealthLevel2 - MaxHealthLevel1;
            mTexture = TextureManager.GetTexture("tower2");
        }

        /// <summary>Sets the target of the tower.</summary>
        /// <param name="target">The Target.</param>
        private void SetTarget(BGameObject target)
        {
            mTarget = target;
            if (mTarget == null) return;
            var targetPosition = new Vector2(target.Position.X + target.Width / 2.0f / 32 + target.TextureOffset.X, target.Position.Y + target.Height / 2.0f / 32 + target.TextureOffset.Y);
            var centerPosition = new Vector2(Position.X + Width / 2.0f / 32, Position.Y);
            var targetDistance = Vector2.Distance(centerPosition, targetPosition);
            mTargetPosition = targetPosition;
            mTargetToFar = targetDistance > Radius;
        }

        override
        public void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Tower");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("level", Level.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}