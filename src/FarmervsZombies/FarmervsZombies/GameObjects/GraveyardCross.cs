using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class GraveyardCross : BGameObject, IBuildable, IPathCollidable, IAttackable
    {
        public bool Team { get; } = false;

        public GraveyardCross(float positionX, float positionY, int loadhealth = 100) : base(TextureManager.GetTexture("graveyard_assets", 33, 33, 3), positionX, positionY, 0, 0)
        {
            Health = loadhealth;
            Defense = 50;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("GraveyardCross");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
