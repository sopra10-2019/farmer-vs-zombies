using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Farmhouse : BGameObject, IBuildable, IPathCollidable, IAttackable
    {
        public bool Team { get; } = true;
        private const int DefaultMaxHealth = 500;


        public Farmhouse(float positionX, float positionY, int loadhealth = DefaultMaxHealth) : base(TextureManager.GetTexture("farm_mid"), positionX, positionY, 0, -1, 4, 3, 2, 1.5f)
        {
            Defense = 125;
            Height = 128;
            Width = 142;
            Health = loadhealth;
            mDrawLiveBarMySelf = true;
            mLiveBar.SetOffset(1.5f, 0f);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("FarmerHouse");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            int health = (int)Health;
            element.SetAttribute("Health", health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
