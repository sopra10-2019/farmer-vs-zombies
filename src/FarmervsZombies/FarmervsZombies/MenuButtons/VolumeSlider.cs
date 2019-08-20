using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.MenuButtons
{
    internal sealed class VolumeSlider
    {
        private readonly Texture2D mBtnImage0;
        private readonly Texture2D mBtnImage1;
        private readonly Texture2D mBtnImage2;
        private readonly Texture2D mBtnImage3;
        private readonly Texture2D mBtnImage4;
        private readonly Texture2D mBtnImage5;
        private readonly Texture2D mBtnImage6;
        private readonly Texture2D mBtnImage7;
        private readonly Texture2D mBtnImage8;
        private readonly Texture2D mBtnImage9;
        private readonly Texture2D mBtnImage10;

        private Texture2D mCurrentBtnImage;
        private readonly Point mBtnPos;
        private readonly int mBtnHeight;
        private readonly int mBtnWidth;

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mCurrentBtnImage, new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)),
                                                             Color.White);
        }

        public VolumeSlider(Point btnPos, int btnHeight, int btnWidth, Texture2D btnImage0, Texture2D btnImage1, Texture2D btnImage2,
                                    Texture2D btnImage3, Texture2D btnImage4, Texture2D btnImage5, Texture2D btnImage6, Texture2D btnImage7,
                                    Texture2D btnImage8, Texture2D btnImage9, Texture2D btnImage10)
        {
            mBtnImage0 = btnImage0;
            mBtnImage1 = btnImage1;
            mBtnImage2 = btnImage2;
            mBtnImage3 = btnImage3;
            mBtnImage4 = btnImage4;
            mBtnImage5 = btnImage5;
            mBtnImage6 = btnImage6;
            mBtnImage7 = btnImage7;
            mBtnImage8 = btnImage8;
            mBtnImage9 = btnImage9;
            mBtnImage10 = btnImage10;
            mBtnPos = btnPos;
            mBtnHeight = btnHeight;
            mBtnWidth = btnWidth;
            mCurrentBtnImage = mBtnImage10;
        }



        public void Update(InputState inputState)
        {
            var mousePosition = inputState.mMouseWindowPosition;
            if (mousePosition.X > mBtnPos.X && mousePosition.X < mBtnPos.X + mBtnWidth && SoundManager.SoundOn &&
                mousePosition.Y > mBtnPos.Y && mousePosition.Y < mBtnPos.Y + mBtnHeight && inputState.IsButtonPressed(MouseButton.LeftButton))
            {
                var pos = mousePosition.X - mBtnPos.X;

                if (pos <= mBtnWidth * 0.1)
                {
                    mCurrentBtnImage = mBtnImage1;
                    SoundManager.MasterVolume = 0.1f;
                }
                else if (pos > mBtnWidth * 0.1 && mBtnWidth * 0.2 >= pos)
                {
                    mCurrentBtnImage = mBtnImage2;
                    SoundManager.MasterVolume = 0.2f;
                }
                else if (pos > mBtnWidth * 0.2 && mBtnWidth * 0.3 >= pos)
                {
                    mCurrentBtnImage = mBtnImage3;
                    SoundManager.MasterVolume = 0.3f;
                }
                else if (pos > mBtnWidth * 0.3 && mBtnWidth * 0.4 >= pos)
                {
                    mCurrentBtnImage = mBtnImage4;
                    SoundManager.MasterVolume = 0.4f;
                }
                else if (pos > mBtnWidth * 0.4 && mBtnWidth * 0.5 >= pos)
                {
                    mCurrentBtnImage = mBtnImage5;
                    SoundManager.MasterVolume = 0.5f;
                }
                else if (pos > mBtnWidth * 0.5 && mBtnWidth * 0.6 >= pos)
                {
                    mCurrentBtnImage = mBtnImage6;
                    SoundManager.MasterVolume = 0.6f;
                }
                else if (pos > mBtnWidth * 0.6 && mBtnWidth * 0.7 >= pos)
                {
                    mCurrentBtnImage = mBtnImage7;
                    SoundManager.MasterVolume = 0.7f;
                }
                else if (pos > mBtnWidth * 0.7 && mBtnWidth * 0.8 >= pos)
                {
                    mCurrentBtnImage = mBtnImage8;
                    SoundManager.MasterVolume = 0.8f;
                }
                else if (pos > mBtnWidth * 0.8 && mBtnWidth * 0.9 >= pos)
                {
                    mCurrentBtnImage = mBtnImage9;
                    SoundManager.MasterVolume = 0.9f;
                }
                else
                {
                    mCurrentBtnImage = mBtnImage10;
                    SoundManager.MasterVolume = 1.0f;
                }

            }
            else if (!SoundManager.SoundOn)
            {
                mCurrentBtnImage = mBtnImage0;
                SoundManager.MasterVolume = 0.0f;
            }
        }
    }
}

