using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Wheat : BGameObject, IAttackable
    {
        public enum WheatType
        {
            Wheat1,
            Wheat2,
            Corn
        }

        public WheatType Type { get; }

        // Time it takes to grow to next stage
        private static readonly Dictionary<WheatType, float> sGrowTime = new Dictionary<WheatType, float>()
        {
            { WheatType.Wheat1, 7.0f },
            { WheatType.Wheat2, 30.0f },
            { WheatType.Corn, 50.0f }
        };
        private float mTime;
        public int mStage;
        public bool Team { get; } = true;

        public Wheat(int positionX, int positionY, WheatType type, int stage = 0, int loadinghealth = 1, float time = 0.0f): base(TextureManager.GetTexture("wheat", 32, 32, 0), positionX, positionY)
        {
            Type = type;
            if (Type == WheatType.Corn)
            {
                TextureOffset = new Vector2(0, -1);
                Width = 32;
                Height = 64;
            }
            mTime = time;
            mStage = stage;

            Health = loadinghealth;
        }

        public override void Update(GameTime gameTime)
        {
            mTime += gameTime.ElapsedGameTime.Milliseconds;
            if (mStage < 3 && mTime >= sGrowTime[Type]*1000)
            {
                mTime = 0.0f;
                mStage++;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            mTexture = GetTexture();
            base.Draw(spriteBatch, camTransform);
        }

        private Texture2D GetTexture()
        {
            switch (Type)
            {
                case WheatType.Wheat1:
                    return TextureManager.GetTexture("wheat", 32, 32, mStage);
                case WheatType.Wheat2:
                    return TextureManager.GetTexture("wheat2", 32, 32, mStage);
                case WheatType.Corn:
                    return TextureManager.GetTexture("corn", 32, 64, mStage);
            }
            return TextureManager.GetTexture("wheat", 32, 32, 0);
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Wheat");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("type", Type.ToString());
            element.SetAttribute("stage", mStage.ToString());
            element.SetAttribute("time", mTime.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("health", Health.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
