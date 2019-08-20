using System.Threading;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class OptionsMenu : Menu
    {
        private Texture2D mBackground;

        private SpriteFont mSpriteFont;

        public Switch mVolume;
        private Switch mMusic;

        private MenuButton mResolution;


        public static bool mFromGame;

        // The Textures for the Animation of slider
        private VolumeSlider mVolumeSlider;

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;
            InputManager.EscPressed += EscPressedOptionsMenu;
        }

        public void LoadContent(ContentManager content)
        {
            // Loading background picture
            mBackground = content.Load<Texture2D>("Buttons/pause");
            var volumeOffImage = TextureManager.GetTexture("button_off");
            var volumeOnImage = TextureManager.GetTexture("button_on");

            mSpriteFont = content.Load<SpriteFont>("FileHeading");

            mVolume = new Switch(volumeOnImage, volumeOffImage, new Point(200, 100),
                                64, 128, "An", "Aus");

            mMusic = new Switch(volumeOnImage, volumeOffImage, new Point(200, 200),
                64, 128, "An", "Aus");

            mResolution = new MenuButton(new Point(200, 300), 75, 240, "Auflösung anpassen");

            mVolume.LoadContent(content);
            mMusic.LoadContent(content);
            mResolution.LoadContent(content);

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

            mVolumeSlider = new VolumeSlider(new Point(400, 100), 70, 300, slider00, slider01,
                slider02, slider03, slider04, slider05, slider06,
                slider07, slider08, slider09, slider10);

            mBackground = content.Load<Texture2D>("Menus/OptionsAndStatisticsMenu");
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();
            mVolumeSlider.Update(inputState);
            if (mVolume.Update(inputState))
            {
                mVolume.ChangeSwitch();
                ChangeVolume();
                Game1.sPause.mVolume.ChangeSwitch();
                Thread.Sleep(100);
            }

            if (mMusic.Update(inputState))
            {
                mMusic.ChangeSwitch();
                ChangeMusik();
                Thread.Sleep(100);
            }

            if (mResolution.Update(inputState))
            {
                GameStateManager.State = GameState.ResolutionMenu;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(mBackground, new Rectangle(0,0,mScreenWidth,mScreenHeight), Color.Azure);
            spriteBatch.DrawString(mSpriteFont, "Ton : ", new Vector2(50, 120), Color.Black);
            spriteBatch.DrawString(mSpriteFont, "Musik : ", new Vector2(50, 220), Color.Black);

            mMusic.Draw(spriteBatch);
            mVolume.Draw(spriteBatch);
            mVolumeSlider.Draw(spriteBatch);
            mResolution.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void EscPressedOptionsMenu(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.OptionsMenu) return;
            if (mFromGame)
            {
                GameStateManager.State = GameState.PlayGameMenu;
                mFromGame = false;
            }
            else
            {
                GameStateManager.State = GameState.MainMenu;
            }
        }

        private static void ChangeVolume()
        {
            SoundManager.SoundOn = !SoundManager.SoundOn;
        }

        private static void ChangeMusik()
        {
            SoundManager.MusicOn = !SoundManager.MusicOn;
            SoundManager.SongVolume = SoundManager.MusicOn ? 1.0f : 0.0f;
        }
    }
}
