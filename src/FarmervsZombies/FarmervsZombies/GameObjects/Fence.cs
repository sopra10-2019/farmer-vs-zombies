using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Fence : BGameObject, IBuildable, IPathCollidable, IAttackable
    {
        private static readonly List<Texture2D> sFenceTexturesLevel1 = TextureManager.GetList("fence", 32, 32);
        private static readonly List<Texture2D> sFenceTexturesLevel2 = TextureManager.GetList("fence2", 32, 32);
        public bool Team { get; } = true;

        public int Level { get; private set; } = 1;
        private const float MaxHealthLevel1 = 120;
        private const float MaxHealthLevel2 = 250;


        public Fence(float positionX, float positionY) : base(sFenceTexturesLevel1[15], positionX, positionY)
        {
            switch (Level)
            {
                case 1:
                    MaxHealth = MaxHealthLevel1;
                    Level = 1;
                    break;
                case 2:
                    MaxHealth = MaxHealthLevel2;
                    Level = 2;
                    break;
            }
        }

        public Fence(float positionX, float positionY, int level = 1, int loadhealth = 100) : base(sFenceTexturesLevel1[15], positionX, positionY)
        {
            if (level == 2) Upgrade();
            Health = loadhealth;
        }

        public override void Update(GameTime gameTime)
        {
            var top = ObjectManager.Instance.CheckTile(new Vector2(Position.X, Position.Y - 1), typeof(Fence));
            var right = ObjectManager.Instance.CheckTile(new Vector2(Position.X + 1, Position.Y), typeof(Fence));
            var bottom = ObjectManager.Instance.CheckTile(new Vector2(Position.X, Position.Y + 1), typeof(Fence));
            var left = ObjectManager.Instance.CheckTile(new Vector2(Position.X - 1, Position.Y), typeof(Fence));
            var val = (top.Count > 0, right.Count > 0, bottom.Count > 0, left.Count > 0);

            var texture = Level == 1 ? GetFenceTexture(sFenceTexturesLevel1, val) : GetFenceTexture(sFenceTexturesLevel2, val);
            mTexture = texture ?? mTexture;

            if (Health < 100)
            {
                mDrawLiveBarMySelf = true;
            }
        }

        public static Texture2D GetFenceTexture(List<Texture2D> fenceTextures, (bool, bool, bool, bool) val)
        {
            switch (val)
            {
                case var t when t == (false, true, false, false):
                    return fenceTextures[0];
                case var t when t == (false, true, false, true):
                    return fenceTextures[1];
                case var t when t == (false, false, false, true):
                    return fenceTextures[2];
                case var t when t == (true, false, false, false):
                    return fenceTextures[3];
                case var t when t == (true, false, true, false):
                    return fenceTextures[4];
                case var t when t == (false, false, true, false):
                    return fenceTextures[5];
                case var t when t == (false, true, true, false):
                    return fenceTextures[6];
                case var t when t == (false, true, true, true):
                    return fenceTextures[7];
                case var t when t == (false, false, true, true):
                    return fenceTextures[8];
                case var t when t == (true, true, true, false):
                    return fenceTextures[9];
                case var t when t == (true, true, true, true):
                    return fenceTextures[10];
                case var t when t == (true, false, true, true):
                    return fenceTextures[11];
                case var t when t == (true, true, false, false):
                    return fenceTextures[12];
                case var t when t == (true, true, false, true):
                    return fenceTextures[13];
                case var t when t == (true, false, false, true):
                    return fenceTextures[14];
                case var t when t == (false, false, false, false):
                    return fenceTextures[15];
                default:
                    return null;
            }
        }

        public bool Upgrade()
        {
            if (Level == 2) return false;
            Level = 2;
            MaxHealth = MaxHealthLevel2;
            Health = Health + MaxHealthLevel2 - MaxHealthLevel1;
            return true;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Fence");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("level", Level.ToString(CultureInfo.InvariantCulture));

            xml.AppendChild(element);
        }
    }
}