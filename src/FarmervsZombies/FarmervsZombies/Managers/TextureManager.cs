using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Managers
{
    internal static class TextureManager
    {
        private static readonly Dictionary<string, Texture2D> sTextureList = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<(string, int, int, int), Texture2D> sUsedTextures = new Dictionary<(string, int, int, int), Texture2D>();
        private static readonly Dictionary<(Texture2D, int, int), List<Texture2D>> sUsedTextureLists = new Dictionary<(Texture2D, int, int), List<Texture2D>>();

        private static GraphicsDevice sGraphicsDevice;
        private static bool sInitialized;
        public static ContentManager Content { get; private set; }

        /// <summary>
        /// Initializes the TextureManager
        /// </summary>
        public static void Initialize(GraphicsDevice graphics)
        {
            sGraphicsDevice = graphics;
            sInitialized = true;
            Debug.WriteLine("TextureManager successfully initialized.");
        }

        /// <summary>
        /// Automatically loads all Textures in Content/Textures into the TextureManager.
        /// </summary>
        public static void LoadContent(ContentManager content)
        {
            Content = content;
            var textureFolder = new DirectoryInfo(Path.Combine(content.RootDirectory, @"Textures"));
            var textureFileList = textureFolder.GetFiles("*.xnb");

            foreach (var f in textureFileList)
            {
                var textureName = Path.GetFileNameWithoutExtension(f.Name);
                try
                {
                    sTextureList[textureName] = content.Load<Texture2D>("Textures/" + textureName);
                    sTextureList[textureName].Name = textureName;
                    Debug.WriteLine($"Successfully loaded texture \"{textureName}\"");
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to load texture \"{textureName}\". ({e.GetType().Name})");
                }
            }
        }

        /// <summary>
        /// Returns a requested texture in from the Content/Textures folder.
        /// </summary>
        /// <param name="textureName">The name of the texture file in Content/Textures.</param>
        /// <returns>Returns a Texture2D or null.</returns>
        public static Texture2D GetTexture(string textureName)
        {
            if (!sInitialized)
            {
                throw new InvalidOperationException("TextureManager not initialized.");
            }
            if (!sTextureList.ContainsKey(textureName))
            {
                throw new ArgumentException($"Texture \"{textureName}\" does not exist.");
            }

            return sTextureList[textureName];
        }

        /// <summary>
        /// Returns a requested tile as a Texture2D from a texture in the Content/Textures folder.
        /// </summary>
        /// <param name="textureName">The name of the texture file in Content/Textures.</param>
        /// <param name="tileWidth">The width of the tile.</param>
        /// <param name="tileHeight">The height of the tile.</param>
        /// <param name="tileIndex">The index of the tile in the texture, starting from 0 and counting from the
        /// top left tile and ending with the bottom right tile.</param>
        /// <returns>Returns a Texture2D or null.</returns>
        public static Texture2D GetTexture(string textureName, int tileWidth, int tileHeight, int tileIndex)
        {
            if (!sInitialized)
            {
                throw new InvalidOperationException("TextureManager not initialized.");
            }
            if (!sTextureList.ContainsKey(textureName))
            {
                throw new ArgumentException($"Texture \"{textureName}\" does not exist.");
            }

            if (sUsedTextures.ContainsKey((textureName, tileWidth, tileHeight, tileIndex)))
                return sUsedTextures[(textureName, tileWidth, tileHeight, tileIndex)];
            var texture = sTextureList[textureName];
            var textureWidth = texture.Width;
            var cols = textureWidth / tileWidth;

            var posX = tileWidth * (tileIndex % cols);
            var posY = tileHeight * (tileIndex / cols);
            var sourceRectangle = new Rectangle(posX, posY, tileWidth, tileHeight);
            var data = new Color[tileWidth * tileHeight];
            try
            {
                texture.GetData(0, sourceRectangle, data, 0, tileWidth * tileHeight);
            }
            catch
            {
                Debug.WriteLine("cols: " + cols + ", tileIndex: " + tileIndex);
                Debug.WriteLine("posX: " + posX + ", posY: " + posY + ", width: " + tileWidth + ", height: " + tileHeight);
            }

            var newTexture = new Texture2D(sGraphicsDevice, tileWidth, tileHeight);
            newTexture.SetData(data);
            sUsedTextures[(textureName, tileWidth, tileHeight, tileIndex)] = newTexture;

            return newTexture;
        }

        /// <summary>
        /// Returns a requested tile as a Texture2D from a texture in the Content/Textures folder.
        /// </summary>
        /// <param name="textureName">The name of the texture file in Content/Textures.</param>
        /// <param name="sourceRectangle">The source rectangle.</param>
        /// <returns>Returns a Texture2D or null.</returns>
        public static Texture2D GetTexture(string textureName, Rectangle sourceRectangle)
        {
            if (!sInitialized)
            {
                throw new InvalidOperationException("TextureManager not initialized.");
            }
            if (!sTextureList.ContainsKey(textureName))
            {
                throw new ArgumentException($"Texture \"{textureName}\" does not exist.");
            }
            var texture = sTextureList[textureName];
            var data = new Color[sourceRectangle.Width * sourceRectangle.Height];
            texture.GetData(0, sourceRectangle, data, 0, sourceRectangle.Width * sourceRectangle.Height);
            var newTexture = new Texture2D(sGraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
            newTexture.SetData(data);

            return newTexture;
        }

        public static List<Texture2D> GetList(string textureName, int tileWidth, int tileHeight)
        {
            if (!sInitialized)
            {
                throw new InvalidOperationException("TextureManager not initialized.");
            }
            if (!sTextureList.ContainsKey(textureName))
            {
                throw new ArgumentException($"Texture \"{textureName}\" does not exist.");
            }
            var texture = sTextureList[textureName];
            return GetList(texture, tileWidth, tileHeight);
        }

        public static List<Texture2D> GetList(Texture2D texture, int tileWidth, int tileHeight)
        {
            if (!sInitialized)
            {
                throw new InvalidOperationException("TextureManager not initialized.");
            }

            if (sUsedTextureLists.ContainsKey((texture, tileWidth, tileHeight)))
                return sUsedTextureLists[(texture, tileWidth, tileHeight)];
            var result = new List<Texture2D>();
            var textureWidth = texture.Width;
            var textureHeight = texture.Height;
            var cols = textureWidth / tileWidth;
            var rows = textureHeight / tileHeight;

            for (var i = 0; i < cols * rows; i++)
            {
                var posX = tileWidth * (i % cols);
                var posY = tileHeight * (i / cols);
                var sourceRectangle = new Rectangle(posX, posY, tileWidth, tileHeight);
                var data = new Color[tileWidth * tileHeight];
                texture.GetData(0, sourceRectangle, data, 0, tileWidth * tileHeight);
                var newTexture = new Texture2D(sGraphicsDevice, tileWidth, tileHeight);
                newTexture.SetData(data);
                result.Add(newTexture);
            }

            sUsedTextureLists[(texture, tileWidth, tileHeight)] = result;

            return result;
        }
    }
}
