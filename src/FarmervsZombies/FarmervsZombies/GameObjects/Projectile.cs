using System;
using System.Globalization;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Projectile : BGameObject
    {
        private Vector2 mDirection;
        private const int MillisecondsPerFrame = 2;
        private const float Speed = 8.0f;
        private float mTimeSinceLastFrame;

        public Projectile(Texture2D texture, float positionX, float positionY, Vector2 direction)
            : base(texture, positionX, positionY)
        {
            mDirection = direction;
            if (mDirection != Vector2.Zero && mDirection.Length() < 1) mDirection.Normalize();
            Width = 20;
            Height = 20;
        }

        public override void Update(GameTime gameTime)
        {
            mTimeSinceLastFrame += gameTime.ElapsedGameTime.Milliseconds;
            if (mTimeSinceLastFrame <= MillisecondsPerFrame) return;
            mTimeSinceLastFrame = 0;
            Position += mDirection * Speed * MillisecondsPerFrame / 32;
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            if (mTexture == null)
            {
                throw new InvalidOperationException($"{GetType().Name} texture was null.");
            }

            spriteBatch.Draw(mTexture,
                new Vector2(Position.X * 32, Position.Y * 32),
                null,
                Color.White,
                (float) (Math.Atan2(mDirection.Y, mDirection.X) + Math.PI / 4),
                new Vector2(mTexture.Width / 2f, mTexture.Height / 2f),
                (float)Width/mTexture.Width,
                SpriteEffects.None,
                Position.Y / 100);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Projectile");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
