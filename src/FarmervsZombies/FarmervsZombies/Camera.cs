using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies
{
    internal sealed class Camera
    {
        private float Zoom { get; set; }
        private Vector2 Position { get; set; }
        private Rectangle Bounds { get; set; }
        public Rectangle VisibleArea { get; private set; }
        public static Matrix Transform { get; private set; }

        private bool mFollowFarmer = true;
        private bool mLocked;
        public bool FollowFarmer
        {
            private get => mFollowFarmer;
            set
            {
                mFollowFarmer = value;
                if (value)
                {
                    ObjectManager.Instance.GetFarmer().Select();
                }
            }
        }

        private readonly int mMapWidth;
        private readonly int mMapHeight;
        private float mCurrentMouseWheelValue, mPreviousMouseWheelValue;
        private bool mSubscribedToInputManager;

        public Camera(Viewport viewport)
        {
            // Initialize the Camera with Screen and Map Size
            Bounds = viewport.Bounds;
            // Debug Purpose
            // Console.WriteLine("Viewport Size = " + Bounds.Size);
            Zoom = 1f;
            var screenCenter = new Vector2(Bounds.Width / 2f, Bounds.Height / 2f);
            Position = screenCenter;
            mMapWidth = Game1.sTileMap.Width;
            mMapHeight = Game1.sTileMap.Height;
        }

        private void UpdateVisibleArea()
        {
            // Compute the visible Area of this Camera
            var inverseViewMatrix = Matrix.Invert(Transform);

            var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var tr = Vector2.Transform(new Vector2(Bounds.X, 0), inverseViewMatrix);
            var bl = Vector2.Transform(new Vector2(0, Bounds.Y), inverseViewMatrix);
            var br = Vector2.Transform(new Vector2(Bounds.Width, Bounds.Height), inverseViewMatrix);

            var min = new Vector2(
                MathHelper.Min(tl.X, MathHelper.Min(tr.X, MathHelper.Min(bl.X, br.X))),
                MathHelper.Min(tl.Y, MathHelper.Min(tr.Y, MathHelper.Min(bl.Y, br.Y))));
            var max = new Vector2(
                MathHelper.Max(tl.X, MathHelper.Max(tr.X, MathHelper.Max(bl.X, br.X))),
                MathHelper.Max(tl.Y, MathHelper.Max(tr.Y, MathHelper.Max(bl.Y, br.Y))));
            VisibleArea = new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
        }

        private void UpdateMatrix()
        {
            // Transform the momentary Position and Zoom into a Transform-Matrix which can be used to draw
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(Bounds.Width * 0.5f, Bounds.Height * 0.5f, 0));
            UpdateVisibleArea();
        }

        private void MoveCamera(Vector2 movePosition)
        {
            if (mLocked) return;

            // Update the Position of the Camera based on the difference to the last Position and MapConstraints
            var newPosition = Position + movePosition;
            Position = newPosition;

            var mapConstraintX = (mMapWidth * 32 + Bounds.Width / 32 / 2) - 32 * Zoom;
            var mapConstraintY = (mMapHeight * 32 + Bounds.Height / 32 / 2) - 32 * Zoom;

            if (Position.X < -10)
            {
                newPosition.X = -10;
                Position = newPosition;
            }

            if (Position.X > mapConstraintX)
            {
                newPosition.X = mapConstraintX;
                Position = newPosition;
            }

            if (Position.Y < -10)
            {
                newPosition.Y = -10;
                Position = newPosition;
            }

            if (Position.Y > mapConstraintY)
            {
                newPosition.Y = mapConstraintY;
                Position = newPosition;
            }
        }

        private void AdjustZoom(float zoomAmount)
        {
            Zoom += zoomAmount;
            if (Zoom < .6f)
            {
                Zoom = .6f;
            }
            if (Zoom > 2f)
            {
                Zoom = 2f;
            }
        }

        public void UpdateCamera(Viewport bounds, InputState inputState)
        {
            // Compute the Inputs to change the Camera Position and Transform Matrix accordingly
            Bounds = bounds.Bounds;

            // Subscribe to InputManager
            if (InputManager.Initialized && !mSubscribedToInputManager)
            {
                InputManager.AnyActionEvent += ToggleFollowFarmer;
                mSubscribedToInputManager = true;
            }

            if (!inputState.IsKeyPressed(Keys.Space) && !inputState.IsKeyPressed(Keys.F1))
            {
                UpdateMatrix();
            }
            if (mFollowFarmer)
            {
                var farmer = ObjectManager.Instance.GetFarmer();
                if (farmer != null)
                {
                    Position = farmer.Position * 32 - new Vector2(Bounds.Width / 2f, Bounds.Height /2f) / 32;
                }
                UpdateMatrix();
            }

            var cameraMovement = Vector2.Zero;
            int moveSpeed;

            if (Zoom > .8f)
            {
                moveSpeed = 15;
            }
            else if (Zoom < .8f && Zoom >= .6f)
            {
                moveSpeed = 20;
            }
            else if (Zoom < .6f && Zoom > .35f)
            {
                moveSpeed = 25;
            }
            else if (Zoom <= .35f)
            {
                moveSpeed = 30;
            }
            else
            {
                moveSpeed = 10;
            }

            if (inputState.IsKeyPressed(Keys.Up) || inputState.mMouseWindowPosition.Y <= Bounds.Top + 5)
            {
                mFollowFarmer = false;
                cameraMovement.Y = -moveSpeed;
            }

            if (inputState.IsKeyPressed(Keys.Down) || inputState.mMouseWindowPosition.Y >= Bounds.Bottom - 5)
            {
                mFollowFarmer = false;
                cameraMovement.Y = moveSpeed;
            }

            if (inputState.IsKeyPressed(Keys.Left) || inputState.mMouseWindowPosition.X <= Bounds.Left + 5)
            {
                mFollowFarmer = false;
                cameraMovement.X = -moveSpeed;
            }

            if (inputState.IsKeyPressed(Keys.Right) || inputState.mMouseWindowPosition.X >= Bounds.Right - 5)
            {
                mFollowFarmer = false;
                cameraMovement.X = moveSpeed;
            }

            mPreviousMouseWheelValue = mCurrentMouseWheelValue;
            mCurrentMouseWheelValue = inputState.mMouseScrollWheelValue;

            if (mCurrentMouseWheelValue > mPreviousMouseWheelValue)
            {
                AdjustZoom(.05f);
            }

            if (mCurrentMouseWheelValue < mPreviousMouseWheelValue)
            {
                AdjustZoom(-.05f);
            }

            MoveCamera(cameraMovement);
        }

        private void ToggleFollowFarmer(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ToggleFollowFarmer)) return;
            if (!FollowFarmer)
            {
                ObjectManager.Instance.ClearSelected();
            }
            FollowFarmer = !FollowFarmer;
        }

        public void SetEndScreenPosition()
        {
            Zoom = 1.85f;
            Position = new Vector2(1250, 1020);
        }

        public void Lock()
        {
            mLocked = true;
        }

        public void Unlock()
        {
            mLocked = false;
        }
    }
}
