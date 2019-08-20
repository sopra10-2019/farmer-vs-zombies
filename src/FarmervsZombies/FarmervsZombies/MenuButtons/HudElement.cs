using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.MenuButtons
{
    sealed class HudElement
    {
        private Texture2D mTextBoxImage;
        private string mInput;
        private SpriteFont mFont;
        public readonly Rectangle mBackRect;

        public HudElement(Point textBoxPos, Point textBoxSize)
        {
            mBackRect.Location = textBoxPos;
            mBackRect.Size = textBoxSize;
            mInput = "";
        }

        public void LoadContent(ContentManager content)
        {
            mFont = content.Load<SpriteFont>("File");
            mTextBoxImage = content.Load<Texture2D>("Buttons/default");
        }

        public void ChangeText(string text)
        {
            mInput = text;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mTextBoxImage, mBackRect, Color.Aqua);
            spriteBatch.DrawString(mFont, mInput, new Vector2(mBackRect.X + 15, mBackRect.Y + 2), Color.Black);
        }
    }
}
