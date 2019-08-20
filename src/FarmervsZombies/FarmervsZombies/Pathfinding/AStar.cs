using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarmervsZombies.Pathfinding.GameGraph;
using Microsoft.Xna.Framework;
using Priority_Queue;

namespace FarmervsZombies.Pathfinding
{
    internal sealed class AStar : IPathfinder
    {
        public IEnumerable<Vector2> GetPath(Vector2 start, Vector2 target, Graph gameGraph, bool strict = true)
        {
            // Add start and target to the game graph
            var startVertex = new Vertex(start);
            var targetVertex = new Vertex(target);
            gameGraph.AddVertex(startVertex);
            gameGraph.AddVertex(targetVertex);
            gameGraph.ConnectNeighbours(startVertex);
            gameGraph.ConnectNeighbours(targetVertex);

            var path = ShortestPath(gameGraph, startVertex, targetVertex, strict);
            gameGraph.RemoveVertex(startVertex);
            gameGraph.RemoveVertex(targetVertex);
            if (!IsTargetValid(path.Last()))
            {
                var pathList = path.ToList();
                if (pathList.Count > 1) pathList[pathList.Count - 1] = pathList[pathList.Count - 2];
                path = pathList;
            }
            return path;
        }

        private static List<Vector2> ShortestPath(Graph graph, Vertex start, Vertex target, bool strict = true)
        {
            var queue = new FastPriorityQueue<Vertex>(graph.VertexCount);
            var predecessor = new Dictionary<Vertex, Vertex>();
            var gValue = new Dictionary<Vertex, float>();
            var processed = new HashSet<Vertex>();

            // Initialize all necessary components
            gValue[start] = 0;
            queue.Enqueue(start, HeuristicCost(start, target));

            // Calculate shortest path
            while (queue.Count != 0)
            {
                var current = queue.Dequeue();
                if (current == target)
                    return ReconstructPath(predecessor, target);

                processed.Add(current);

                foreach (var edge in current.Edges)
                {
                    if (strict && edge.Weight > Vector2.Distance(current.Position, edge.TargetVertex.Position))
                        continue;
                    var neighbour = edge.TargetVertex;
                    if (!gValue.ContainsKey(neighbour)) gValue[neighbour] = float.PositiveInfinity;
                    if (processed.Contains(neighbour))
                        continue;

                    var tentativeValue = gValue[current] + edge.Weight;
                    try
                    {
                        if (tentativeValue >= gValue[neighbour])
                            continue;
                    }
                    catch
                    {
                        Debug.WriteLine("Graph vertex count and number of vertices match: " + graph.CheckGraph());
                    }

                    predecessor[neighbour] = current;
                    gValue[neighbour] = tentativeValue;
                    if (queue.Contains(neighbour))
                    {
                        queue.UpdatePriority(neighbour, tentativeValue + HeuristicCost(neighbour, target));
                    }
                    else
                    {
                        queue.Enqueue(neighbour, tentativeValue + HeuristicCost(neighbour, target));
                    }
                }
            }

            return new List<Vector2> { start.Position, start.Position };
        }

        private static List<Vector2> ReconstructPath(IReadOnlyDictionary<Vertex, Vertex> predecessors, Vertex current)
        {
            var path = new List<Vector2> { current.Position };

            while (predecessors.ContainsKey(current))
            {
                current = predecessors[current];
                if (GameGraph.GameGraph.Graph.CheckCollision((int) current.Position.X, (int) current.Position.Y) &&
                    predecessors.ContainsKey(current)) path.Clear();
                path.Add(current.Position);
            }

            path.Reverse(); 
            return path;
        }

        private static float HeuristicCost(Vertex start, Vertex target)
        {
            return Vector2.Distance(start.Position, target.Position);
        }

        private bool IsTargetValid(Vector2 target)
        {
            for (int i = (int)(target.X - 0.35f); i < Math.Ceiling(target.X + 0.35f); i++)
            {
                for (int j = (int)(target.Y - 0.35f); j < Math.Ceiling(target.Y + 0.35f); j++)
                {
                    if (GameGraph.GameGraph.Graph.CheckCollision(i, j)) return false;
                }
            }

            return true;
        }
    }
}
