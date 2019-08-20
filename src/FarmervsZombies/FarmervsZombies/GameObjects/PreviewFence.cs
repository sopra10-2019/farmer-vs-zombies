using System.Collections.Generic;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal sealed class PreviewFence: BGameObject
    {
        private static readonly List<Texture2D> sFenceTextures = TextureManager.GetList("fence_preview", 32, 32);
        public PreviewFence(int positionX, int positionY) : base(sFenceTextures[15], positionX, positionY) { }

        public override void Update(GameTime gameTime)
        {
            var top = ObjectManager.Instance.CheckTile(new Vector2(Position.X, Position.Y - 1), typeof(PreviewFence));
            var right = ObjectManager.Instance.CheckTile(new Vector2(Position.X + 1, Position.Y), typeof(PreviewFence));
            var bottom = ObjectManager.Instance.CheckTile(new Vector2(Position.X, Position.Y + 1), typeof(PreviewFence));
            var left = ObjectManager.Instance.CheckTile(new Vector2(Position.X - 1, Position.Y), typeof(PreviewFence));
            var val = (top.Count > 0, right.Count > 0, bottom.Count > 0, left.Count > 0);

            var texture = Fence.GetFenceTexture(sFenceTextures, val);
            mTexture = texture ?? mTexture;
        }

        public override void Select() { }
    }
}
