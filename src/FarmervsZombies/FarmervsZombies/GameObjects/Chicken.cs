using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Chicken : BAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = 0;
        private const int DefaultMaxHealth = 100;


        public int ResourcesHeld => mResourcesHeld * EconomyManager.Instance.GetProfit("ActionEgg");

        public Chicken(float positionX, float positionY, int loadhealth = DefaultMaxHealth, int level = 0) : base(TextureManager.GetTexture("chicken_walk", 32, 32, 12),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            mRandomWalk = true;
                        
            MaxHealth = DefaultMaxHealth;
            Mass = 0.25f;

            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenWalkLeft, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenWalkRight, this);
            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenWalkUp, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenWalkDown, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.ChickenEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("chicken_walk", 32, 32, 0);
            mStandTextureList[1] = TextureManager.GetTexture("chicken_walk", 32, 32, 12);
            mStandTextureList[2] = TextureManager.GetTexture("chicken_walk", 32, 32, 8);
            mStandTextureList[3] = TextureManager.GetTexture("chicken_walk", 32, 32, 4);
            Health = loadhealth;
            mExperience = level;
            mFeedAction = "ActionPlantWheat1";
            mStarveNotificationText = "Ein Hühnchen hungert!";
        }

        public void Upgrade()
        {
            if (mExperience < ExperienceForLevelUp) return;
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAttackChicken")) return;

            ObjectManager.Instance.Add(new AttackChicken(Position.X, Position.Y));
            ObjectManager.Instance.QueueRemoval(this);
        }

        public override void Slaughter()
        {
            Slaughter("ActionSlaughterChicken");
            base.Slaughter();
        }

        protected override void GenerateResources(bool sound = true)
        {
            EconomyManager.Instance.GoldIncrease("ActionEgg", sound);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Chicken");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Level", mExperience.ToString(CultureInfo.InvariantCulture));

            xml.AppendChild(element);
        }
    }
}