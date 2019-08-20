using System;
using Microsoft.Xna.Framework;

namespace FarmervsZombies
{
    internal static class ExtensionMethods
    {
        public static float Angle(this Vector2 v1, Vector2 v2)
        {
            var dotProduct = Vector2.Dot(Vector2.Normalize(v1), Vector2.Normalize(v2));
            return (float) Math.Acos(dotProduct);
        }
    }
}
