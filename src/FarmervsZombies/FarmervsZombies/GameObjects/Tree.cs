using System;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Tree : BGameObject, IPathCollidable
    {
        private static readonly float sDefaultTextureOffsetX = -1;
        private static readonly float sDefaultTextureOffsetY = -3;
        private static readonly Random sRandom = new Random();
        private bool mTexturesRandomized;

        // texture concatenation needed to add tree stem and leaves textures
        public Tree(float positionX, float positionY, int loadhealh = 100) : base(TextureManager.GetTexture("tree", 96, 128, 0),
            positionX,
            positionY,
            sDefaultTextureOffsetX,
            sDefaultTextureOffsetY)
        {
            Health = loadhealh;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (mTexturesRandomized) return;
            var graveyard = ObjectManager.Instance.GetGraveyard();
            if (graveyard == null) return;
            var graveyardDistance = Vector2.Distance(Position, graveyard.Position);
            mTexture = TextureManager.GetTexture("tree", 96, 128, graveyardDistance < 40 ? sRandom.Next(2, 4) : sRandom.Next(0, 2));
            mTexturesRandomized = true;
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Tree");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("Health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}