using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.MenuButtons
{
    internal sealed class Switch : MenuButton
    {
        private readonly Texture2D mBtnImageTwo;
        private Texture2D mCurrentBtnImage;
        private readonly string mBtnText1;
        private string mCurrentText;
        public void ChangeSwitch()
        {
            mCurrentBtnImage = mCurrentBtnImage == mBtnImage ? mBtnImageTwo : mBtnImage;
            mCurrentText = mCurrentBtnImage == mBtnImage ? mBtnText : mBtnText1;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(mCurrentBtnImage, new Rectangle(mBtnPos, new Point(mBtnWidth, mBtnHeight)), Color.White);

            spriteBatch.DrawString(mSpriteFont, mCurrentText, new Vector2(mBtnPos.X + mBtnWidth / 10, mBtnPos.Y + mBtnHeight / 3), Color.Black);
        }

        public Switch(Texture2D btnImage, Texture2D btnImageTwo,Point btnPos, int btnHeight, int btnWidth, string btnText, string btnText1) : base(btnImage, btnPos, btnHeight, btnWidth, btnText)
        {
            mBtnPos = btnPos;
            mBtnHeight = btnHeight;
            mBtnWidth = btnWidth;
            mBtnState = BtnStates.NotClicked;
            mBtnText = btnText;
            mBtnImageTwo = btnImageTwo;
            mCurrentBtnImage = mBtnImage;
            mBtnText1 = btnText1;
            mCurrentText = btnText;
        }
    }
}