using System;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.MenuButtons
{
    internal class MenuButton
    {
        protected readonly Texture2D mBtnImage;
        private readonly Texture2D mBtnHoverImage;
        private readonly Texture2D mBtnClickImage;
        protected Point mBtnPos;
        protected int mBtnHeight;
        protected int mBtnWidth;
        protected BtnStates mBtnState;

        protected string mBtnText;
        protected SpriteFont mSpriteFont;

        private readonly int mTxtPosX;
        private readonly int mTxtPosY;

        public bool IsActive { private get; set; } = true;

        protected enum BtnStates
        {
            NotClicked,
            Clicked,
            OnHover
        }

        protected MenuButton(Texture2D btnImage, Point btnPos, int btnHeight, int btnWidth, string btnText, int txtposX = 0, int txtposY = 3)
        {
            mBtnImage = btnImage;
            mBtnPos = btnPos;
            mBtnHeight = btnHeight;
            mBtnWidth = btnWidth;
            mBtnState = BtnStates.NotClicked;
            mBtnText = btnText;
            mTxtPosX = txtposX;
            mTxtPosY = txtposY;

        }

        public MenuButton(Point btnPos, int btnHeight, int btnWidth, string btnText, int txtposX = 0, int txtposY = 3)
        {
            mBtnImage = TextureManager.GetTexture("buttons", 96, 32, 6);
            mBtnHoverImage = TextureManager.GetTexture("buttons", 96, 32, 8);
            mBtnClickImage = TextureManager.GetTexture("buttons", 96, 32, 10);
            mBtnPos = btnPos;
            mBtnHeight = btnHeight;
            mBtnWidth = btnWidth;
            mBtnState = BtnStates.NotClicked;
            mBtnText = btnText;
            mTxtPosX = txtposX;
            mTxtPosY = txtposY;
        }

        public void LoadContent(ContentManager content)
        {
            mSpriteFont = content.Load<SpriteFont>("File");
        }

        public void SetPosition(Point pos)
        {
            mBtnPos = pos;
        }

        public string GetInput()
        {
            return mBtnText;
        }

        public bool Update(InputState inputState)
        {
            if (!IsActive) return false;

            var mousePosition = inputState.mMouseWindowPosition;
            switch (inputState.IsButtonPressed(MouseButton.LeftButton))
            {
                case false when mousePosition.X > mBtnPos.X && mousePosition.X < mBtnPos.X + mBtnWidth &&
                                               mousePosition.Y > mBtnPos.Y && mousePosition.Y < mBtnPos.Y + mBtnHeight:
                    mBtnState = BtnStates.OnHover;
                    return false;
                case true when mousePosition.X > mBtnPos.X && mousePosition.X < mBtnPos.X + mBtnWidth &&
                                              mousePosition.Y > mBtnPos.Y && mousePosition.Y < mBtnPos.Y + mBtnHeight
                    && inputState.IsButtonCurrentlyPressed(MouseButton.LeftButton):
                    mBtnState = BtnStates.Clicked;
                    return true;
                default:
                    mBtnState = BtnStates.NotClicked;
                    return false;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsActive)
            {
                switch (mBtnState)
                {
                    case BtnStates.OnHover:
                        spriteBatch.Draw(mBtnHoverImage,
                            new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)),
                            Color.White);
                        break;
                    case BtnStates.Clicked:
                        spriteBatch.Draw(mBtnClickImage,
                            new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)),
                            Color.White);
                        break;
                    case BtnStates.NotClicked:
                        spriteBatch.Draw(mBtnImage,
                            new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)),
                            Color.White);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                spriteBatch.Draw(mBtnHoverImage,
                    new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)),
                    Color.Gray);
            }

            if (mTxtPosX == 0 && mTxtPosY == 3)
            {
                spriteBatch.DrawString(mSpriteFont, mBtnText, new Vector2(mBtnPos.X + mBtnWidth / 10, mBtnPos.Y + mBtnHeight / 3), Color.Black);
            }
            else
            {
                spriteBatch.DrawString(mSpriteFont, mBtnText, new Vector2(mBtnPos.X + mBtnWidth / mTxtPosX, mBtnPos.Y + mBtnHeight / mTxtPosY), Color.Black);
            }
            
        }
    }
}