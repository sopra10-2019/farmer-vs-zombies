using System.Collections.Generic;
using FarmervsZombies.Pathfinding.GameGraph;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Pathfinding
{
    internal interface IPathfinder
    {
        IEnumerable<Vector2> GetPath(Vector2 start, Vector2 target, Graph gameGraph, bool strict);
    }
}
