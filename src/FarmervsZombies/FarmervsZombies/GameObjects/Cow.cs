using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Cow : BAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -0.5f;
        private const int DefaultMaxHealth = 125;


        public int ResourcesHeld => mResourcesHeld * EconomyManager.Instance.GetProfit("ActionMilk");

        public Cow(float positionX, float positionY, int loadhealth = DefaultMaxHealth, int level = 0) : base(TextureManager.GetTexture("cow_walk", 32, 64, 9),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            MaxHealth = DefaultMaxHealth;
            Mass = 8f;

            mRandomWalk = true;

            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.CowWalkUp, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.CowWalkRight, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.CowWalkDown, this);
            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.CowWalkLeft, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.CowEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.CowEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.CowEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.CowEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("cow_walk", 32, 64, 1);
            mStandTextureList[1] = TextureManager.GetTexture("cow_walk", 64, 32, 13);
            mStandTextureList[2] = TextureManager.GetTexture("cow_walk", 32, 64, 9);
            mStandTextureList[3] = TextureManager.GetTexture("cow_walk", 64, 32, 5);
            Health = loadhealth;
            mExperience = level;
            mFeedAction = "ActionPlantWheat2";
            mStarveNotificationText = "Eine Kuh hungert!";
        }

        public void Upgrade()
        {
            if (mExperience < ExperienceForLevelUp) return;
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAttackCow")) return;

            ObjectManager.Instance.Add(new AttackCow(Position.X, Position.Y));
            ObjectManager.Instance.QueueRemoval(this);
        }

        public override void Slaughter()
        {
            Slaughter("ActionSlaughterCow");
            base.Slaughter();
        }

        protected override void GenerateResources(bool sound = true)
        {
            EconomyManager.Instance.GoldIncrease("ActionMilk", sound);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Cow");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Level", mExperience.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}