using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class ResolutionMenu : Menu
    {
        private Texture2D mBackground;

        private MenuButton m1920X1080;
        private MenuButton m1680X1050;
        private MenuButton m1600X1200;
        private MenuButton m1600X1024;
        private MenuButton m1600X900;
        private MenuButton m1440X900;
        private MenuButton m1366X768;
        private MenuButton m1280X960;
        private MenuButton m1280X800;
        private MenuButton m1280X768;
        private MenuButton m1024X768;
        private MenuButton m800X600;

        private MenuButton mDeactivated;

        public ResolutionMenu()
        {

        }

        public void Initialize(GraphicsDevice gd)
        {
            mScreenWidth = gd.Viewport.Width;
            mScreenHeight = gd.Viewport.Height;
            InputManager.EscPressed += EscPressed;
        }

        public void LoadContent(ContentManager content)
        {
            mBackground = content.Load<Texture2D>("Menus/MainMenu");

            m1920X1080 = new MenuButton(new Point((mScreenWidth - 600) / 4, mScreenHeight / 5), 75, 200, "1920 x 1080");
            m1680X1050 = new MenuButton(new Point(2 * (mScreenWidth - 600) / 4 + 200, mScreenHeight / 5), 75, 200, "1680 x 1050");
            m1600X1200 = new MenuButton(new Point(3 * (mScreenWidth - 600) / 4 + 400, mScreenHeight / 5), 75, 200, "1600 x 1200");
            m1600X1024 = new MenuButton(new Point((mScreenWidth - 600) / 4, 2 * mScreenHeight / 5), 75, 200, "1600 x 1024");
            m1600X900 =  new MenuButton(new Point(2 * (mScreenWidth - 600) / 4 + 200, 2 * mScreenHeight / 5), 75, 200, "1600 x 900");
            m1440X900 =  new MenuButton(new Point(3 * (mScreenWidth - 600) / 4 + 400, 2 * mScreenHeight / 5), 75, 200, "1440 x 900");
            m1366X768 =  new MenuButton(new Point((mScreenWidth - 600) / 4, 3 * mScreenHeight / 5), 75, 200, "1366 x 768");
            m1280X960 =  new MenuButton(new Point(2 * (mScreenWidth - 600) / 4 + 200, 3 * mScreenHeight / 5), 75, 200, "1280 x 960");
            m1280X800 =  new MenuButton(new Point(3 * (mScreenWidth - 600) / 4 + 400, 3 * mScreenHeight / 5), 75, 200, "1280 x 800");
            m1280X768 =  new MenuButton(new Point((mScreenWidth - 600) / 4, 4 * mScreenHeight / 5), 75, 200, "1280 x 768");
            m1024X768 =  new MenuButton(new Point(2 * (mScreenWidth - 600) / 4 + 200, 4 * mScreenHeight / 5), 75, 200, "1024 x 768");
            m800X600 =   new MenuButton(new Point(3 * (mScreenWidth - 600) / 4 + 400, 4 * mScreenHeight / 5), 75, 200, "800 x 600");

            m1920X1080.LoadContent(content);
            m1680X1050.LoadContent(content);
            m1600X1200.LoadContent(content);
            m1600X1024.LoadContent(content);
            m1600X900.LoadContent(content);
            m1440X900.LoadContent(content);
            m1366X768.LoadContent(content);
            m1280X960.LoadContent(content);
            m1280X800.LoadContent(content);
            m1280X768.LoadContent(content);
            m1024X768.LoadContent(content);
            m800X600.LoadContent(content);

            var res = (mScreenWidth, mScreenHeight);
            if (res == (1920, 1080))
            {
                mDeactivated = m1920X1080;
                mDeactivated.IsActive = false;
            }
            else if (res == (1680, 1050))
            {
                mDeactivated = m1680X1050;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 1200))
            {
                mDeactivated = m1600X1200;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 1024))
            {
                mDeactivated = m1600X1024;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 900))
            {
                mDeactivated = m1600X900;
                mDeactivated.IsActive = false;
            }
            else if (res == (1440, 900))
            {
                mDeactivated = m1440X900;
                mDeactivated.IsActive = false;
            }
            else if (res == (1366, 768))
            {
                mDeactivated = m1366X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 960))
            {
                mDeactivated = m1280X960;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 800))
            {
                mDeactivated = m1280X800;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 768))
            {
                mDeactivated = m1280X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (1024, 768))
            {
                mDeactivated = m1024X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (800, 600))
            {
                mDeactivated = m800X600;
                mDeactivated.IsActive = false;
            }
        }

        public void Update()
        {
            var inputState = InputManager.GetCurrentInputState();

            if (m1920X1080.Update(inputState))
            {
                Game1.SetResolution(1920, 1080);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1680X1050.Update(inputState))
            {
                Game1.SetResolution(1680, 1050);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1600X1200.Update(inputState))
            {
                Game1.SetResolution(1600, 1200);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1600X1024.Update(inputState))
            {
                Game1.SetResolution(1600, 1024);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1600X900.Update(inputState))
            {
                Game1.SetResolution(1600, 900);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1440X900.Update(inputState))
            {
                Game1.SetResolution(1440, 900);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1366X768.Update(inputState))
            {
                Game1.SetResolution(1366, 768);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1280X960.Update(inputState))
            {
                Game1.SetResolution(1280, 960);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1280X800.Update(inputState))
            {
                Game1.SetResolution(1280, 800);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1280X768.Update(inputState))
            {
                Game1.SetResolution(1280, 768);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m1024X768.Update(inputState))
            {
                Game1.SetResolution(1024, 768);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }

            if (m800X600.Update(inputState))
            {
                Game1.SetResolution(800, 600);
                UpdateButtons();
                GameStateManager.State = GameState.OptionsMenu;
            }
        }

        private void UpdateButtons()
        {
            mScreenWidth = Game1.Resolution.Item1;
            mScreenHeight = Game1.Resolution.Item2;

            mDeactivated.IsActive = true;
            var res = (mScreenWidth, mScreenHeight);
            if (res == (1920, 1080))
            {
                mDeactivated = m1920X1080;
                mDeactivated.IsActive = false;
            }
            else if (res == (1680, 1050))
            {
                mDeactivated = m1680X1050;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 1200))
            {
                mDeactivated = m1600X1200;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 1024))
            {
                mDeactivated = m1600X1024;
                mDeactivated.IsActive = false;
            }
            else if (res == (1600, 900))
            {
                mDeactivated = m1600X900;
                mDeactivated.IsActive = false;
            }
            else if (res == (1440, 900))
            {
                mDeactivated = m1440X900;
                mDeactivated.IsActive = false;
            }
            else if (res == (1366, 768))
            {
                mDeactivated = m1366X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 960))
            {
                mDeactivated = m1280X960;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 800))
            {
                mDeactivated = m1280X800;
                mDeactivated.IsActive = false;
            }
            else if (res == (1280, 768))
            {
                mDeactivated = m1280X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (1024, 768))
            {
                mDeactivated = m1024X768;
                mDeactivated.IsActive = false;
            }
            else if (res == (800, 600))
            {
                mDeactivated = m800X600;
                mDeactivated.IsActive = false;
            }

            m1920X1080.SetPosition(new Point((mScreenWidth - 600) / 4, mScreenHeight / 5));
            m1680X1050.SetPosition(new Point(2 * (mScreenWidth - 600) / 4 + 200, mScreenHeight / 5));
            m1600X1200.SetPosition(new Point(3 * (mScreenWidth - 600) / 4 + 400, mScreenHeight / 5));
            m1600X1024.SetPosition(new Point((mScreenWidth - 600) / 4, 2 * mScreenHeight / 5));
            m1600X900.SetPosition(new Point(2 * (mScreenWidth - 600) / 4 + 200, 2 * mScreenHeight / 5));
            m1440X900.SetPosition(new Point(3 * (mScreenWidth - 600) / 4 + 400, 2 * mScreenHeight / 5));
            m1366X768.SetPosition(new Point((mScreenWidth - 600) / 4, 3 * mScreenHeight / 5));
            m1280X960.SetPosition(new Point(2 * (mScreenWidth - 600) / 4 + 200, 3 * mScreenHeight / 5));
            m1280X800.SetPosition(new Point(3 * (mScreenWidth - 600) / 4 + 400, 3 * mScreenHeight / 5));
            m1280X768.SetPosition(new Point((mScreenWidth - 600) / 4, 4 * mScreenHeight / 5));
            m1024X768.SetPosition(new Point(2 * (mScreenWidth - 600) / 4 + 200, 4 * mScreenHeight / 5));
            m800X600.SetPosition(new Point(3 * (mScreenWidth - 600) / 4 + 400, 4 * mScreenHeight / 5));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mBackground, new Rectangle(0, 0, mScreenWidth, mScreenHeight), Color.Azure);
            m1920X1080.Draw(spriteBatch);
            m1680X1050.Draw(spriteBatch);
            m1600X1200.Draw(spriteBatch);
            m1600X1024.Draw(spriteBatch);
            m1600X900.Draw(spriteBatch);
            m1440X900.Draw(spriteBatch);
            m1366X768.Draw(spriteBatch);
            m1280X960.Draw(spriteBatch);
            m1280X800.Draw(spriteBatch);
            m1280X768.Draw(spriteBatch);
            m1024X768.Draw(spriteBatch);
            m800X600.Draw(spriteBatch);
            spriteBatch.End();
        }

        private static void EscPressed(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.ResolutionMenu) return;
            GameStateManager.State = GameState.OptionsMenu;
        }
    }
}
