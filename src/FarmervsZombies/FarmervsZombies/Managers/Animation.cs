using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Managers
{
    internal sealed class Animation
    {
        private static bool sInitialized;

        private readonly List<Texture2D> mFrames;
        private readonly float mFrameInterval;
        private int mFrameIndex;
        private int mFrameIndexOnce;
        private float mTime;
        private float mLastFrame;
        private bool mPlayAnimationOnce;

        public bool AnimationFinished
        {
            get
            {
                if (!mPlayAnimationOnce) return false;
                return mFrameIndexOnce == mFrames.Count - 1;
            }

            set
            {
                if (!mPlayAnimationOnce) mPlayAnimationOnce = true;
                if(value) mFrameIndexOnce = mFrames.Count - 1;
            }
        }

        public Vector2 TextureOffset { get; }
        public int Width { get; }

        public Animation(Texture2D frameSheet, int frameWidth, int frameHeight, float frameInterval) : this(frameSheet,
            frameWidth,
            frameHeight,
            0,
            0,
            frameInterval)
        {

        }

        public Animation(Texture2D frameSheet,
            int frameWidth,
            int frameHeight,
            float offsetX,
            float offsetY,
            float frameInterval)
        {
            if (!sInitialized) throw new InvalidOperationException("Animations are not initialized.");

            mFrameInterval = frameInterval;
            mFrames = TextureManager.GetList(frameSheet, frameWidth, frameHeight);
            TextureOffset = new Vector2(offsetX, offsetY);
            Width = frameWidth;
        }

        public static void Initialize()
        {
            sInitialized = true;
            Debug.WriteLine("Animations successfully initialized.");
        }

        public void Update(GameTime gameTime)
        {
            mTime += (float) gameTime.ElapsedGameTime.TotalSeconds;

            if (mTime - mLastFrame > mFrameInterval)
            {
                mFrameIndex = (mFrameIndex + 1) % mFrames.Count;
                if (mPlayAnimationOnce && mFrameIndexOnce < mFrames.Count - 1) mFrameIndexOnce++;
                mLastFrame = mTime;
            }
        }

        public Texture2D GetTexture()
        {
            if (!sInitialized) throw new InvalidOperationException("Animations are not initialized.");
            return mFrames[mFrameIndex];
        }

        public Texture2D GetTextureOnce()
        {
            if (!sInitialized) throw new InvalidOperationException("Animations are not initialized.");
            if (!mPlayAnimationOnce)
            {
                mPlayAnimationOnce = true;
            }

            return mFrames[mFrameIndexOnce];
        }

        /// <summary>
        /// Resets the whole animation.
        /// </summary>
        public void Reset()
        {
            mFrameIndex = 0;
            mFrameIndexOnce = 0;
            AnimationFinished = false;
            mPlayAnimationOnce = false;
        }


        public void ResetOnce()
        {
            mFrameIndex = 0;
            mFrameIndexOnce = 0;
            AnimationFinished = false;
        }
    }
}
