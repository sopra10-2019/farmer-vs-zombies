using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Pathfinding.GameGraph
{
    internal static class GameGraph
    {
        public static Graph Graph { get; } = new Graph(Game1.MapWidth, Game1.MapHeight);

        public static void Build(int width, int height)
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    Graph.AddVertex(i,j);
                }
            }

            foreach (var v in Graph.VerticesTile)
            {
                if (v != null) Graph.ConnectNeighbours(v, 1, true);
            }

            Debug.WriteLine(Graph.VerticesTile);
        }

        public static void AddCollision(Vector2 position)
        {
            Graph.RemoveVertex((int)position.X, (int)position.Y);
        }

        public static void AddCollision(Vector2 position, float collisionValue)
        {
            Graph.AddCollision((int)position.X, (int)position.Y, collisionValue);
        }

        public static void RemoveCollision(Vector2 position)
        {
            var xPos = (int)position.X;
            var yPos = (int)position.Y;
            if (0 > xPos || xPos >= Game1.MapWidth || 0 > yPos || yPos >= Game1.MapHeight) return;
            Graph.RemoveCollision(xPos, yPos);
            Graph.ConnectNeighbours(xPos, yPos);
        }
    }
}
