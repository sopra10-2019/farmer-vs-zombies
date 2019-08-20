using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal interface IAttackable
    {
        bool Team { get; }
        Vector2 Position { get; }
        Vector2 CollisionBoxCenter { get; }
        Vector2 CollisionBoxSize { get; }
        float Health { get; }
        void Damage(float damage);
    }
}