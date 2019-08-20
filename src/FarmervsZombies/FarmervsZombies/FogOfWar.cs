using System;
using System.Diagnostics;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    internal sealed class FogOfWar
    {
        private FogStates[,] mVisible;
        private readonly int[,] mTime;
        private readonly int mVisibleX;
        private readonly int mVisibleY;
        private Texture2D mNotDiscovered;

        private enum FogStates
        {
            Discovered,
            NotDiscovered
        }

        // x and y for the size of the fog
        public FogOfWar(int x, int y)
        {
            // setting the size of the matrix and all fields to notDiscovered
            mVisible = new FogStates[x, y];
            for (var r = 0; r < x; r++)
            {
                for (var s = 0; s < y; s++)
                {
                    mVisible[r, s] = FogStates.NotDiscovered;
                }
            }
            mTime = new int[x,y];
            for (var r = 0; r < x; r++)
            {
                for (var s = 0; s < y; s++)
                {
                    mVisible[r, s] = 0;
                    mTime[r, s] = 600;
                }
            }
            mVisibleX = x;
            mVisibleY = y;
        }

        public void LoadContent(ContentManager content)
        {
            try
            {
                mNotDiscovered = content.Load<Texture2D>("FogOfWar\\notdiscovered");
            }
            catch (Exception e)
            {
                Debug.WriteLine($"FogOfWar\\notdiscovered could not be loaded : {e}\n");
            }
        }

        public void Update(Vector2 farmerPos)
        {
            for (var r = 0; r < mVisibleX; r++)
            {
                for (var s = 0; s < mVisibleY; s++)
                {
                    if (mTime[r, s] > 0)
                    {
                        mTime[r, s] -= 1;
                        if (mTime[r, s] == 0)
                        {
                            mVisible[r, s] = FogStates.NotDiscovered;
                        }
                    }
                }
            }
            // TODO : make all fields which have a certain distance to farmer visible.
            // starting the update loop when farmer is created
            foreach (var obj in ObjectManager.Instance.GetAnimals())
            {
                if (obj.Team)
                {
                    for (var x = (int)(obj.Position.X - 5); x < (int)(obj.Position.X + 5); x++)
                    {
                        for (var y = (int)(obj.Position.Y - 5); y < (int)(obj.Position.Y + 5); y++)
                        {
                            if (x >= 0 && x < mVisibleX && y >= 0 && y < mVisibleY)
                            {
                                mVisible[x, y] = FogStates.Discovered;
                                mTime[x, y] = 600;
                            }
                        }
                    }
                }
            }
            for (var x = (int)(farmerPos.X - 10); x < (int)(farmerPos.X + 10); x++)
            {
                for (var y = (int) (farmerPos.Y - 10); y < (int)(farmerPos.Y + 10); y++)
                {
                    if (x >= 0 && x < mVisibleX && y >= 0 && y < mVisibleY)
                    {
                        mVisible[x, y] = FogStates.Discovered;
                        mTime[x, y] = 600;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camTransform);
            // looping through the array and drawing the fog
            for (var x = 0; x < mVisibleX; x++)
            {
                for (var y = 0; y < mVisibleY; y++)
                {
                    if (mVisible[x, y] == FogStates.NotDiscovered)
                    {
                        spriteBatch.Draw(mNotDiscovered, new Rectangle(32 * x, 32 * y, 32, 32), Color.White);
                    }
                }
            }   
            spriteBatch.End();
        }

        public void Reset()
        {
            // setting the size of the matrix and all fields to notDiscovered
            mVisible = new FogStates[mVisibleX, mVisibleY];
            for (var r = 0; r < mVisibleX; r++)
            {
                for (var s = 0; s < mVisibleY; s++)
                {
                    mVisible[r, s] = FogStates.NotDiscovered;
                }
            }
        }

        public void SetField(int x, int y)
        {
            mVisible[x,y] = FogStates.Discovered;
        }

        public bool FieldIsVisible(int x, int y)
        {
            return mVisible[x, y] == FogStates.Discovered;
        }

        public void ToXml(XmlDocument doc, XmlElement xml)
        {
            var counter = 0;
            var element = doc.CreateElement("FogOfWar");
            for (var r = 0; r < mVisibleX; r++)
            {
                for (var s = 0; s < mVisibleY; s++)
                {
                    if (mVisible[r,s] == FogStates.Discovered)
                    {
                        var pos = "(" + r.ToString() + ","+s.ToString()+ ")";
                        element.SetAttribute("pos"+counter.ToString(), pos);
                        counter++;
                    }
                }
            }
            xml.AppendChild(element);
        }
    }
}
