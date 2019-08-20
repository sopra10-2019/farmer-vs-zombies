using System;
using System.Diagnostics;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class MainMenu : Menu
    {
        private Texture2D mBackground;

        // The buttons
        private MenuButton mStart;
        private MenuButton mOptions;
        private MenuButton mStatistics;
        private MenuButton mAchievements;
        private MenuButton mExit;

        public MainMenu()
        {
            Debug.WriteLine("Main Menu created");
        }

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;
            InputManager.EscPressed += EscPressed;
        }

        public void LoadContent(ContentManager content)
        {
            // loading background picture
            mBackground = content.Load<Texture2D>("Menus/MainMenu");

            // start button definition here because initialization gets called before LoadContent
            mStart = new MenuButton(new Point((mScreenWidth / 2) - 100, (mScreenHeight / 5) - 37), 75, 200, "Spiel Starten", 5);

            // Drawing button for statistics menu
            mStatistics = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + ButtonDistance), 75, 150, "Statistiken", 6);
            mAchievements = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + 2 * ButtonDistance), 75, 150, "Achievements");

            mOptions = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + 3 * ButtonDistance), 75, 150, "Optionen", 5);

            mExit = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + 4 * ButtonDistance), 75, 150, "Beenden", 5);

            // Loa0ing content of Buttons
            mStart.LoadContent(content);
            mStatistics.LoadContent(content);
            mAchievements.LoadContent(content);
            mOptions.LoadContent(content);
            mExit.LoadContent(content);
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();

            if (mStart.Update(inputState))
            {
                GameStateManager.State = GameState.StartGameMenu;
            }

            if (mOptions.Update(inputState))
            {
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (mStatistics.Update(inputState))
            {
                GameStateManager.State = GameState.StatisticsMenu;
                Game1.sStatistics.Load();
            }

            if (mAchievements.Update(inputState))
            {
                GameStateManager.State = GameState.AchievementsMenu;
                Game1.sAchievements.Load();
            }

            if (mExit.Update(inputState))
            {
                Environment.Exit(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mBackground, new Rectangle(0,0,mScreenWidth, mScreenHeight), Color.Azure);
            mStart.Draw(spriteBatch);
            mOptions.Draw(spriteBatch);
            mStatistics.Draw(spriteBatch);
            mAchievements.Draw(spriteBatch);
            mExit.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void EscPressed(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.MainMenu) return;
            Environment.Exit(0);
        }
    }
}