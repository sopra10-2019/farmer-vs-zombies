using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    internal sealed class Tooltip
    {
        private readonly string mText;
        private readonly SpriteFont mFont;
        private readonly Texture2D mBackground;
        private const int Padding = 5;
        public bool Visible { private get; set; }

        public Tooltip(string text)
        {
            mText = text;
            mFont = TextureManager.Content.Load<SpriteFont>("tooltip");
            mBackground = TextureManager.GetTexture("tooltip");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!Visible) return;
            var mousePosition = InputManager.GetCurrentInputState().mMouseWindowPosition;
            var cursor = TextureManager.GetTexture("cursor");
            var fontSize = mFont.MeasureString(mText);
            var drawPosition = new Vector2((int)mousePosition.X + cursor.Width, (int)mousePosition.Y - 20);
            spriteBatch.Draw(mBackground, new Rectangle((int)drawPosition.X, (int)drawPosition.Y, (int)fontSize.X + 2 * Padding, (int)fontSize.Y + 2 * Padding), null, Color.White);
            spriteBatch.DrawString(mFont, mText, new Vector2(drawPosition.X + Padding, drawPosition.Y + Padding), Color.White);
        }
    }
}
