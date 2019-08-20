using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    internal sealed class Notification
    {
        private List<string> mTitleLines = new List<string>();
        private List<string> mTextLines = new List<string>();
        private readonly SpriteFont mFont;
        private readonly SpriteFont mTitleFont;
        private readonly Texture2D mBackground;
        private const int Padding = 8;
        private const int TitlePadding = 15;
        private readonly int mWidth;
        public int Height { get; private set; }
        public readonly string mTitle;
        public readonly string mText;

        public Notification(string title, string text, int width)
        {
            mFont = TextureManager.Content.Load<SpriteFont>("notification");
            mTitleFont = TextureManager.Content.Load<SpriteFont>("notificationHeading");
            mWidth = width;
            mBackground = TextureManager.GetTexture("notification");
            CalculateLines(title, text, width);
            mTitle = title;
            mText = text;
        }

        public Notification(string text, int width)
        {
            mFont = TextureManager.Content.Load<SpriteFont>("notification");
            mTitleFont = TextureManager.Content.Load<SpriteFont>("notificationHeading");
            mWidth = width;
            mBackground = TextureManager.GetTexture("tooltip");
            CalculateLines("", text, width);
            mTitle = "";
            mText = text;
        }

        private void CalculateLines(string title, string text, int width)
        {
            List<string> result;
            string[] words;
            StringBuilder current = new StringBuilder();
            if (title != "")
            {
                // Calculate title lines
                result = new List<string>();

                words = title.Split(' ');
                current.Clear();
                if(words.Length > 0) current.Append(words[0]);
                foreach (var word in words.Skip(1))
                {
                    if (mTitleFont.MeasureString(current + " " + word).X <= width - 2 * Padding)
                    {
                        current.Append(" " + word);
                    }
                    else
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        current.Append(word);
                    }
                }
                if (!result.Contains(current.ToString())) result.Add(current.ToString());
                if (result.Count == 0) result.Add(current.ToString());
                mTitleLines = result;
            }
            // Calculate text lines
            result = new List<string>();

            words = text.Split(' ');
            current.Clear();
            if (words.Length > 0) current.Append(words[0]);
            foreach (var word in words.Skip(1))
            {
                if (mFont.MeasureString(current + " " + word).X <= width - 2 * Padding)
                {
                    current.Append(" " + word);
                }
                else
                {
                    result.Add(current.ToString());
                    current.Clear();
                    current.Append(word);
                }
            }
            if (!result.Contains(current.ToString())) result.Add(current.ToString());
            if (result.Count == 0) result.Add(current.ToString());
            mTextLines = result;
            Height = 2 * Padding + mTitleLines.Count * mTitleFont.LineSpacing + mTextLines.Count * mFont.LineSpacing;
            if (mTitleLines.Count > 0) Height += TitlePadding;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, null);
            var currentPosition = new Vector2(position.X + Padding, position.Y + Padding);
            spriteBatch.Draw(mBackground, new Rectangle((int)position.X, (int)position.Y, mWidth, Height), null, Color.White);
            foreach (var line in mTitleLines)
            {
                spriteBatch.DrawString(mTitleFont, line, new Vector2(currentPosition.X, currentPosition.Y), Color.White);
                currentPosition.Y += mTitleFont.LineSpacing;
            }

            if (mTitleLines.Count > 0) currentPosition.Y += TitlePadding;
            foreach (var line in mTextLines)
            {
                spriteBatch.DrawString(mFont, line, new Vector2(currentPosition.X, currentPosition.Y), Color.White);
                currentPosition.Y += mFont.LineSpacing;
            }
            spriteBatch.End();
        }
    }
}
