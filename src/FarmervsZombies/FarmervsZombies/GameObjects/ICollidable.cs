using Microsoft.Xna.Framework;

namespace FarmervsZombies.GameObjects
{
    internal interface ICollidable
    {
        Vector2 CollisionBoxCenter { get; }
        float Mass { get; }
    }
}