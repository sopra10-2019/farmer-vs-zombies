using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal abstract class BAnimal : BMovableGameObject, IAttackable, ICollidable
    {
        public bool Team { get; protected set; } = true;
        protected Animation mWalkUpAnimation;
        protected Animation mWalkDownAnimation;
        protected Animation mWalkLeftAnimation;
        protected Animation mWalkRightAnimation;

        protected Animation mEatDownAnimation;
        protected Animation mEatUpAnimation;
        protected Animation mEatLeftAnimation;
        protected Animation mEatRightAnimation;

        protected readonly Texture2D[] mStandTextureList = new Texture2D[4];

        private const float EatCooldown = 6f;
        private float mTimeSinceEat;
        private bool mFed = true;
        private int mNotificationCount = 0;

        protected int ExperienceForLevelUp { get; set; } = 12;
        protected int mExperience;
        protected int mResourcesHeld;
        private bool mSmallCollisionBox;
        private const float SmallSizeFactor = 5f / 7f;
        protected string mFeedAction;
        protected string mStarveNotificationText;

        public virtual bool LevelUp => mExperience >= ExperienceForLevelUp;

        protected BAnimal(Texture2D texture,
            float positionX,
            float positionY,
            float textureOffsetX,
            float textureOffsetY)
            : base(texture, positionX, positionY, textureOffsetX, textureOffsetY)
        {
            mDrawLiveBarMySelf = true;
            RegenerationRate = 0.1f;
        }

        public void CollectResources(bool sound = true)
        {
            for (var i = 0; i < mResourcesHeld; i++)
            {
                GenerateResources(sound && i == 0);
            }

            mResourcesHeld = 0;
        }

        public virtual void Slaughter()
        {
            CollectResources(false);
            ObjectManager.Instance.QueueRemoval(this);
        }

        protected void Slaughter(string action)
        {
            EconomyManager.Instance.GoldIncrease(action);
            if (!LevelUp) return;
            EconomyManager.Instance.GoldIncrease(action, false);
            EconomyManager.Instance.GoldIncrease(action, false);
        }

        protected virtual void GenerateResources(bool sound = true)
        {

        }

        protected virtual void Eat()
        {
            if (Game1.sTileMap.GetTileType((int)Position.X, (int)Position.Y) != Tile.Grass) return;

            switch (mDirection)
            {
                case 0:
                    mTexture = mEatUpAnimation.GetTexture();
                    TextureOffset = mEatUpAnimation.TextureOffset;
                    break;
                case 1:
                    mTexture = mEatRightAnimation.GetTexture();
                    TextureOffset = mEatRightAnimation.TextureOffset;
                    break;
                case 2:
                    mTexture = mEatDownAnimation.GetTexture();
                    TextureOffset = mEatDownAnimation.TextureOffset;
                    break;
                case 3:
                    mTexture = mEatLeftAnimation.GetTexture();
                    TextureOffset = mEatLeftAnimation.TextureOffset;
                    break;
            }

            if (mTimeSinceEat < EatCooldown) return;

            Feed(mFeedAction, mStarveNotificationText);

            mResourcesHeld++;
            if (LevelUp) mResourcesHeld++;
            if (mFed) Health += 10;
            mExperience++;
            mTimeSinceEat = 0;
        }

        private void Feed(string feedAction, string starveText)
        {
            if (mFed) mFed = false;
            else if (!EconomyManager.Instance.SeedDecrease(feedAction, false))
            {
                mFed = true;
                Health -= 20;
                if (Health < 0) base.Death();
                NotificationManager.AddNotification(starveText, 3);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            switch (mDirection)
            {
                case 0:
                    TextureOffset = mWalkUpAnimation.TextureOffset;
                    break;
                case 1:
                    TextureOffset = mWalkRightAnimation.TextureOffset;
                    break;
                case 2:
                    TextureOffset = mWalkDownAnimation.TextureOffset;
                    break;
                case 3:
                    TextureOffset = mWalkLeftAnimation.TextureOffset;
                    break;
            }
            if (Velocity == Vector2.Zero)
            {
                mTexture = mStandTextureList[mDirection];
            }
            else
            {
                switch (mDirection)
                {
                    case 0:
                        mTexture = mWalkUpAnimation.GetTexture();
                        break;
                    case 1:
                        mTexture = mWalkRightAnimation.GetTexture();
                        break;
                    case 2:
                        mTexture = mWalkDownAnimation.GetTexture();
                        break;
                    case 3:
                        mTexture = mWalkLeftAnimation.GetTexture();
                        break;
                }
            }

            Width = mTexture.Width;
            Height = mTexture.Height;
            if (!LevelUp && !(this is BAttackAnimal))
            {
                Width = (int)(Width * SmallSizeFactor);
                Height = (int)(Height * SmallSizeFactor);
                TextureOffset *= SmallSizeFactor;
                CollisionBoxOffset *= mSmallCollisionBox ? 1 : SmallSizeFactor;
                CollisionBoxSize *= mSmallCollisionBox ? 1 : SmallSizeFactor;
                mSmallCollisionBox = true;
            }
            else
            {
                CollisionBoxOffset /= mSmallCollisionBox ? SmallSizeFactor : 1;
                CollisionBoxSize /= mSmallCollisionBox ? SmallSizeFactor : 1;
                mSmallCollisionBox = false;
            }

            if (LevelUp)
            {
                mBackgroundColour = Color.Beige;
            }

            if (!HasPath)
            {
                Eat();
            }

            mTimeSinceEat += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }
    }
}
