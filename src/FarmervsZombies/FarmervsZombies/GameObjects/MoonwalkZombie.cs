using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class MoonwalkZombie : Zombie
    {
        private readonly Vector2 mStartPosition;
        private Vector2 mTarget;
        private const float WalkDistance = 3;
        private new const float Speed = 1.5f;
        private bool mFirst = true;

        public MoonwalkZombie(float positionX, float positionY, bool imposter = false, bool randomWalk = false, float randomWalkDistance = 1, int loadhealth = 100)
            : base(positionX, positionY, imposter ? AnimationManager.NecromancerWalkRight : AnimationManager.ZombieWalkRight,
                imposter ? AnimationManager.NecromancerWalkLeft : AnimationManager.ZombieWalkLeft, null, null, true, randomWalk, randomWalkDistance)
        {
            Health = loadhealth;
            mStartPosition = new Vector2(positionX, positionY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (mDirection)
            {
                case 1:
                    mTexture = mWalkRightAnimation.GetTexture();
                    TextureOffset = mWalkRightAnimation.TextureOffset;
                    break;
                case 3:
                    mTexture = mWalkLeftAnimation.GetTexture();
                    TextureOffset = mWalkLeftAnimation.TextureOffset;
                    break;
            }

            if (mFirst)
            {
                mTarget = new Vector2(mStartPosition.X - WalkDistance, mStartPosition.Y);
                mDirection = 3;
                mFirst = false;
            }
            else if (Vector2.Distance(Position, mTarget) < 0.1f)
            {
                if (mDirection == 1)
                {
                    mTarget = new Vector2(mStartPosition.X - WalkDistance, mStartPosition.Y);
                    mDirection = 3;
                }
                else
                {
                    mTarget = new Vector2(mStartPosition.X + WalkDistance, mStartPosition.Y);
                    mDirection = 1;
                }
            }

            var dist = mTarget - Position;
            if (dist != Vector2.Zero) dist.Normalize();
            Position += dist * Speed / (1000f / gameTime.ElapsedGameTime.Milliseconds);
        }
    }
}
