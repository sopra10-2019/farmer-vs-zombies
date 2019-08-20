using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Pathfinding.GameGraph
{
    internal sealed class Graph
    {
        private readonly Vertex[,] mVerticesTile;
        private readonly List<Vertex> mVerticesOther;
        public IEnumerable<Vertex> VerticesTile
        {
            get { return mVerticesTile.Cast<Vertex>().Where(vertex => vertex != null); }
        }

        private readonly float[,] mVerticesTileCollision;

        private IEnumerable<Vertex> VerticesOther => mVerticesOther;
        public int VertexCount { get; private set; }
        private readonly int mWidth;
        private readonly int mHeight;

        public Graph(int width, int height)
        {
            mVerticesTile = new Vertex[width, height];
            mVerticesTileCollision = new float[width, height];
            mVerticesOther = new List<Vertex>();
            mWidth = width;
            mHeight = height;
        }

        public void AddVertex(Vertex vertex)
        {
            mVerticesOther.Add(vertex);
            VertexCount++;
        }

        public void AddVertex(int xPos, int yPos)
        {
            if (0 > xPos || xPos >= mWidth || 0 > yPos || yPos >= mHeight) return;
            if (mVerticesTile[xPos, yPos] != null) return;
            mVerticesTile[xPos, yPos] = new Vertex(new Vector2(xPos + 0.5f, yPos + 0.5f));
            VertexCount++;
        }

        public void RemoveVertex(Vertex vertex)
        {
            if (mVerticesOther.Remove(vertex)) VertexCount--;

            foreach (var edge in vertex.Edges)
            {
                var edges = new List<Edge>();
                foreach (var edgeBack in edge.TargetVertex.Edges)
                {
                    if (edgeBack.TargetVertex != vertex) continue;
                    edges.Add(edgeBack);
                }

                foreach (var edgeBack in edges)
                {
                    edge.TargetVertex.RemoveEdge(edgeBack);
                }
            }
        }

        public void RemoveVertex(int xPos, int yPos)
        {
            if (0 > xPos || xPos >= mWidth || 0 > yPos || yPos >= mHeight || mVerticesTile[xPos, yPos] == null) return;
            RemoveVertex(mVerticesTile[xPos, yPos]);
            mVerticesTile[xPos, yPos] = null;
            VertexCount--;
        }

        public void ConnectNeighbours(Vertex vertex, float maxDistance = 1f, bool unidirectional = false)
        {
            foreach (var v in VerticesOther)
            {
                if (Vector2.Distance(vertex.Position, v.Position) > maxDistance || vertex == v) continue;
                ConnectNodes(vertex, v, unidirectional);
            }

            var lowerBound = vertex.Position - maxDistance * Vector2.One;
            var upperBound = vertex.Position + maxDistance * Vector2.One;
            for (var i = (int) MathHelper.Clamp(lowerBound.X, 0, mWidth - 1);
                i <= MathHelper.Clamp(upperBound.X, 0, mWidth - 1);
                i++)
            {
                for (var j = (int) MathHelper.Clamp(lowerBound.Y, 0, mHeight - 1);
                    j <= MathHelper.Clamp(upperBound.Y, 0, mHeight - 1);
                    j++)
                {
                    var distance = Vector2.Distance(vertex.Position, new Vector2(i + 0.5f, j + 0.5f));
                    if (distance <= maxDistance && mVerticesTile[i, j] != null && mVerticesTile[i, j] != vertex)
                    {
                        ConnectNodes(vertex, mVerticesTile[i, j], unidirectional);
                    }
                }
            }
        }

        public void ConnectNeighbours(int xPos, int yPos, float maxDistance = 1f, bool unidirectional = false)
        {
            if (0 > xPos || xPos >= mWidth || 0 > yPos || yPos >= mHeight || mVerticesTile[xPos, yPos] == null) return;
            ConnectNeighbours(mVerticesTile[xPos, yPos], maxDistance, unidirectional);
        }

        private void ConnectNodes(Vertex v1, Vertex v2, bool unidirectional)
        {
            var distance = Vector2.Distance(v1.Position, v2.Position);
            var collisionValue1 = mVerticesTile[(int)v1.Position.X, (int)v1.Position.Y] == v1
                ? mVerticesTileCollision[(int)v1.Position.X, (int)v1.Position.Y]
                : 0;
            var collisionValue2 = mVerticesTile[(int)v2.Position.X, (int)v2.Position.Y] == v2
                ? mVerticesTileCollision[(int)v2.Position.X, (int)v2.Position.Y]
                : 0;
            distance += Math.Max(collisionValue1, collisionValue2);
            v1.AddEdge(new Edge(v2, distance));
            if (!unidirectional) v2.AddEdge(new Edge(v1, distance));
        }

        public bool CheckGraph()
        {
            return VertexCount == Vertex.Count;
        }

        public bool CheckCollision(int i, int j)
        {
            if (i < 0 || i >= Game1.MapWidth || j < 0 || j >= Game1.MapHeight) return true;
            return mVerticesTile[i, j] == null || mVerticesTileCollision[i, j] > 0;
        }

        public void AddCollision(int i, int j, float collisionValue)
        {
            mVerticesTileCollision[i, j] = collisionValue;
            if (mVerticesTile[i, j] == null)
            {
                AddVertex(i, j);
                ConnectNeighbours(mVerticesTile[i, j]);
            }
            foreach (var edgeOut in mVerticesTile[i, j].Edges)
            {
                var neighbor = edgeOut.TargetVertex;
                edgeOut.Weight = Vector2.Distance(mVerticesTile[i, j].Position, neighbor.Position) +
                                 Math.Max(collisionValue, mVerticesTileCollision[(int) neighbor.Position.X,
                                     (int) neighbor.Position.Y]);
                var edgeIn = edgeOut.TargetVertex.Edges.Find(e => e.TargetVertex == mVerticesTile[i, j]);
                edgeIn.Weight = edgeOut.Weight;
            }
        }

        public void RemoveCollision(int i, int j)
        {
            AddCollision(i, j, 0);
        }
    }
}
