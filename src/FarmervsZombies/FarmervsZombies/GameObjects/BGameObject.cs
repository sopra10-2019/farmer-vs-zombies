using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.GameObjects
{
    internal abstract class BGameObject : ISelectable
    {
        protected Texture2D mTexture;
        protected Color mBackgroundColour = Color.White;
        public Vector2 TextureOffset { get; protected set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public Vector2 Position { get; protected set; }
        protected Vector2 CenterPosition => new Vector2(Position.X + Width / 64.0f + TextureOffset.X, Position.Y + Height / 64.0f + TextureOffset.Y);
        public bool Selected { get; protected set; }
        public float Health { get; set; } = 100;
        public float MaxHealth { get; protected set; } = 100;

        protected float Defense { get; set; } = 75;
        public Vector2 TextureBoxSize => new Vector2((float)Width / 32, (float)Height / 32);
        public Vector2 CollisionBoxSize { get; protected set; }
        public Vector2 CollisionBoxOffset { get; protected set; }
        public Vector2 CollisionBoxCenter => Position + CollisionBoxOffset;
        public QuadTree CurrentQuadTree { get; set; }
        public List<QuadTree> CurrentSubQuadTrees { get; } = new List<QuadTree>();

        protected readonly LiveBar mLiveBar;
        protected bool mDrawLiveBarMySelf = false;
        private float LayerDepth => MathHelper.Clamp((1 - (Position.Y + Position.X / Game1.MapWidth) / (Game1.MapHeight + 1)) * 0.5f, 0.0f, 0.5f);
        protected float RegenerationRate { private get; set; } = 0.5f;
        public float Mass { get; protected set; } = 1000;

        protected BGameObject(Texture2D texture, float positionX, float positionY)
        {
            mTexture = texture;
            Width = mTexture.Width;
            Height = mTexture.Height;
            Position = new Vector2(positionX, positionY);
            TextureOffset = Vector2.Zero;
            CollisionBoxSize = new Vector2(1,1);
            CollisionBoxOffset = new Vector2(0.5f, 0.5f);
            mLiveBar = new LiveBar();
            mLiveBar.LoadContent();
        }

        protected BGameObject(Texture2D texture, float positionX, float positionY, float textureOffsetX, float textureOffsetY) : this(texture, positionX, positionY)
        {
            TextureOffset = new Vector2(textureOffsetX, textureOffsetY);
        }

        protected BGameObject(Texture2D texture,
            float positionX,
            float positionY,
            float textureOffsetX,
            float textureOffsetY,
            float collisionBoxSizeX,
            float collisionBoxSizeY) : this(texture, positionX, positionY, textureOffsetX, textureOffsetY)
        {
            CollisionBoxSize = new Vector2(collisionBoxSizeX, collisionBoxSizeY);
        }

        protected BGameObject(Texture2D texture,
            float positionX,
            float positionY,
            float textureOffsetX,
            float textureOffsetY,
            float collisionBoxSizeX,
            float collisionBoxSizeY,
            float collisionBoxOffsetX,
            float collisionBoxOffsetY) : this(texture, positionX, positionY, textureOffsetX, textureOffsetY, collisionBoxSizeX, collisionBoxSizeY)
        {
            CollisionBoxOffset = new Vector2(collisionBoxOffsetX, collisionBoxOffsetY);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            if (mTexture == null)
            {
                throw new InvalidOperationException($"{GetType().Name} texture is null.");
            }

            spriteBatch.Draw(mTexture,
                new Rectangle((int)((Position.X + TextureOffset.X) * 32),
                    (int)((Position.Y + TextureOffset.Y) * 32),
                    Width,
                    Height),
                null,
                mBackgroundColour, 0.0f, new Vector2(0,0), SpriteEffects.None, LayerDepth);

            if (!Selected) return;
            var texture = TextureManager.GetTexture("circle");
            spriteBatch.Draw(texture,
                new Rectangle((int)((CollisionBoxCenter.X - CollisionBoxSize.X / 2) * 32),
                    (int)((CollisionBoxCenter.Y - CollisionBoxSize.Y / 2) * 32),
                    (int) (CollisionBoxSize.X * 32),
                    (int) (CollisionBoxSize.Y * 32)),
                null,
                mBackgroundColour,
                0.0f,
                new Vector2(0, 0),
                SpriteEffects.None,
                0.5f);

            if (mDrawLiveBarMySelf)
            {
                mLiveBar.Draw(spriteBatch);
            }
        }

        public void Deselect()
        {
            Selected = false;
        }

        public virtual void Select()
        {
            Selected = true;
            ObjectManager.Instance.AddSelected(this);
        }

        public virtual void Update(GameTime gameTime)
        {
            Regenerate(gameTime);
            mLiveBar.Update(Health, MaxHealth, Position);
        }

        private void Regenerate(GameTime gameTime)
        {
            Health += RegenerationRate * (float) gameTime.ElapsedGameTime.TotalSeconds;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        public virtual void Damage(float damage)
        {
            Health -= damage * DamageReduction();
            if (Health <= 0) Death();
        }

        protected virtual void Death()
        {
            // Death animations and class specific treatment should be implemented via method override in the specific classes
            // These methods should then lastly call base.Death()
            ObjectManager.Instance.QueueRemoval(this);
        }

        private float DamageReduction()
        {
            return 1 - Defense / (Defense + 100);
        } 

        public virtual void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("object");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}