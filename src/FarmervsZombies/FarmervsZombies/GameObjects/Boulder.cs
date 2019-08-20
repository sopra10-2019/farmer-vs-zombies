using System;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Boulder : BGameObject, IPathCollidable
    {
        private static readonly float sDefaultTextureOffsetX = 0;
        private static readonly float sDefaultTextureOffsetY = 0;
        private static readonly Random sRandom = new Random();
        private bool mTexturesRandomized;

        public Boulder(float positionX, float positionY) : base(TextureManager.GetTexture("terrain", 32, 32, 25),
            positionX,
            positionY,
            sDefaultTextureOffsetX,
            sDefaultTextureOffsetY)
        {

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (mTexturesRandomized) return;
            var graveyard = ObjectManager.Instance.GetGraveyard();
            if (graveyard == null) return;
            var graveyardDistance = Vector2.Distance(Position, graveyard.Position);
            mTexture = TextureManager.GetTexture( graveyardDistance > 40 ? "rocks_light" : "rocks_dark", 48, 48, sRandom.Next(2, 4));
            mTexturesRandomized = true;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Boulder");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}