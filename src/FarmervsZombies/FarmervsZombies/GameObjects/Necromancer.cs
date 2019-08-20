using System;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Necromancer : Zombie
    {
        private const float AbilityCooldown = 20;
        private const float AbilityRange = 7.5f;
        private const float AbilityDamage = 250;
        private const float AbilityCastTime = 3;
        private const int DefaultMaxHealth = 125;

        private float mTimeSinceAbility = float.PositiveInfinity;
        private float mTimeSinceCast;
        private bool mIsCasting;
        private readonly Texture2D mAbilityTexture;

        public Necromancer(float positionX, float positionY, int loadhealth = DefaultMaxHealth)
            : base(positionX,
                positionY,
                AnimationManager.NecromancerWalkLeft,
                AnimationManager.NecromancerWalkRight,
                AnimationManager.NecromancerWalkUp,
                AnimationManager.NecromancerWalkDown,
                false)
        {
            mTexture = TextureManager.GetTexture("necromancer_big_walk", 32, 64, 1);
            mAbilityTexture = TextureManager.GetTexture("circle");
            MaxHealth = DefaultMaxHealth;

            AttackDamage = Game1.Difficulty == 3 ? 100 : 50;
            AttackRange = 2;
            Defense *= 4;
            MaxHealth = 200 + 100 * Game1.Difficulty;
            Mass = 1.2f;
            Health = loadhealth;

            Target = ObjectManager.Instance.GetFarmer();
        }

        private void StartCast()
        {
            if (mTimeSinceAbility < AbilityCooldown) return;

            mIsCasting = true;
            mTimeSinceCast = 0;
            Speed /= 4;
        }

        private void CastAbility()
        {
            SoundManager.PlaySound("explosion");
            // Explosion animation
            foreach (var target in ObjectManager.Instance.TargetsInRange(CollisionBoxCenter, AbilityRange))
            {
                if (Team == target.Team) continue;
                target.Damage(AbilityDamage);
            }

            mIsCasting = false;
            mBackgroundColour = Color.White;
            mTimeSinceAbility = 0;
            Speed *= 4;
        }

        protected override void Death()
        {
            SoundManager.PlaySound("necro_death");
            Game1.sStatistics.Win();
            Game1.AiDied = true;
        }

        protected override Texture2D GetStandingTexture()
        {
            switch (mDirection)
            {
                case 0:
                    return TextureManager.GetTexture("necromancer_big_walk",  32, 64, 3);
                case 1:
                    return TextureManager.GetTexture("necromancer_big_walk",  32, 64, 6);
                case 2:
                    return TextureManager.GetTexture("necromancer_big_walk",  32, 64, 0);
                case 3:
                    return TextureManager.GetTexture("necromancer_big_walk",  32, 64, 9);
                default:
                    throw new InvalidOperationException("Invalid direction for necromancer.");
            }
        }

        protected override void MakeSound(Farmer farmer)
        {
            if (farmer != null && Vector2.Distance(Position, farmer.Position) > 30 && sRandom.NextDouble() < 0.001)
            {
                SoundManager.PlaySound("evil_laugh", 0.3f);
            }
            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (mIsCasting)
            {
                mBackgroundColour = Color.Lerp(Color.White, Color.Crimson, mTimeSinceCast / AbilityCastTime);

                if (mTimeSinceCast >= AbilityCastTime)
                {
                    CastAbility();
                }
            }
            else if (HasTarget && Vector2.Distance(CollisionBoxCenter, Target.CollisionBoxCenter) < AbilityRange - 1.5f)
            {
                StartCast();
            }

            mTimeSinceAbility += gameTime.ElapsedGameTime.Milliseconds / 1000f;
            mTimeSinceCast += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            base.Draw(spriteBatch, camTransform);

            if (!mIsCasting) return;
            if (mTimeSinceCast < 2f)
            {
                var texture = TextureManager.GetTexture("tower_radius_red");
                spriteBatch.Draw(texture,
                    new Rectangle((int)((Position.X - AbilityRange + Width / 64.0f) * 32),
                        (int)((Position.Y - AbilityRange) * 32),
                        (int)AbilityRange * 64,
                        (int)AbilityRange * 64),
                    null,
                    mBackgroundColour,
                    0.0f,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0.5f);
            }
            else
            {
                var lerp = (float)Math.Sin(10 * Math.PI * mTimeSinceCast - Math.PI / 2) / 2f + 0.5f;
                spriteBatch.Draw(mAbilityTexture,
                    new Rectangle((int)((Position.X - AbilityRange + Width / 64.0f) * 32),
                        (int)((Position.Y - AbilityRange) * 32),
                        (int)AbilityRange * 64,
                        (int)AbilityRange * 64),
                    null,
                    Color.Lerp(Color.Black, Color.DeepSkyBlue, lerp),
                    0.0f,
                    new Vector2(0, 0),
                    SpriteEffects.None,
                    0.5f);
            }
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Necromancer");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}