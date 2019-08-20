using System.Globalization;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class StatisticsMenu: Menu
    {
        private Texture2D mBackground;
        private HudElement mAnimalsBought;
        private HudElement mAnimalsMax;
        private HudElement mGametime;
        private HudElement mMaxGold;
        private HudElement mZombiesKilled;
        private HudElement mWon;
        private HudElement mLost;
        private HudElement mPoints;


        private Texture2D mHighScoreTexture;

        private SpriteFont mFont;
        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;
            InputManager.EscPressed += EscPressedStatisticsMenu;
        }

        public void LoadContent(ContentManager content)
        {
            // Loading background picture
            mBackground = content.Load<Texture2D>("Menus/OptionsAndStatisticsMenu");
            mAnimalsBought = new HudElement(new Point(470, 100), new Point(150, 30));
            mAnimalsMax = new HudElement(new Point(470, 200), new Point(150, 30));
            mGametime = new HudElement(new Point(470, 300), new Point(150, 30));
            mMaxGold = new HudElement(new Point(470, 400), new Point(150, 30));
            mZombiesKilled = new HudElement(new Point(470, 500), new Point(150, 30));
            mWon = new HudElement(new Point(470, 600), new Point(150, 30));
            mLost = new HudElement(new Point(470, 700), new Point(150, 30));
            mPoints = new HudElement(new Point(470, 800), new Point(150, 30));
            

            mFont = content.Load<SpriteFont>("File");

            mHighScoreTexture = content.Load<Texture2D>("Textures\\highscore");

            mAnimalsBought.LoadContent(content);
            mAnimalsMax.LoadContent(content);
            mGametime.LoadContent(content);
            mMaxGold.LoadContent(content);
            mZombiesKilled.LoadContent(content);
            mWon.LoadContent(content);
            mLost.LoadContent(content);
            mPoints.LoadContent(content);
        }


        public void Update()
        {
            mAnimalsBought.ChangeText(Game1.sStatistics.Animals.ToString(CultureInfo.InvariantCulture));
            mAnimalsMax.ChangeText(Game1.sStatistics.OldAnimalsAlive.ToString(CultureInfo.InvariantCulture));
            mGametime.ChangeText(Game1.sStatistics.OldGameTime.ToString(CultureInfo.InvariantCulture));
            mMaxGold.ChangeText(Game1.sStatistics.OldGold.ToString(CultureInfo.InvariantCulture));
            mZombiesKilled.ChangeText(Game1.sStatistics.ZombiesKilled.ToString(CultureInfo.InvariantCulture));
            mWon.ChangeText(Game1.sStatistics.GameWon.ToString());
            mLost.ChangeText(Game1.sStatistics.GameLost.ToString());
            mPoints.ChangeText(Game1.sStatistics.OldPoints.ToString());
            Game1.sStatistics.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(mBackground, new Rectangle(0, 0,mScreenWidth, mScreenHeight),Color.Azure);
            spriteBatch.DrawString(mFont, "Insgesamt gekaufte Tiere : ", new Vector2(50, 100), Color.Black);
            mAnimalsBought.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Höste Anzahl gleichzeitig lebender Tiere : ", new Vector2(50, 200), Color.Black);
            mAnimalsMax.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Insgesamte Ingame time in Sekunden : ", new Vector2(50, 300), Color.Black);
            mGametime.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Maximale Anzahl Gold : ", new Vector2(50, 400), Color.Black);
            mMaxGold.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Insgesamt getötete Zombies : ", new Vector2(50, 500), Color.Black);
            mZombiesKilled.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Gewonenne Spiele : ", new Vector2(50, 600), Color.Black);
            mWon.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Verlorene Spiele : ", new Vector2(50, 700), Color.Black);
            mLost.Draw(spriteBatch);

            spriteBatch.DrawString(mFont, "Maximale Punkte : ", new Vector2(50, 800), Color.Black);
            mPoints.Draw(spriteBatch);

            spriteBatch.Draw(mHighScoreTexture,new Rectangle(850,130,200,200), Color.AliceBlue);
            spriteBatch.DrawString(mFont, Game1.sStatistics.mHighScore1.ToString(), new Vector2(880, 205), Color.Black);
            spriteBatch.DrawString(mFont, Game1.sStatistics.mHighScore2.ToString(), new Vector2(880, 250), Color.Black);
            spriteBatch.DrawString(mFont, Game1.sStatistics.mHighScore3.ToString(), new Vector2(880, 295), Color.Black);

            spriteBatch.End();
        }

        private void EscPressedStatisticsMenu(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.StatisticsMenu) return;
            GameStateManager.State = GameState.MainMenu;
        }
    }
}
