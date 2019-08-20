using System.Collections.Generic;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace FarmervsZombies.MenuButtons
{
    sealed class LiveBar
    {
        private readonly List<Texture2D> mBars;
        private Texture2D mCurrentBar;
        private Vector2 mPos;
        private Vector2 mOffset;

        public LiveBar()
        {
            mBars = new List<Texture2D>();
            mOffset = Vector2.Zero;
        }

        public void LoadContent()
        {
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 0));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 1));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 2));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 3));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 4));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 5));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 6));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 7));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 8));
            mBars.Add(TextureManager.GetTexture("livebar", 50, 6, 9));

            // Since all animals start with full live the livebar will be full when fist initialized
            mCurrentBar = mBars[9];

            mPos = new Vector2();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mCurrentBar, new Vector2((mPos.X + mOffset.X) * 32 - 10, (mPos.Y + mOffset.Y) * 32 - 35), Color.AliceBlue);

        }

        public void Update(float health, float maxHealth, Vector2 pos, float y = 0)
        {
            for (var i = 0; i <= 9; i++)
            {
                if (health / maxHealth > (float)i / 10 && health / maxHealth <= (float)(i + 1) / 10)
                {
                    mCurrentBar = mBars[i];
                }
            }
            mPos = new Vector2(pos.X, pos.Y + y);
        }

        public void SetOffset(float offsetX, float offsetY)
        {
            mOffset = new Vector2(offsetX, offsetY);
        }
    }
}
