using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class AchievementsMenu : Menu
    {
        private Texture2D mBackground;




        private SpriteFont mFont;
        private HudElement mErsteGeburt;
        private HudElement mWahrerFarmer;
        private HudElement mFarmingSimulator;
        private HudElement mDerAnfangVomEnde;
        private HudElement mVomGejagtenZumJäger;
        private HudElement mGehirnlos;
        private HudElement mObdachlos;
        private HudElement mDasWarErstDerAnfang;
        private HudElement mDieLangeNacht;
        private HudElement mDieApokalypseIstVorbei;
        private HudElement mSieHattenKeineChance;

        public void Initialize(GraphicsDevice gd)
        {
            mScreenHeight = gd.Viewport.Height;
            mScreenWidth = gd.Viewport.Width;
            InputManager.EscPressed += EscPressedAchievementsMenu;
        }

        public void LoadContent(ContentManager content)
        {
            // Loading background picture
            mBackground = content.Load<Texture2D>("Menus/OptionsAndStatisticsMenu");
            mErsteGeburt = new HudElement(new Point(400, 100), new Point(150, 30));
            mWahrerFarmer = new HudElement(new Point(400, 200), new Point(150, 30));
            mFarmingSimulator = new HudElement(new Point(400, 300), new Point(150, 30));
            mDerAnfangVomEnde = new HudElement(new Point(400, 400), new Point(150, 30));
            mVomGejagtenZumJäger = new HudElement(new Point(400, 500), new Point(150, 30));
            mGehirnlos = new HudElement(new Point(400, 600), new Point(150, 30));
            mObdachlos = new HudElement(new Point(400, 700), new Point(150, 30));
            mDasWarErstDerAnfang = new HudElement(new Point(400, 800), new Point(150, 30));
            mDieLangeNacht = new HudElement(new Point(400, 900), new Point(150, 30));
            mDieApokalypseIstVorbei = new HudElement(new Point(400, 1000), new Point(150, 30));
            mSieHattenKeineChance = new HudElement(new Point(400, 1100), new Point(150, 30));


            mFont = content.Load<SpriteFont>("File");


            mErsteGeburt.LoadContent(content);
            mWahrerFarmer.LoadContent(content);
            mFarmingSimulator.LoadContent(content);
            mDerAnfangVomEnde.LoadContent(content);
            mVomGejagtenZumJäger.LoadContent(content);
            mGehirnlos.LoadContent(content);
            mObdachlos.LoadContent(content);
            mDasWarErstDerAnfang.LoadContent(content);
            mDieLangeNacht.LoadContent(content);
            mDieApokalypseIstVorbei.LoadContent(content);
            mSieHattenKeineChance.LoadContent(content);
            mErsteGeburt.ChangeText("To Do!");
            mWahrerFarmer.ChangeText("To Do!");
            mFarmingSimulator.ChangeText("To Do!");
            mDerAnfangVomEnde.ChangeText("To Do!");
            mVomGejagtenZumJäger.ChangeText("To Do!");
            mGehirnlos.ChangeText("To Do!");
            mObdachlos.ChangeText("To Do!");
            mDasWarErstDerAnfang.ChangeText("To Do!");
            mDieLangeNacht.ChangeText("To Do!");
            mDieApokalypseIstVorbei.ChangeText("To Do!");
            mSieHattenKeineChance.ChangeText("To Do!");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(mBackground, new Rectangle(0, 0, mScreenWidth, mScreenHeight), Color.Azure);
            spriteBatch.DrawString(mFont, "Erstegeburt : ", new Vector2(mErsteGeburt.mBackRect.X - 300, mErsteGeburt.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.ErsteGeburt)
            {
                mErsteGeburt.ChangeText("Done!");
            }
            mErsteGeburt.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Wahrer Farmer : ", new Vector2(mWahrerFarmer.mBackRect.X - 300, mWahrerFarmer.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.WahrerFarmer)
            {
                mWahrerFarmer.ChangeText("Done!");
            }
            mWahrerFarmer.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Farming Simulator : ", new Vector2(mFarmingSimulator.mBackRect.X - 300, mFarmingSimulator.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.FarmingSimulator)
            {
                mFarmingSimulator.ChangeText("Done!");
            }
            mFarmingSimulator.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Der Anfang vom Ende : ", new Vector2(mDerAnfangVomEnde.mBackRect.X - 300, mDerAnfangVomEnde.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.DerAnfangVomEnde)
            {
                mDerAnfangVomEnde.ChangeText("Done!");
            }
            mDerAnfangVomEnde.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Vom Gejagten zum Jäger : ", new Vector2(mVomGejagtenZumJäger.mBackRect.X - 300, mVomGejagtenZumJäger.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.VomGejagtenZumJäger)
            {
                mVomGejagtenZumJäger.ChangeText("Done!");
            }
            mVomGejagtenZumJäger.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Gehirnlos : ", new Vector2(mGehirnlos.mBackRect.X - 300, mGehirnlos.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.Gehirnlos)
            {
                mGehirnlos.ChangeText("Done!");
            }
            mGehirnlos.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Obdachlos : ", new Vector2(mObdachlos.mBackRect.X - 300, mObdachlos.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.Obdachlos)
            {
                mObdachlos.ChangeText("Done!");
            }
            mObdachlos.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Das war erst der Anfang : ", new Vector2(mDasWarErstDerAnfang.mBackRect.X - 300, mDasWarErstDerAnfang.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.DasWarErstDerAnfang)
            {
                mDasWarErstDerAnfang.ChangeText("Done!");
            }
            mDasWarErstDerAnfang.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Die lange Nacht : ", new Vector2(mDieLangeNacht.mBackRect.X - 300, mDieLangeNacht.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.DieLangeNacht)
            {
                mDieLangeNacht.ChangeText("Done!");
            }
            mDieLangeNacht.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Der Apokalypse ist vorbei! : ", new Vector2(mDieApokalypseIstVorbei.mBackRect.X - 300, mDieApokalypseIstVorbei.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.DieApokalypseIstVorbei)
            {
                mDieApokalypseIstVorbei.ChangeText("Done!");
            }
            mDieApokalypseIstVorbei.Draw(spriteBatch);
            spriteBatch.DrawString(mFont, "Sie hatten keine Chance! : ", new Vector2(mSieHattenKeineChance.mBackRect.X - 300, mSieHattenKeineChance.mBackRect.Y), Color.Black);
            if (Game1.sAchievements.SieHattenKeineChance)
            {
                mSieHattenKeineChance.ChangeText("Done!");
            }
            mSieHattenKeineChance.Draw(spriteBatch);
            spriteBatch.End();
        }

        private void EscPressedAchievementsMenu(object sender, InputState inputState)
        {
            if (GameStateManager.State != GameState.AchievementsMenu) return;
            GameStateManager.State = GameState.MainMenu;
        }
    }
}

