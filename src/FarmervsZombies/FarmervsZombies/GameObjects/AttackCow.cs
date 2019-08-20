using System;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class AttackCow : BAttackAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -0.5f;

        private const float RageCooldown = 15;
        private const float RageDuration = 6;
        private float mTimeSinceRage = 15;
        private bool mRageActive;

        private const int DefaultMaxHealth = 750;

        public AttackCow(float positionX, float positionY, int loadhealth = DefaultMaxHealth) : base(TextureManager.GetTexture("attack_cow_walk", 32, 64, 9),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            Mass = 12f;
            MaxHealth = DefaultMaxHealth;
            AttackRange = 1.2f;
            AttackDamage = 45f;
            Defense *= 3;
        
            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowWalkUp, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowWalkRight, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowWalkDown, this);
            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowWalkLeft, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackCowEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("attack_cow_walk", 32, 64, 1);
            mStandTextureList[1] = TextureManager.GetTexture("attack_cow_walk", 64, 32, 13);
            mStandTextureList[2] = TextureManager.GetTexture("attack_cow_walk", 32, 64, 9);
            mStandTextureList[3] = TextureManager.GetTexture("attack_cow_walk", 64, 32, 5);
            Health = loadhealth;
            mFeedAction = "ActionPlantWheat2";
            mStarveNotificationText = "Eine Kuh hungert!";
        }

        public override void Slaughter()
        {
            EconomyManager.Instance.GoldIncrease("ActionSlaughterAttackCow");
            base.Slaughter();
        }

        public void Enrage()
        {
            if (mTimeSinceRage < RageCooldown) return;

            AttackDamage *= 1.5f;
            AttackCooldown /= 1.5f;
            Speed *= 2;
            Defense /= 2;
            mBackgroundColour = Color.Red;
            mTimeSinceRage = 0;
            mRageActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (mRageActive && mTimeSinceRage >= RageDuration)
            {
                AttackDamage /= 1.5f;
                AttackCooldown *= 1.5f;
                Speed /= 2;
                Defense *= 2;
                mBackgroundColour = Color.White;
                mRageActive = false;
            }

            if (mRageActive)
            {
                var lerp = (float)Math.Sin(2 * Math.PI * mTimeSinceRage - Math.PI / 2) / 2f + 0.5f;
                mBackgroundColour = Color.Lerp(Color.Red, Color.DarkRed, lerp);
            }

            mTimeSinceRage += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("AttackCow");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}