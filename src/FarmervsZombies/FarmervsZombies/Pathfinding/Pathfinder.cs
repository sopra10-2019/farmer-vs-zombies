using Microsoft.Xna.Framework;

namespace FarmervsZombies.Pathfinding
{
    internal static class Pathfinder
    {
        // Pathfinding algorithm can be exchanged for anything that implements IPathfinder
        private static readonly IPathfinder sPathfinder = new AStar();

        public static UnitPath GetPath(Vector2 start, Vector2 target, bool hasToReachTarget = false, bool strict = true)
        {
            return GetFullPath(start, target, hasToReachTarget, strict).Smooth();
        }

        public static UnitPath GetFullPath(Vector2 start, Vector2 target, bool hasToReachTarget = false, bool strict = true)
        {
            return new UnitPath(sPathfinder.GetPath(start, target, GameGraph.GameGraph.Graph, strict), hasToReachTarget);
        }

        public static bool ExistsPath(Vector2 start, Vector2 target)
        {
            var path = GetFullPath(start, target);
            return start == target || start != path.Target;
        }
    }
}
