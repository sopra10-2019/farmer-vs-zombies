using System.Xml;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal sealed class TreasureZombie : Zombie
    {
        private readonly GraveyardTreasure mTreasure;


        public TreasureZombie(float positionX, float positionY, GraveyardTreasure treasure)
            : base(positionX, positionY, AnimationManager.ZombieWalkLeft, AnimationManager.ZombieWalkRight, null, null, true, true, 2)
        {
            mTreasure = treasure;
            Aggressive = true;
            Mass = 1.2f;
        }

        protected override void Death()
        {
            mTreasure.RemoveZombie(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Vector2.Distance(Position, mTreasure.Position + new Vector2(0.5f, 0.5f)) > 5f && !HasTarget)
            {
                RequestPath(mTreasure.Position + new Vector2(0.5f, 0.5f));
            }
        }

        // NOTE: Do not save this class, it is implicitly saved
        public override void ToXml(XmlDocument doc, XmlElement xml)
        {

        }
    }
}