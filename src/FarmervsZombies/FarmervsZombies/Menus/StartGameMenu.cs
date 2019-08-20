using System.Diagnostics;
using System.Threading;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class StartGameMenu : Menu
    {
        // the background
        private Texture2D mBackground;

        // The buttons
        private MenuButton mStartNewGame;
        private MenuButton mLoadGame;
        private MenuButton mTechDemo;
        
        public StartGameMenu()
        {
            Debug.WriteLine("Start Game Menu created");
        }

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;

            InputManager.EscPressed += EscPressedStartGameMenu;
        }
        public void LoadContent(ContentManager content)
        {

            mStartNewGame = new MenuButton(new Point(mScreenWidth / 2 - 100, mScreenHeight / 5 - 37), 75, 200, "Neues Spiel", txtposX: 5);

            mLoadGame = new MenuButton(new Point(mScreenWidth / 2 - 75, mScreenHeight / 5 - 37 + ButtonDistance), 75, 150, "Spiel laden");

            mTechDemo = new MenuButton(new Point(mScreenWidth / 2 - 100, mScreenHeight / 5 - 37 + 2 * ButtonDistance), 75, 200, "Tech Demo starten");

            mBackground = content.Load<Texture2D>("Menus/MainMenu");


            // loading buttons
            mStartNewGame.LoadContent(content);
            mLoadGame.LoadContent(content);
            mTechDemo.LoadContent(content);
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();

            if (mLoadGame.Update(inputState))
            {
                GameStateManager.State = GameState.LoadGame;
                Game1.sStatistics.Load();
                Game1.sAchievements.Load();
                Thread.Sleep(200);
            }

            if (mStartNewGame.Update(inputState))
            {
                GameStateManager.State = GameState.LevelMenu;
                Game1.sStatistics.Load();
                Game1.sAchievements.Load();
                EconomyManager.Instance.ResetGold();
            }

            if (mTechDemo.Update(inputState))
            {
                Game1.GeneratePerformanceDemo();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mBackground, new Rectangle(0, 0, mScreenWidth, mScreenHeight), Color.Azure);
            mStartNewGame.Draw(spriteBatch);
            mLoadGame.Draw(spriteBatch);
            mTechDemo.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void EscPressedStartGameMenu(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.StartGameMenu) return;
            GameStateManager.State = GameState.MainMenu;
        }
    }
}
