using System.Globalization;
using System.Xml;
using FarmervsZombies.AI;
using FarmervsZombies.Managers;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Graveyard : BGameObject, IBuildable, IPathCollidable, IAttackable
    {
        public bool Team { get; } = false;

        public Graveyard(float positionX, float positionY, int loadhealth = 500) : base(TextureManager.GetTexture("graveyard_assets", 33, 33, 5), positionX, positionY, 0, 0)
        {
            Health = loadhealth;
            Defense = 200;
        }

        protected override void Death()
        {
            Ai.SpawnNecromancer(Position);
            base.Death();
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Graveyard");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
