using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class GraveyardTreasure : BGameObject, IBuildable, IPathCollidable
    {
        private readonly List<TreasureZombie> mWatchers = new List<TreasureZombie>();
        private static readonly Random sRandom = new Random();
        private int mHeldGold;
        private int mHeldSeeds;
        private int mHeldAnimals;
        public bool HasWatchers => mWatchers.Any();

        public GraveyardTreasure(float positionX, float positionY, int loadhealth = 100) : base(TextureManager.GetTexture("graveyard_assets", 33, 33, 21), positionX, positionY, 0, 0)
        {
            Health = loadhealth;

            var level = sRandom.Next(2);
            switch (level)
            {
                case 0:
                    mHeldGold = 333 * Game1.Difficulty;
                    break;
                case 1:
                    mHeldAnimals = 2 * Game1.Difficulty;
                    break;
                case 2:
                    mHeldSeeds = 5 * Game1.Difficulty;
                    break;
                default:
                    throw new InvalidOperationException("KABOOM");
            }

            for (var i = 0; i < 2 * (Game1.Difficulty + 1); i++)
            {
                var zombie = new TreasureZombie(Position.X, Position.Y, this);
                mWatchers.Add(zombie);
                ObjectManager.Instance.Add(zombie);
            }
        }

        public void CollectResources()
        {
            if (HasWatchers) return;

            if (mHeldGold != 0) SoundManager.PlaySound("coin");
            EconomyManager.Instance.GoldAmount += mHeldGold;
            mHeldGold = 0;

            for (var i = 0; i < mHeldSeeds; i++)
            {
                var choice = sRandom.Next(3);
                switch (choice)
                {
                    case 0:
                        EconomyManager.Instance.SeedAmount1++;
                        break;
                    case 1:
                        EconomyManager.Instance.SeedAmount2++;
                        break;
                    case 2:
                        EconomyManager.Instance.SeedAmount3++;
                        break;
                    default:
                        throw new InvalidOperationException("KABOOM");
                }
            }
            mHeldSeeds = 0;

            for (var i = 0; i < mHeldAnimals; i++)
            {
                var choice = sRandom.Next(3);
                switch (choice)
                {
                    case 0:
                        ObjectManager.Instance.Add(new Chicken(Position.X, Position.Y));
                        break;
                    case 1:
                        ObjectManager.Instance.Add(new Cow(Position.X, Position.Y));
                        break;
                    case 2:
                        ObjectManager.Instance.Add(new Pig(Position.X, Position.Y));
                        break;
                    default:
                        throw new InvalidOperationException("KABOOM");
                }
            }
            mHeldAnimals = 0;

            ObjectManager.Instance.QueueRemoval(this);
        }

        public void RemoveZombie(TreasureZombie zombie)
        {
            mWatchers.Remove(zombie);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("GraveyardTreasure");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}