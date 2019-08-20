using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Pig : BAnimal
    {
        private const float DefaultTextureOffsetX = 0;
        private const float DefaultTextureOffsetY = -1;
        private const int DefaultMaxHealth = 150;

        public Pig(float positionX, float positionY, int loadhealth = DefaultMaxHealth, int level = 0) : base(TextureManager.GetTexture("pig_walk", 32, 64, 9),
            positionX,
            positionY,
            DefaultTextureOffsetX,
            DefaultTextureOffsetY)
        {
            Mass = 10f;
            MaxHealth = DefaultMaxHealth;
            mRandomWalk = true;
            ExperienceForLevelUp = 8;

            mWalkUpAnimation = AnimationManager.GetAnimation(AnimationManager.PigWalkUp, this);
            mWalkRightAnimation = AnimationManager.GetAnimation(AnimationManager.PigWalkRight, this);
            mWalkDownAnimation = AnimationManager.GetAnimation(AnimationManager.PigWalkDown, this);
            mWalkLeftAnimation = AnimationManager.GetAnimation(AnimationManager.PigWalkLeft, this);

            mEatLeftAnimation = AnimationManager.GetAnimation(AnimationManager.PigEatLeft, this);
            mEatRightAnimation = AnimationManager.GetAnimation(AnimationManager.PigEatRight, this);
            mEatUpAnimation = AnimationManager.GetAnimation(AnimationManager.PigEatUp, this);
            mEatDownAnimation = AnimationManager.GetAnimation(AnimationManager.PigEatDown, this);

            mStandTextureList[0] = TextureManager.GetTexture("pig_walk", 32, 64, 1);
            mStandTextureList[1] = TextureManager.GetTexture("pig_walk", 64, 32, 13);
            mStandTextureList[2] = TextureManager.GetTexture("pig_walk", 32, 64, 9);
            mStandTextureList[3] = TextureManager.GetTexture("pig_walk", 64, 32, 5);

            
            Health = loadhealth;
            mExperience = level;
            mFeedAction = "ActionPlantCorn";
            mStarveNotificationText = "Ein Schwein hungert!";
        }

        public void Upgrade()
        {
            if (mExperience < ExperienceForLevelUp) return;
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAttackPig")) return;

            ObjectManager.Instance.Add(new AttackPig(Position.X, Position.Y));
            ObjectManager.Instance.QueueRemoval(this);
        }

        public override void Slaughter()
        {
            Slaughter("ActionSlaughterPig");
            base.Slaughter();
        }

        protected override void GenerateResources(bool sound = true)
        {

        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Pig");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Level", mExperience.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}