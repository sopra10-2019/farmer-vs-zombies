using System.Threading;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{

    internal sealed class PauseMenu : Menu
    {
        private SpriteFont mSpriteFont;

        private Textbox mTextbox;

        private Texture2D mBackground;

        private MenuButton mExit;
        private MenuButton mSaveAndLeave;
        private MenuButton mBack;
        private MenuButton mOptions;
        public Switch mVolume;

        // The Textures for the Animation of slider
        private VolumeSlider mVolumeSlider;

        public bool ShowTextBox
        {
            get => mTextbox != null && mTextbox.mShowTextBox;
            set
            {
                if (mTextbox != null) mTextbox.mShowTextBox = value;
            }
        }

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height / 2;
            mScreenWidth = gd.Viewport.Width / 2;
        }
        public void LoadContent(ContentManager content)
        {
            // Loading background picture
            mBackground = content.Load<Texture2D>("Buttons/pause");
            var volumeOffImage = TextureManager.GetTexture("button_off");
            var volumeOnImage = TextureManager.GetTexture("button_on");

            mSpriteFont = content.Load<SpriteFont>("FileHeading");

            mSaveAndLeave = new MenuButton(new Point(mScreenWidth - 150 + 300, 450), 75, 300,
                                                "Speichern und Verlassen");
            mExit = new MenuButton(new Point(mScreenWidth - 75, 450), 75, 150, "Verlassen");
            mVolume = new Switch(volumeOnImage, volumeOffImage, new Point(mScreenWidth - 300, 100),
                                64, 128, "An", "Aus");
            mBack = new MenuButton(new Point(mScreenWidth - 75 - 350, 450), 75, 200, "Zurück ins Spiel");

            mOptions = new MenuButton(new Point(mScreenWidth - 75 - 350, 300), 75, 150, "Optionen");
            mExit.LoadContent(content);
            mSaveAndLeave.LoadContent(content);
            mVolume.LoadContent(content);
            mBack.LoadContent(content);
            mOptions.LoadContent(content);

            var slider00 = content.Load<Texture2D>("Buttons/slider/Volumebar_00");
            var slider01 = content.Load<Texture2D>("Buttons/slider/Volumebar_01");
            var slider02 = content.Load<Texture2D>("Buttons/slider/Volumebar_02");
            var slider03 = content.Load<Texture2D>("Buttons/slider/Volumebar_03");
            var slider04 = content.Load<Texture2D>("Buttons/slider/Volumebar_04");
            var slider05 = content.Load<Texture2D>("Buttons/slider/Volumebar_05");
            var slider06 = content.Load<Texture2D>("Buttons/slider/Volumebar_06");
            var slider07 = content.Load<Texture2D>("Buttons/slider/Volumebar_07");
            var slider08 = content.Load<Texture2D>("Buttons/slider/Volumebar_08");
            var slider09 = content.Load<Texture2D>("Buttons/slider/Volumebar_09");
            var slider10 = content.Load<Texture2D>("Buttons/slider/Volumebar_10");

            mVolumeSlider = new VolumeSlider(new Point(mScreenWidth, 100), 70, 300, slider00, slider01,
                slider02, slider03, slider04, slider05, slider06,
                slider07, slider08, slider09, slider10);


            mTextbox = new Textbox(new Point(mScreenWidth - 200,mScreenHeight - 200), new Point(300,50));
            mTextbox.LoadContent(content);
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();
            if (mSaveAndLeave.Update(inputState))
            {
                ShowTextBox = true;
            }

            if (mExit.Update(inputState))
            {
                GameStateManager.State = GameState.MainMenu;
                Game1.sStatistics.Save();
                Game1.sAchievements.Save();
                Game1.sSelection.Clear();
                ObjectManager.Instance.UnloadAll();
            }

            if (mVolume.Update(inputState))
            {
                mVolume.ChangeSwitch();
                Game1.sOptions.mVolume.ChangeSwitch();
                ChangeVolume();
                Thread.Sleep(100);
            }

            if (mBack.Update(inputState))
            {
                GameStateManager.State = GameState.PlayGameMenu;
            }

            if (mOptions.Update(inputState))
            {
                Game1.sStatistics.Save();
                GameStateManager.State = GameState.OptionsMenu;
                OptionsMenu.mFromGame = true;
            }

            mTextbox.Update();
            mVolumeSlider.Update(inputState);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mBackground, new Rectangle(mScreenWidth / 2, 30, mScreenWidth, mScreenHeight), Color.Azure);
            mExit.Draw(spriteBatch);
            mSaveAndLeave.Draw(spriteBatch);
            spriteBatch.DrawString(mSpriteFont, "Ton : ", new Vector2(mScreenWidth - 450, 120), Color.Black);
            mVolume.Draw(spriteBatch);
            mVolumeSlider.Draw(spriteBatch);
            mBack.Draw(spriteBatch);
            mOptions.Draw(spriteBatch);
            if (ShowTextBox)
            {
                mTextbox.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        private static void ChangeVolume()
        {
            SoundManager.SoundOn = !SoundManager.SoundOn;
        }
    }
}
