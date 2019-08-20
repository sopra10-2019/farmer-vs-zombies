using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using FarmervsZombies.Pathfinding.GameGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    internal sealed class TileMap
    {
        private readonly Tile[,] mTiles;
        private readonly int[,] mTileTextureIndices;

        private readonly Dictionary<Tile, Texture2D> mTextures = new Dictionary<Tile, Texture2D>();
        private List<Texture2D> mGrassTextures;
        private List<Texture2D> mWastelandTextures;
        private List<Texture2D> mWaterTextures;
        public Dictionary<Vector2, float> mPlowedTiles = new Dictionary<Vector2, float>();
        private const int Cooldown = 20;
        public int Height => mTiles.GetLength(0);
        public int Width => mTiles.GetLength(1);

        // If these flags aren't set before drawing an exception will be thrown
        private bool mInitialized;
        private bool mBuilt;

        public TileMap(int height, int width)
        {
            mTiles = new Tile[height, width];
            mTileTextureIndices = new int[height, width];
        }

        public void Build()
        {
            // Fill the map with random tiles, will be exchanged for future map building logic
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    mTiles[i, j] = Tile.Grass;
                    mTileTextureIndices[i, j] = 0;
                }
            }
            mBuilt = true;
        }

        public void LoadContent()
        {
            mGrassTextures = TextureManager.GetList("terrain", 32, 32);
            mWastelandTextures = TextureManager.GetList("wasteland", 32, 32);
            mWaterTextures = TextureManager.GetList("water", 32, 32);   
            mTextures[Tile.Grass] = TextureManager.GetTexture("terrain", 32, 32, 0);
            mTextures[Tile.Dirt] = TextureManager.GetTexture("terrain", 32, 32, 10);
            mTextures[Tile.Water] = TextureManager.GetTexture("water", 32, 32, 10);
            mTextures[Tile.Wasteland] = TextureManager.GetTexture("wasteland", 32, 32, 0);

            mInitialized = true;
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            // Map can not be drawn if not built and initialized
            if (!mBuilt || !mInitialized)
            {
                throw new InvalidOperationException("Map can not be drawn in current state");
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camTransform);
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    var texture = mTextures[GetTileType(i, j)];
                    var tileIndex = mTileTextureIndices[i, j];
                    switch (GetTileType(i, j))
                    {
                        case Tile.Grass:
                            texture = mGrassTextures[tileIndex];
                            break;
                        case Tile.Wasteland:
                            texture = mWastelandTextures[tileIndex];
                            break;
                        case Tile.Water:
                            texture = mWaterTextures[tileIndex];
                            break;
                    }
                    spriteBatch.Draw(texture, new Rectangle(32 * i, 32 * j, 32, 32), Color.White);
                }
            }
            spriteBatch.End();
        }

        public void SetTile(int i, int j, Tile tileType)
        {
            if (i < 0 || i >= Width || j < 0 || j >= Height) return;
            if (mTiles[i, j] == tileType) return;

            if (mTiles[i, j] == Tile.Water && !ObjectManager.Instance.CheckTile(new Vector2(i, j), typeof(IPathCollidable)).Any())
            {
                GameGraph.RemoveCollision(new Vector2(i, j));
            }
            else if (tileType == Tile.Water)
            {
                GameGraph.AddCollision(new Vector2(i, j));
            }
            mTiles[i, j] = tileType;

            for (var k = i - 1; k <= i + 1; k++)
            {
                for (var l = j - 1; l <= j + 1; l++)
                {
                    UpdateTileTexture(k, l);
                }
            }
        }

        private void UpdateTileTexture(int i, int j)
        {
            if (i < 0 || i >= Width || j < 0 || j >= Height) return;
            var tileType = GetTileType(i, j);
            if (tileType == Tile.Grass || tileType == Tile.Wasteland)
            {
                var top = i < Height && i >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i, j - 1) == Tile.Dirt;
                var topRight = i + 1 < Height && i + 1 >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i + 1, j - 1) == Tile.Dirt;
                var right = i + 1 < Height && i + 1 >= 0 && j < Width && j >= 0 && GetTileType(i + 1, j) == Tile.Dirt;
                var bottomRight = i + 1 < Height && i + 1 >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i + 1, j + 1) == Tile.Dirt;
                var bottom = i < Height && i >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i, j + 1) == Tile.Dirt;
                var bottomLeft = i - 1 < Height && i - 1 >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i - 1, j + 1) == Tile.Dirt;
                var left = i - 1 < Height && i - 1 >= 0 && j < Width && j >= 0 && GetTileType(i - 1, j) == Tile.Dirt;
                var topLeft = i - 1 < Height && i - 1 >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i - 1, j - 1) == Tile.Dirt;
                (bool, bool, bool, bool) val1 = (top, right, bottom, left);
                (bool, bool, bool, bool) val2 = (topRight, bottomRight, bottomLeft, topLeft);

                switch (val1)
                {
                    case var t when t == (false, false, false, false):
                        mTileTextureIndices[i, j] = 0;
                        switch (val2)
                        {
                            case var t2 when t2 == (false, true, false, false):
                                mTileTextureIndices[i, j] = 6;
                                break;
                            case var t2 when t2 == (false, false, true, false):
                                mTileTextureIndices[i, j] = 8;
                                break;
                            case var t2 when t2 == (true, false, false, false):
                                mTileTextureIndices[i, j] = 12;
                                break;
                            case var t2 when t2 == (false, false, false, true):
                                mTileTextureIndices[i, j] = 14;
                                break;
                            case var t2 when t2 == (false, true, false, true):
                                mTileTextureIndices[i, j] = 15;
                                break;
                            case var t2 when t2 == (false, true, true, false):
                                mTileTextureIndices[i, j] = 16;
                                break;
                            case var t2 when t2 == (true, false, true, false):
                                mTileTextureIndices[i, j] = 17;
                                break;
                            case var t2 when t2 == (true, true, false, false):
                                mTileTextureIndices[i, j] = 18;
                                break;
                            case var t2 when t2 == (true, true, true, true):
                                mTileTextureIndices[i, j] = 19;
                                break;
                            case var t2 when t2 == (false, false, true, true):
                                mTileTextureIndices[i, j] = 20;
                                break;
                            case var t2 when t2 == (true, false, true, true):
                                mTileTextureIndices[i, j] = 21;
                                break;
                            case var t2 when t2 == (true, false, false, true):
                                mTileTextureIndices[i, j] = 22;
                                break;
                            case var t2 when t2 == (true, true, false, true):
                                mTileTextureIndices[i, j] = 23;
                                break;
                            case var t2 when t2 == (false, true, true, true):
                                mTileTextureIndices[i, j] = 24;
                                break;
                            case var t2 when t2 == (true, true, true, false):
                                mTileTextureIndices[i, j] = 26;
                                break;
                        }
                        break;
                    case var t when t == (true, false, false, true):
                        mTileTextureIndices[i, j] = 1;
                        if (bottomRight) mTileTextureIndices[i, j] = 34;
                        break;
                    case var t when t == (true, true, false, false):
                        mTileTextureIndices[i, j] = 2;
                        if (bottomLeft) mTileTextureIndices[i, j] = 35;
                        break;
                    case var t when t == (false, false, true, true):
                        mTileTextureIndices[i, j] = 4;
                        if (topRight) mTileTextureIndices[i, j] = 37;
                        break;
                    case var t when t == (false, true, true, false):
                        mTileTextureIndices[i, j] = 5;
                        if (topLeft) mTileTextureIndices[i, j] = 38;
                        break;
                    case var t when t == (false, false, true, false):
                        mTileTextureIndices[i, j] = 7;
                        if (topLeft && topRight) mTileTextureIndices[i, j] = 40;
                        else if (topLeft) mTileTextureIndices[i, j] = 41;
                        else if (topRight) mTileTextureIndices[i, j] = 39;
                        break;
                    case var t when t == (false, true, false, false):
                        mTileTextureIndices[i, j] = 9;
                        if (topLeft && bottomLeft) mTileTextureIndices[i, j] = 43;
                        else if (topLeft) mTileTextureIndices[i, j] = 44;
                        else if (bottomLeft) mTileTextureIndices[i, j] = 42;
                        break;
                    case var t when t == (false, false, false, true):
                        mTileTextureIndices[i, j] = 11;
                        if (topRight && bottomRight) mTileTextureIndices[i, j] = 46;
                        else if (bottomRight) mTileTextureIndices[i, j] = 45;
                        else if (topRight) mTileTextureIndices[i, j] = 47;
                        break;
                    case var t when t == (true, false, false, false):
                        mTileTextureIndices[i, j] = 13;
                        if (bottomLeft && bottomRight) mTileTextureIndices[i, j] = 49;
                        else if (bottomLeft) mTileTextureIndices[i, j] = 50;
                        else if (bottomRight) mTileTextureIndices[i, j] = 48;
                        break;

                    case var t when t == (true, false, true, true):
                        mTileTextureIndices[i, j] = 27;
                        break;
                    case var t when t == (true, false, true, false):
                        mTileTextureIndices[i, j] = 28;
                        break;
                    case var t when t == (true, true, true, false):
                        mTileTextureIndices[i, j] = 29;
                        break;
                    case var t when t == (true, true, false, true):
                        mTileTextureIndices[i, j] = 30;
                        break;
                    case var t when t == (false, true, false, true):
                        mTileTextureIndices[i, j] = 31;
                        break;
                    case var t when t == (false, true, true, true):
                        mTileTextureIndices[i, j] = 32;
                        break;
                    case var t when t == (true, true, true, true):
                        mTileTextureIndices[i, j] = 33;
                        break;
                }
            }

            if (tileType == Tile.Water)
            {
                var top = i < Height && i >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i, j - 1) == Tile.Grass;
                var topRight = i + 1 < Height && i + 1 >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i + 1, j - 1) == Tile.Grass;
                var right = i + 1 < Height && i + 1 >= 0 && j < Width && j >= 0 && GetTileType(i + 1, j) == Tile.Grass;
                var bottomRight = i + 1 < Height && i + 1 >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i + 1, j + 1) == Tile.Grass;
                var bottom = i < Height && i >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i, j + 1) == Tile.Grass;
                var bottomLeft = i - 1 < Height && i - 1 >= 0 && j + 1 < Width && j + 1 >= 0 && GetTileType(i - 1, j + 1) == Tile.Grass;
                var left = i - 1 < Height && i - 1 >= 0 && j < Width && j >= 0 && GetTileType(i - 1, j) == Tile.Grass;
                var topLeft = i - 1 < Height && i - 1 >= 0 && j - 1 < Width && j - 1 >= 0 && GetTileType(i - 1, j - 1) == Tile.Grass;
                (bool, bool, bool, bool) val1 = (top, right, bottom, left);
                (bool, bool, bool, bool) val2 = (topRight, bottomRight, bottomLeft, topLeft);

                switch (val1)
                {
                    case var t when t == (false, false, false, false):
                        mTileTextureIndices[i, j] = 10;
                        switch (val2)
                        {
                            case var t2 when t2 == (false, true, false, false):
                                mTileTextureIndices[i, j] = 1;
                                break;
                            case var t2 when t2 == (false, false, true, false):
                                mTileTextureIndices[i, j] = 2;
                                break;
                            case var t2 when t2 == (true, false, false, false):
                                mTileTextureIndices[i, j] = 4;
                                break;
                            case var t2 when t2 == (false, false, false, true):
                                mTileTextureIndices[i, j] = 5;
                                break;
                        }
                        break;

                    case var t when t == (true, false, false, true):
                        mTileTextureIndices[i, j] = 6;
                        break;
                    case var t when t == (true, false, false, false):
                        mTileTextureIndices[i, j] = 7;
                        break;
                    case var t when t == (true, true, false, false):
                        mTileTextureIndices[i, j] = 8;
                        break;
                    case var t when t == (false, false, false, true):
                        mTileTextureIndices[i, j] = 9;
                        break;
                    case var t when t == (false, true, false, false):
                        mTileTextureIndices[i, j] = 11;
                        break;
                    case var t when t == (false, false, true, true):
                        mTileTextureIndices[i, j] = 12;
                        break;
                    case var t when t == (false, false, true, false):
                        mTileTextureIndices[i, j] = 13;
                        break;
                    case var t when t == (false, true, true, false):
                        mTileTextureIndices[i, j] = 14;
                        break;
                }
            }

        }

        public Tile GetTileType(int i, int j)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not get tile in current state.");
            }

            if (i < Height && i >= 0 && j < Width && j >= 0)
            {
                return mTiles[i, j];
            }

            throw new InvalidOperationException("Map index out of bounds.");
        }
        
        public void PlowTile(int i, int j)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not plow tile in current state.");
            }

            if (i < Height && i >= 0 && j < Width && j >= 0 && mTiles[i, j] == Tile.Grass)
            {
                SetTile(i, j, Tile.Dirt);
                mPlowedTiles.Add(new Vector2(i, j), 0);
            }
        }

        public void PlantGrass(int i, int j, bool saveLoad = false)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not plant grass in current state.");
            }

            if (i < Height && i >= 0 && j < Width && j >= 0 && (mTiles[i, j] != Tile.Wasteland || saveLoad))
            {
                SetTile(i, j, Tile.Grass);
            }
        }

        public void RemoveWasteland(int i, int j)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not plant grass in current state.");
            }

            if (i < Height && i >= 0 && j < Width && j >= 0 && mTiles[i, j] == Tile.Wasteland)
            {
                SetTile(i, j, Tile.Grass);
            }
        }

        public void PlantWater(int i, int j)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not plant water in current state.");
            }

            SetTile(i, j, Tile.Water);
        }

        public void PlantWasteland(int i, int j)
        {
            if (!mBuilt)
            {
                throw new InvalidOperationException("Can not plant wasteland in current state.");
            }

            SetTile(i, j, Tile.Wasteland);

            foreach (var wheat in ObjectManager.Instance.CheckTile(new Vector2(i, j), typeof(Wheat)))
            {
                ObjectManager.Instance.QueueRemoval(wheat);
            }
        }

        public void Update(GameTime gameTime)
        {
            var clone = new Dictionary<Vector2, float>();
            foreach (var obj in mPlowedTiles)
            {
                clone.Add(obj.Key, obj.Value);
            }
            foreach (var obj in mPlowedTiles.Keys)
            {
                if (mPlowedTiles[obj] >= Cooldown)
                {
                    SetTile((int)obj.X, (int)obj.Y, Tile.Grass);
                    clone.Remove(obj);
                }
                else
                {
                    clone[obj] += gameTime.ElapsedGameTime.Milliseconds / 1000.0f;
                }
            }
            mPlowedTiles = clone;
        }

        public void ToXml(XmlDocument doc, XmlElement xml)
        {
            var counter = 0;
            var element = doc.CreateElement("Map");
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (mTiles[i, j] != 0 && !mPlowedTiles.ContainsKey(new Vector2(i, j)))
                    {
                        element.SetAttribute("X" + (int)mTiles[i, j] + "X" + counter, i + "," + j);
                        xml.AppendChild(element);
                        counter++;
                    }
                }
            }
        }

        public void Load(string path)
        {
            if (File.Exists(path))
            {
                var settings = new XmlReaderSettings
                {
                    Async = true
                };

                using (var xmlReader = XmlReader.Create(path, settings))
                {
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType != XmlNodeType.Element) continue;
                        if (!xmlReader.HasAttributes) continue;
                        for (var i = 0; i < mTiles.GetLength(0); i++)
                        {
                            for (var j = 0; j < mTiles.GetLength(1); j++)
                            {
                                try
                                {
                                    var index = xmlReader.GetAttribute("pos" + i + j);
                                    if (index != null)
                                    {
                                        SetTile(i, j, (Tile) int.Parse(index));
                                    }
                                }
                                catch (FormatException e)
                                {
                                    Debug.WriteLine(e.Message);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Build();
            }

            mBuilt = true;
        }
    }
}
