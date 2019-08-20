using System;
using System.Collections.Generic;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    internal sealed class UnitSelectionBox
    {
        private static UnitSelectionBox sInstance;

        public float Height { get; set; }
        public float Width { get; set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public static UnitSelectionBox Instance => sInstance ?? (sInstance = new UnitSelectionBox());

        public void Update(Vector2 mousePosition1)
        {
            var mouseWorldPosition = InputManager.GetCurrentInputState().mMouseWorldPosition;
            float width;
            float height;
            if (mousePosition1.X < mouseWorldPosition.X && mousePosition1.Y > mouseWorldPosition.Y)
            {
                width = Math.Abs((mousePosition1.X * 32) - (mouseWorldPosition.X * 32));
                height = Math.Abs((mouseWorldPosition.Y * 32) - (mousePosition1.Y * 32));
                X = mousePosition1.X;
                Y = mouseWorldPosition.Y;
                Instance.Height = height / 32;
                Instance.Width = width / 32;
            }
            else if (mousePosition1.X < mouseWorldPosition.X && mousePosition1.Y < mouseWorldPosition.Y)
            {
                width = Math.Abs((mousePosition1.X * 32) - (mouseWorldPosition.X * 32));
                height = Math.Abs((mouseWorldPosition.Y * 32) - (mousePosition1.Y * 32));
                X = mousePosition1.X;
                Y = mousePosition1.Y;
                Instance.Height = height / 32;
                Instance.Width = width / 32;
            }
            else if (mousePosition1.X > mouseWorldPosition.X && mousePosition1.Y > mouseWorldPosition.Y)
            {
                width = Math.Abs((mousePosition1.X * 32) - (mouseWorldPosition.X * 32));
                height = Math.Abs((mouseWorldPosition.Y * 32) - (mousePosition1.Y * 32));
                X = mouseWorldPosition.X;
                Y = mouseWorldPosition.Y;
                Instance.Height = height / 32;
                Instance.Width = width / 32;
            }
            else if (mousePosition1.X > mouseWorldPosition.X && mousePosition1.Y < mouseWorldPosition.Y)
            {
                width = Math.Abs((mousePosition1.X * 32) - (mouseWorldPosition.X * 32));
                height = Math.Abs((mouseWorldPosition.Y * 32) - (mousePosition1.Y * 32));
                X = mouseWorldPosition.X;
                Y = mousePosition1.Y;
                Instance.Height = height / 32;
                Instance.Width = width / 32;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            var width = Instance.Width * 32;
            var height = Instance.Height * 32;
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camTransform);
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int)(Instance.X * 32), (int)((Instance.Y + Height) * 32), (int)width, 1), Color.White);
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int)(Instance.X * 32), (int)(Instance.Y * 32), (int)width, 1), Color.White);
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int)((Instance.X + Width) * 32), (int)(Instance.Y * 32), 1, (int)height), Color.White);
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int)(Instance.X * 32), (int)(Instance.Y * 32), 1, (int)height), Color.White);

            spriteBatch.End();
        }

        public static bool Select(IEnumerable<BGameObject> list, bool box, Vector2 mousePosition)
        {
            if (box)
            {
                BGameObject selected = null;
                foreach (var i in list)
                {
                    if (selected == null)
                    {
                        selected = i;
                    }else if(Math.Abs(mousePosition.X - i.Position.X) + Math.Abs(mousePosition.Y - i.Position.Y) < Math.Abs(mousePosition.X - selected.Position.X) + Math.Abs(mousePosition.Y - selected.Position.Y))
                    {
                        selected = i;
                    }
                }
                selected?.Select();
                if (selected is Wheat wheat)
                {
                    if (wheat.Type == Wheat.WheatType.Corn)
                    {
                        if (wheat.Position.Y > (mousePosition.Y))
                        {
                            wheat.Deselect();
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                foreach (var i in list)
                {
                    i?.Select();
                }

                return true;
            }
        }
    }
}