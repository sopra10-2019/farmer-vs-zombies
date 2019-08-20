using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class AttackPig : BAttackAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -1;

        private const float TauntCooldown = 20;
        private const float TauntDuration = 5;
        private const float TauntRange = 5;
        private float mTimeSinceTaunt;
        private bool mTauntActive;

        private const int DefaultMaxHealth = 1250;
        
        public AttackPig(float positionX, float positionY, int loadhealth = DefaultMaxHealth) : base(TextureManager.GetTexture("attack_pig_walk", 32, 64, 9),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            Mass = 15f;
            Defense *= 6;
            AttackDamage = 30f;
            MaxHealth = DefaultMaxHealth;

            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigWalkUp, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigWalkRight, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigWalkDown, this);
            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigWalkLeft, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.AttackPigEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("attack_pig_walk", 32, 64, 1);
            mStandTextureList[1] = TextureManager.GetTexture("attack_pig_walk", 64, 32, 13);
            mStandTextureList[2] = TextureManager.GetTexture("attack_pig_walk", 32, 64, 9);
            mStandTextureList[3] = TextureManager.GetTexture("attack_pig_walk", 64, 32, 5);
            Health = loadhealth;
            mFeedAction = "ActionPlantCorn";
            mStarveNotificationText = "Ein Schwein hungert!";
        }

        public override void Slaughter()
        {
            EconomyManager.Instance.GoldIncrease("ActionSlaughterAttackPig");
            base.Slaughter();
        }

        public void Taunt()
        {
            if (!Selected || mTimeSinceTaunt < TauntCooldown) return;

            foreach (var current in ObjectManager.Instance.TargetsInRange(Position, TauntRange))
            {
                if (Team == current.Team) continue;

                if (current is ITauntable tauntable)
                {
                    tauntable.Taunt(this);
                }
            }

            Defense *= 2;
            Speed /= 4;
            mTimeSinceTaunt = 0;
            mTauntActive = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (mTauntActive && mTimeSinceTaunt >= TauntDuration)
            {
                Defense /= 2;
                Speed *= 4;
                mTauntActive = false;
            }

            mTimeSinceTaunt += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Cow");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}