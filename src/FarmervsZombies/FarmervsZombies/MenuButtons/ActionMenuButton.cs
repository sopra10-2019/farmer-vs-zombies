using System;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.MenuButtons
{
    internal sealed class ActionMenuButton
    {
        private static readonly Texture2D sTexture = TextureManager.GetTexture("buttons", new Rectangle(96, 192, 96, 96));
        private static readonly Texture2D sTextureHover = TextureManager.GetTexture("buttons", new Rectangle(96, 288, 96, 96));
        private static readonly Texture2D sTextureClicked = TextureManager.GetTexture("buttons", new Rectangle(96, 384, 96, 96));
        private readonly Texture2D mIconTexture;
        public const int Width = 30;
        private const int Height = 30;
        private const int Padding = 3;
        private int mIconWidth;
        private int mIconHeight;
        private Vector2 mWorldPosition;
        private Vector2 mIconWorldPosition;
        private readonly Action mAction;
        public BtnStates mButtonState = BtnStates.Default;
        public Tooltip Tooltip { get; }

        public enum BtnStates
        {
            Default,
            Clicked,
            Hover
        }


        private ActionMenuButton(Vector2 worldPosition, Texture2D icon, Action action)
        {
            mWorldPosition = worldPosition;
            mIconTexture = icon;
            mIconWidth = Width - 2 * Padding;
            mIconHeight = Height - 2 * Padding;
            mAction = action;
            mIconWorldPosition = new Vector2(mWorldPosition.X + (float)Padding / 32, mWorldPosition.Y + (float)Padding / 32);
            InputManager.AnyActionEvent += ClickButton;
        }

        public ActionMenuButton(Vector2 worldPosition, Texture2D icon, Action action, string tooltipText) : this(worldPosition,icon, action)
        {
            Tooltip = new Tooltip(tooltipText);
        }

        public void Update(InputState inputState)
        {
            var mouseWorldPosition = inputState.mMouseWorldPosition;

            if (mouseWorldPosition.X > mWorldPosition.X && mouseWorldPosition.X < mWorldPosition.X + (float)Width/32 &&
                mouseWorldPosition.Y > mWorldPosition.Y && mouseWorldPosition.Y < mWorldPosition.Y + (float)Height/32)
            {
                mButtonState = BtnStates.Hover;
            }
            else
            {
                mButtonState = BtnStates.Default;
            }

            if (Tooltip != null) Tooltip.Visible = (mButtonState == BtnStates.Hover || mButtonState == BtnStates.Clicked);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            Texture2D currentTexture;
            switch (mButtonState)
            {
                case BtnStates.Hover:
                    currentTexture = sTextureHover;
                    break;
                case BtnStates.Clicked:
                    currentTexture = sTextureClicked;
                    break;
                default:
                    currentTexture = sTexture;
                    break;

            }
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camTransform);
            // Draw Button Texture
            spriteBatch.Draw(currentTexture,
                new Rectangle((int)(mWorldPosition.X * 32), (int)(mWorldPosition.Y * 32), Width, Height),
                null,
                Color.White);

            // Draw Icon
            if (mIconTexture == null) return;
            spriteBatch.Draw(mIconTexture,
                new Rectangle((int)(mIconWorldPosition.X * 32), (int)(mIconWorldPosition.Y * 32), mIconWidth, mIconHeight),
                null,
                Color.White);
            spriteBatch.End();
        }

        public void SetPosition(Vector2 worldPosition)
        {
            mWorldPosition = worldPosition;
            mIconWidth = Width - 2 * Padding;
            mIconHeight = Height - 2 * Padding;
            mIconWorldPosition = new Vector2(mWorldPosition.X + (float)Padding / 32, mWorldPosition.Y + (float)Padding / 32);
        }

        private void ClickButton(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.MouseClick) || mButtonState != BtnStates.Hover) return;
            mButtonState = BtnStates.Clicked;
            mAction();
        }
    }
}
