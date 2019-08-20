using System.Text;
using FarmervsZombies.Managers;
using FarmervsZombies.Menus;
using FarmervsZombies.SaveAndLoad;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies.MenuButtons
{
    internal sealed class Textbox
    {
        private Texture2D mTextBoxImage;
        private StringBuilder mInput;
        private SpriteFont mFont;
        private readonly Rectangle mBackRect;
        public bool mShowTextBox;

        public Textbox(Point textBoxPos, Point textBoxSize)
        {
            mBackRect.Location = textBoxPos;
            mBackRect.Size = textBoxSize;
            mInput = new StringBuilder();
            mShowTextBox = false;
        }

        public void LoadContent(ContentManager content)
        {
            mFont = content.Load<SpriteFont>("File");
            mTextBoxImage = content.Load<Texture2D>("Buttons/default");
        }


        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();
            var keysPressed = inputState.GetCurrentlyPressedKeys();
            foreach (var key in keysPressed)
            {
                if (mInput.Length <= 20)
                {
                    if (key == Keys.Enter)
                    {
                        if (mInput.Length > 0)
                        {
                            GameStateManager.State = Menu.GameState.MainMenu;
                            Save();
                            Game1.sStatistics.Save();
                            Game1.sAchievements.Save();
                            Game1.sSelection.Clear();
                            ObjectManager.Instance.UnloadAll();
                        }
                        else
                        {
                            mShowTextBox = false;
                            mInput.Clear();
                        }
                    }

                    else if (key != Keys.Escape)
                    {
                        if (key == Keys.Back && mInput.Length > 0)
                        {
                            mInput = mInput.Remove(mInput.Length - 1, 1);
                        }
                        else if (mInput.Length < 20 && (int) Keys.A <= (int)key && (int) Keys.Z >= (int)key)
                        {
                            mInput.Append(key.ToString());
                        }
                    }
                }
                
            }


            // make logic fot textbox so that user can input text

        }

        // if esc is pressed during game saving the textbox just disappears

        public void Draw(SpriteBatch spriteBatch)
        {
            if (mShowTextBox)
            {
                spriteBatch.Draw(mTextBoxImage, mBackRect, Color.Aqua);
                spriteBatch.DrawString(mFont, mInput, new Vector2(mBackRect.X + 20, mBackRect.Y + 20), Color.Black);
            }
        }

        private void Save()
        {
            mInput.Append(".spielstand");
            var saveAndLoad = new SaveLoad();
            saveAndLoad.Save(ObjectManager.Instance.GetList(), mInput.ToString());
            mInput.Clear();
        }

    }
}
