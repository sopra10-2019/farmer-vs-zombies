using System.Diagnostics;
using FarmervsZombies.AI;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using FarmervsZombies.SaveAndLoad;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class LevelMenu : Menu
    {

        private Texture2D mBackground;

        // The buttons
        private MenuButton mLevelOne;
        private MenuButton mLevelTwo;
        private MenuButton mLevelTree;

        public LevelMenu()
        {
            Debug.WriteLine("Level Game Menu created");
        }

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;
            InputManager.EscPressed += EscPressed;
        }
        public void LoadContent(ContentManager content)
        {

            mLevelOne = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37), 75, 150, "Einfach", txtposX: 5);

            mLevelTwo = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + ButtonDistance), 75, 150, "Mittel", txtposX: 5);

            mLevelTree = new MenuButton(new Point((mScreenWidth / 2) - 75, (mScreenHeight / 5) - 37 + 2 * ButtonDistance), 75, 150, "Schwer", txtposX: 5);

            mBackground = content.Load<Texture2D>("Menus/MainMenu");

            mLevelOne.LoadContent(content);
            mLevelTwo.LoadContent(content);
            mLevelTree.LoadContent(content);
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();
            if (mLevelOne.Update(inputState)) {
                Game1.Difficulty = 1;
            }
            if (mLevelTwo.Update(inputState)) {
                Game1.Difficulty = 2;
            }
            if (mLevelTree.Update(inputState)) {
                Game1.Difficulty = 3;
            }
            EconomyManager.Instance.Initialize();


            if (mLevelOne.Update(inputState) || mLevelTwo.Update(inputState) || mLevelTree.Update(inputState))
            {
                var saveLoad = new SaveLoad();
                saveLoad.Load("NEWMAP.spiel");
                GameStateManager.State = GameState.PlayGameMenu;
                FarmerQueueManager.Instance.EmptyFQueue();
                Game1.FarmerDied = false;
                Game1.GraveyardBuilt = false;
                Game1.AiDied = false;
                Game1.sFog.Reset();
                Ai.Reset();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mBackground, new Rectangle(0, 0, mScreenWidth, mScreenHeight), Color.Azure);
            mLevelOne.Draw(spriteBatch);
            mLevelTwo.Draw(spriteBatch);
            mLevelTree.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void EscPressed(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.LevelMenu) return;
            GameStateManager.State = GameState.StartGameMenu;
        }

    }
}
