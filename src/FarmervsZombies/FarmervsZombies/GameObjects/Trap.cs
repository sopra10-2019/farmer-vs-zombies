using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class Trap : BGameObject, IBuildable
    {
        private readonly Animation mCloseAnimation;
        private bool mActivated;
        private const float TotalDamage = 100;
        public Trap(float positionX, float positionY) : base(TextureManager.GetTexture("trap", 64, 64, 0), positionX, positionY)
        {
            Width = 32;
            Height = 32;
            mCloseAnimation = AnimationManager.GetAnimation(AnimationManager.TrapClose, this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (!mActivated)
            {
                var enemiesOnTrap = ObjectManager.Instance.CheckTile(Position, typeof(Zombie));
                if (enemiesOnTrap.Any())
                {
                    mActivated = true;
                }
            }
            else
            {
                mTexture = mCloseAnimation.GetTextureOnce();
                if (mCloseAnimation.AnimationFinished)
                {
                    var enemiesOnTrap = ObjectManager.Instance.CheckTile(Position, typeof(IAttackable)).Where(obj => !((IAttackable)obj).Team).ToList();
                    var damagePerEnemy = TotalDamage / enemiesOnTrap.Count;
                    Debug.WriteLine("Enemies on Trap:" + enemiesOnTrap.Count + ", damagePerEnemy: " + damagePerEnemy);
                    foreach (var enemy in enemiesOnTrap)
                    {
                        enemy?.Damage(damagePerEnemy);
                    }
                    
                }
                if (mCloseAnimation.AnimationFinished)
                {
                    ObjectManager.Instance.Remove(this);
                }
            }
        }

        public override void ToXml(XmlDocument doc, XmlElement xml)
        {
            var element = doc.CreateElement("Trap");
            element.SetAttribute("posX", Position.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("posY", Position.Y.ToString(CultureInfo.InvariantCulture));
            xml.AppendChild(element);
        }
    }
}
