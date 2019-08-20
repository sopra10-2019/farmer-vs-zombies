using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Priority_Queue;

namespace FarmervsZombies.Pathfinding.GameGraph
{
    internal sealed class Vertex : FastPriorityQueueNode
    {
        public static int Count { get; private set; }
        public Vector2 Position { get; }
        public List<Edge> Edges { get; }

        public Vertex(Vector2 position)
        {
            Count++;
            Edges = new List<Edge>();
            Position = position;
        }

        public void AddEdge(Edge edge)
        {
            Edges.Add(edge);
        }

        public void RemoveEdge(Edge edge)
        {
            Edges.Remove(edge);
        }

        ~Vertex()
        {
            Count--;
        }
    }
}
