using System;
using System.Collections.Generic;
using System.Linq;
using FarmervsZombies.GameObjects;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Managers
{
    internal sealed class SteeringManager
    {
        private readonly BMovableGameObject mSteeringGameObject;
        private const float AvoidForceScaling = 1.5f;
        private const float SeekForceScaling = 1;
        private const float AlignmentForceScaling = 1;
        private const float CohesionForceScaling = 1;
        private const float SeparationForceScaling = 1.2f;
        private const float CollisionForceScaling = 3f;
        private Vector2 mSteeringVector = Vector2.Zero;
        private static readonly Random sRandom = new Random();

        public SteeringManager(BMovableGameObject steeringGameObject)
        {
            mSteeringGameObject = steeringGameObject;
        }


        public Vector2 GetSteering()
        {
            return mSteeringVector;
        }

        public void UpdateSteering(Vector2 target, List<ICollidable> neighbors)
        {
            mSteeringVector = Vector2.Zero;
            Seek(target);
            if (mSteeringGameObject is ICollidable)
            {
                Collide();
                if (target != mSteeringGameObject.CollisionBoxCenter)
                {
                    CollisionAvoidance(neighbors);
                    Flocking(neighbors);
                }
            }
        }

        private void Seek(Vector2 target)
        {
            var velocity = mSteeringGameObject.Velocity;
            var desiredVelocity = target - mSteeringGameObject.CollisionBoxCenter;
            if (desiredVelocity != Vector2.Zero) desiredVelocity.Normalize();
            desiredVelocity *= mSteeringGameObject.Speed;
            var seekForce = desiredVelocity - velocity;
            seekForce *= SeekForceScaling;
            mSteeringVector += seekForce;
        }

        private void CollisionAvoidance(IEnumerable<ICollidable> neighbors)
        {
            var velocity = mSteeringGameObject.Velocity;
            var threateningNeighbors = new List<ICollidable>();
            var angle = (float)Math.Atan2(velocity.Y, velocity.X);
            foreach (var neighbor in neighbors)
            {
                var threatDirection = Vector2.Transform(neighbor.CollisionBoxCenter - mSteeringGameObject.CollisionBoxCenter, Matrix.CreateRotationZ(-angle));
                if (Math.Abs(threatDirection.Y) < mSteeringGameObject.CollisionBoxSize.Length() && threatDirection.X >= 0)
                {
                    threateningNeighbors.Add(neighbor);
                }
            }

            var threateningObject =
                GetClosestThreateningObject(threateningNeighbors);

            var avoidanceVector = Vector2.Zero;

            if (threateningObject != null)
            {
                var threateningObjectCenter = threateningObject.CollisionBoxCenter;
                var gameObjectCenter = mSteeringGameObject.CollisionBoxCenter;
                var threatDirection = Vector2.Transform(threateningObjectCenter - gameObjectCenter, Matrix.CreateRotationZ(-angle));
                avoidanceVector = new Vector2(-velocity.Y, velocity.X) * -threatDirection.Y;
                if (avoidanceVector == Vector2.Zero) avoidanceVector = new Vector2(-velocity.Y, velocity.X);
                if (avoidanceVector != Vector2.Zero) avoidanceVector.Normalize();
                avoidanceVector *= AvoidForceScaling / MathHelper.Clamp(Vector2.Distance(threateningObjectCenter, gameObjectCenter), 0.1f, 3f);
            }

            mSteeringVector += avoidanceVector;
        }

        private void Flocking(IEnumerable<ICollidable> neighbors)
        {
            var velocity = mSteeringGameObject.Velocity;
            var flockingPartners = new List<BMovableGameObject>();
            foreach (var neighbor in neighbors)
            {
                if (neighbor is BMovableGameObject movable && Vector2.Dot(movable.Velocity, velocity) > 0) flockingPartners.Add(movable);
            }
            Alignment(flockingPartners);
            Cohesion(flockingPartners);
            Separation(flockingPartners);
        }

        private void Collide()
        {
            var collisionVector = Vector2.Zero;
            foreach (var waterTile in mSteeringGameObject.GetCollidingWaterTiles())
            {
                var waterTileCenter = waterTile + Vector2.One / 2;
                var distance = mSteeringGameObject.CollisionBoxCenter - waterTileCenter;
                if (distance != Vector2.Zero) distance /= distance.LengthSquared() * MathHelper.Clamp(distance.Length(), 0, 1);
                else
                {
                    var angle = sRandom.NextDouble() * Math.PI * 2;
                    distance += 3 * new Vector2((float) Math.Cos(angle), (float) Math.Sin(angle));
                }
                collisionVector += distance * 1000 / mSteeringGameObject.Mass;
            }

            foreach (var colliding in mSteeringGameObject.GetCollidingObjects())
            {
                var distance = mSteeringGameObject.CollisionBoxCenter - colliding.CollisionBoxCenter;
                if (distance != Vector2.Zero) distance /= distance.LengthSquared() * MathHelper.Clamp(distance.Length(), 0, 1);
                else
                {
                    var angle = sRandom.NextDouble() * Math.PI * 2;
                    distance += 3 * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                }

                distance *= colliding.Mass / mSteeringGameObject.Mass;
                collisionVector += distance;
            }
            mSteeringVector += collisionVector * CollisionForceScaling;
        }

        private void Alignment(List<BMovableGameObject> neighbors)
        {
            var velocity = mSteeringGameObject.Velocity;
            var neighborsDirection = Vector2.Zero;
            foreach (var neighbor in neighbors)
            {
                neighborsDirection += neighbor.Velocity;
            }

            if (neighborsDirection == Vector2.Zero) return;
            neighborsDirection.Normalize();
            neighborsDirection *= velocity.Length();
            var alignmentVector = neighborsDirection - velocity;
            mSteeringVector += alignmentVector * AlignmentForceScaling;
        }

        private void Cohesion(IReadOnlyCollection<BMovableGameObject> neighbors)
        {
            var center = mSteeringGameObject.CollisionBoxCenter;
            var neighborCount = 0;
            foreach (var neighbor in neighbors)
            {
                center += neighbor.CollisionBoxCenter;
                neighborCount++;
            }

            center /= neighborCount + 1;
            var cohesionVector = center - mSteeringGameObject.CollisionBoxCenter;
            if (cohesionVector != Vector2.Zero) cohesionVector.Normalize();
            mSteeringVector += cohesionVector * CohesionForceScaling;
        }

        private void Separation(List<BMovableGameObject> neighbors)
        {
            var separationVector = Vector2.Zero;
            foreach (var neighbor in neighbors)
            {
                var distance = Vector2.Zero;
                if (neighbor is BMovableGameObject movable) distance = mSteeringGameObject.CollisionBoxCenter - movable.CollisionBoxCenter;
                if (distance != Vector2.Zero) distance /= distance.LengthSquared();
                separationVector += distance;
            }

            if (separationVector == Vector2.Zero) return;
            separationVector.Normalize();
            mSteeringVector += separationVector * SeparationForceScaling;
        }

        private ICollidable GetClosestThreateningObject(IReadOnlyCollection<ICollidable> threateningCollidables)
        {
            if (threateningCollidables.Count == 0) return null;

            const float futureTime = 0.125f;
            var steeringObjectFuturePosition = mSteeringGameObject.CollisionBoxCenter + futureTime * mSteeringGameObject.Velocity;
            var closestObject = threateningCollidables.First();
            var closestObjectFuturePosition = closestObject.CollisionBoxCenter;
            if (closestObject is BMovableGameObject closestMovable) closestObjectFuturePosition += futureTime * closestMovable.Velocity;
            foreach (var obj in threateningCollidables)
            {
                var objFuturePosition = obj.CollisionBoxCenter;
                if (obj is BMovableGameObject movable) objFuturePosition += futureTime * movable.Velocity;
                if (Vector2.Distance(objFuturePosition, steeringObjectFuturePosition) <
                    Vector2.Distance(closestObjectFuturePosition, steeringObjectFuturePosition))
                {
                    closestObject = obj;
                    closestObjectFuturePosition = objFuturePosition;
                }
            }

            return Vector2.Distance(closestObjectFuturePosition, steeringObjectFuturePosition) <
                   mSteeringGameObject.Velocity.Length()
                ? closestObject
                : null;
        }
    }
}
