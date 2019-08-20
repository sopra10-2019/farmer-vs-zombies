using System.Diagnostics;
using System.Globalization;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class HudMenu : Menu
    {
        private int mLive;

        private HudElement mLiveBox;
        private HudElement mMoneyBox;
        private Texture2D mCoin;
        private Animation mCoinSpin;
        private Texture2D mHealth;

        // seeds
        private Texture2D mSeed1;
        private Texture2D mSeed2;
        private Texture2D mSeed3;


        private HudElement mSeedBox1;
        private HudElement mSeedBox2;
        private HudElement mSeedBox3;
        private HudElement mPoints;
        private HudElement mTime;

        private HudElement mFps;

        private readonly bool mShowPoints;
        private readonly bool mShowTime;

        public bool HudVisible { private get; set; }
        private bool mShowFps;

        public HudMenu()
        {
            Debug.WriteLine("Hud Menu Created");
            mLive = 1000;
            mShowPoints = true;
            mShowTime = true;
            HudVisible = true;
        }

        public void LoadContent(ContentManager content)
        {
            mLiveBox = new HudElement(new Point(48, 5), new Point(100, 30));
            mMoneyBox = new HudElement(new Point(189, 5), new Point(100, 30));
            mSeedBox1 = new HudElement(new Point(339, 5), new Point(30, 30));
            mSeedBox2 = new HudElement(new Point(399, 5), new Point(30, 30));
            mSeedBox3 = new HudElement(new Point(459, 5), new Point(30, 30));

            mHealth = TextureManager.GetTexture("icons", 32, 32, 25);
            mCoin = TextureManager.GetTexture("icons", 32, 32, 24);
            mSeed1 = TextureManager.GetTexture("wheat", 32, 32, 3);
            mSeed2 = TextureManager.GetTexture("wheat2", 32, 32, 3);
            mSeed3 = TextureManager.GetTexture("icons", 32, 32, 2);
            
            mCoinSpin = AnimationManager.GetAnimation(AnimationManager.CoinSpin, this);

            mSeedBox1 = new HudElement(new Point(330, 5), new Point(100, 30));
            mSeedBox2 = new HudElement(new Point(471, 5), new Point(100, 30));
            mSeedBox3 = new HudElement(new Point(612, 5), new Point(100, 30));

            mPoints = new HudElement(new Point(612+141*3, 5), new Point(100, 30));
            mTime = new HudElement(new Point(612 + 141 * 4, 5), new Point(100, 30));

            mFps = new HudElement(new Point(48, 30), new Point(100, 20));

            mLiveBox.LoadContent(content);
            mMoneyBox.LoadContent(content);


            mSeedBox1.LoadContent(content);
            mSeedBox2.LoadContent(content);
            mSeedBox3.LoadContent(content);
            mPoints.LoadContent(content);
            mTime.LoadContent(content);
            mFps.LoadContent(content);
        }


        public void Update()
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            var maxHealth = farmer?.MaxHealth ?? 100;
            // if a farmer exists the live of the farmer is used 0 else
            mLive = (int)(farmer?.Health ?? 100);
            mLiveBox.ChangeText(mLive + "/" + (int)maxHealth);
            mMoneyBox.ChangeText(EconomyManager.Instance.GoldAmount.ToString());

            mSeedBox1.ChangeText(EconomyManager.Instance.SeedAmount1.ToString());
            mSeedBox2.ChangeText(EconomyManager.Instance.SeedAmount2.ToString());
            mSeedBox3.ChangeText(EconomyManager.Instance.SeedAmount3.ToString());
            mPoints.ChangeText(Game1.sStatistics.Points.ToString());
            mTime.ChangeText(((int)(Game1.mTime)/10000).ToString(CultureInfo.InvariantCulture));
            mFps.ChangeText(Game1.FramesPerSecond.ToString());

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!HudVisible) return;

            mCoin = mCoinSpin.GetTexture();
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
            spriteBatch.Draw(mHealth, new Vector2(15,0), Color.AliceBlue);
            spriteBatch.Draw(mCoin, new Vector2(156,0), Color.AliceBlue);
            spriteBatch.Draw(mSeed1, new Vector2(297, 0), Color.AliceBlue);
            spriteBatch.Draw(mSeed2, new Vector2(438, 0), Color.AliceBlue);
            spriteBatch.Draw(mSeed3, new Vector2(579, 0), Color.AliceBlue);
            mLiveBox.Draw(spriteBatch);
            mMoneyBox.Draw(spriteBatch);
            mSeedBox1.Draw(spriteBatch);
            mSeedBox2.Draw(spriteBatch);
            mSeedBox3.Draw(spriteBatch);
            if (mShowPoints)
            {
                mPoints.Draw(spriteBatch);
            }

            if (mShowTime)
            {
                mTime.Draw(spriteBatch);
            }

            if (mShowFps)
            {
                mFps.Draw(spriteBatch);
            }
            spriteBatch.End();
        }

        public void ToggleFps(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ToggleFps)) return;
            mShowFps = !mShowFps;
        }
    }
}
