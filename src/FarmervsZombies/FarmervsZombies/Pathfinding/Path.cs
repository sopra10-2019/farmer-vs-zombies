using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Pathfinding
{
    internal sealed class UnitPath
    {
        private List<Vector2> Path { get; }
        private int mPathIndex;
        private bool mInvalidated;
        private const float MinDistance = 0.05f;
        private readonly bool mHasToReachTarget;

        public UnitPath(IEnumerable<Vector2> path, bool hasToReachTarget = false)
        {
            Path = path.ToList();
            mHasToReachTarget = hasToReachTarget;
        }

        public bool Valid => mPathIndex < Path.Count && !mInvalidated;

        public Vector2 Target => Path.Last();

        public int Length => Path.Count;

        public Vector2 Next(Vector2 position)
        {
            var forwardVector = mPathIndex + 1 < Path.Count ? Path[mPathIndex + 1] - Path[mPathIndex] : Vector2.Zero;
            var backVector = mPathIndex > 0 ? Path[mPathIndex - 1] - Path[mPathIndex] : -forwardVector;
            if (forwardVector == Vector2.Zero) forwardVector = -backVector;
            if (mPathIndex < Path.Count && Vector2.Distance(Path[mPathIndex], position) < MinDistance
                || Path.Count > 1 && PositionPastPoint(Path[mPathIndex],
                    position,
                    forwardVector,
                    backVector))
            {
                mPathIndex++;
            }

            if (mPathIndex == Path.Count)
            {
                if (mHasToReachTarget && Path.Count > 0) mPathIndex--;
                else return position;
            }

            if (mPathIndex > Path.Count)
            {
                throw new InvalidOperationException("Path is not valid anymore.");
            }

            return Path[mPathIndex];
        }

        private static bool PositionPastPoint(Vector2 point, Vector2 position, Vector2 forwardVector, Vector2 backVector)
        {
            var direction = position - point;
            var forwardAngle = direction.Angle(forwardVector);
            var backAngle = direction.Angle(backVector);
            return forwardAngle < backAngle;
        }

        public void Invalidate()
        {
            mInvalidated = true;
        }

        public bool TargetReached(Vector2 position)
        {
            return Vector2.Distance(Path.Last(), position) < MinDistance || mPathIndex == Path.Count;
        }

        public UnitPath Smooth()
        {
            if (mPathIndex != 0) return this;

            while (true)
            {
                var redundant = new List<Vector2>();
                for (var i = 0; i < Path.Count - 2; i++)
                {
                    if (TilesCollide(Path[i], Path[i + 2])) continue;

                    redundant.Add(Path[i + 1]);
                    i++;
                }

                if (!redundant.Any()) break;

                foreach (var node in redundant)
                    Path.Remove(node);
            }

            return this;
        }

        private static bool TilesCollide(Vector2 first, Vector2 second)
        {
            var d = second - first;
            var solutions = SolveQuadratic(d.X * d.Y, d.X + d.Y, 0.75f);

            if (solutions == null)
                throw new InvalidOperationException("Debugger-senpai pls notice me");

            var min = Math.Min(Math.Abs(solutions.Value.Item1), Math.Abs(solutions.Value.Item2));
            var startOffset = new Vector2(d.X < 0 ? -1 : 0, d.Y < 0 ? -1 : 0);
            var targetOffset = -Vector2.One - startOffset;
            var checkPosition = first + startOffset;
            while (Vector2.Distance(checkPosition + targetOffset, first) <= d.Length())
            {
                try
                {
                    if (GameGraph.GameGraph.Graph.CheckCollision((int) Math.Floor(checkPosition.X),
                            (int) Math.Floor(checkPosition.Y)) ||
                        GameGraph.GameGraph.Graph.CheckCollision((int) Math.Ceiling(checkPosition.X),
                            (int) Math.Floor(checkPosition.Y)) ||
                        GameGraph.GameGraph.Graph.CheckCollision((int) Math.Floor(checkPosition.X),
                            (int) Math.Ceiling(checkPosition.Y)) ||
                        GameGraph.GameGraph.Graph.CheckCollision((int) Math.Ceiling(checkPosition.X),
                            (int) Math.Ceiling(checkPosition.Y))) return true;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }

                checkPosition += d * min;
            }

            return false;
        }

        public bool Empty()
        {
            return Path.First() == Target;
        }

        private static (float, float)? SolveQuadratic(float a, float b, float c)
        {
            if (b * b - 4 * a * c < 0)
                return null;

            return ((float)((-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a)), 
                    (float)((-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a)));
        }
    }
}
