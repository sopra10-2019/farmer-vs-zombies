using System;
using System.Collections.Generic;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies
{
    /// <summary>
    /// Implementation of a quad tree.
    /// Objects are saved as a combination of a collision box and a texture box.
    /// </summary>
    internal sealed class QuadTree
    {
        private const int NodeCapacity = 4;
        private const int MaxDepth = 10;
        private readonly int mCurrentDepth;
        private readonly Vector2 mCenterCoordinates;
        private readonly float mDimensionLength;
        private readonly Vector2 mLowerBound;
        private readonly Vector2 mUpperBound;

        private readonly List<BGameObject> mGameObjects;

        private int mGameObjectCount;
        // 0 = north west, 1 = north east, 2 = south west, 3 = south east
        private readonly QuadTree[] mSubQuadTrees = new QuadTree[4];
        private readonly QuadTree mRootQuadTree;
        private readonly QuadTree mParentQuadTree;
        private static readonly Vector2 sGlobalCollisionBoxBoundarySize = new Vector2(Game1.MapWidth, Game1.MapHeight);
        private static readonly Vector2 sGlobalCollisionBoxBoundaryCenter = new Vector2(Game1.MapWidth, Game1.MapHeight) / 2;

        // collision box only for now
        private readonly List<BGameObject> mGameObjectsFromUpperQuad;

        public QuadTree(Vector2 centerCoordinates,
            float dimensionLength,
            QuadTree rootQuadTree = null,
            QuadTree parentQuadTree = null)
        {
            mCenterCoordinates = centerCoordinates;
            mDimensionLength = dimensionLength;
            mLowerBound = mCenterCoordinates - new Vector2(mDimensionLength, mDimensionLength);
            mUpperBound = mCenterCoordinates + new Vector2(mDimensionLength, mDimensionLength);
            mGameObjects = new List<BGameObject>();
            mRootQuadTree = rootQuadTree ?? this;
            mParentQuadTree = parentQuadTree;
            mCurrentDepth = parentQuadTree?.mCurrentDepth + 1 ?? 0;
            mGameObjectsFromUpperQuad = new List<BGameObject>();
        }

        /// <summary>
        /// Inserts a game object in the quad tree.
        /// </summary>
        /// <param name="gameObject">The object that is being inserted</param>
        /// <param name="insertInThisQuad">If true, then force add object in the current quad. (Should never be used outside of the quad tree)</param>
        /// <returns>true, if insert is successful, otherwise false</returns>
        public bool InsertGameObject(BGameObject gameObject, bool insertInThisQuad = false)
        {
            if (insertInThisQuad)
            {
                mGameObjects.Add(gameObject);
                gameObject.CurrentQuadTree = this;
                DecrementCountOfNodeAndParents(-1);
                InsertInSubQuadTrees(gameObject);
                return true;
            }

            if (IsGameObjectOutOfBounds(gameObject))
            {
                return false;
            }

            if (mGameObjects.Count < NodeCapacity && mSubQuadTrees[0] == null)
            {
                return InsertGameObject(gameObject, true);
            }

            if (mSubQuadTrees[0] == null)
            {
                SubdivideQuadTree();
            }

            if (!mSubQuadTrees[GetQuadrantIndex(gameObject.Position)].InsertGameObject(gameObject))
            {
                InsertGameObject(gameObject, true);
            }

            return true;
        }

        /// <summary>
        /// Remove a game object from the quad tree
        /// </summary>
        /// <param name="gameObject">The object that is being removed</param>
        /// <param name="removeInThisQuad">If true, then force remove object in the current quad. (Should never be used outside of the quad tree)</param>
        /// <returns>true, if remove is successful, otherwise false</returns>
        public bool RemoveGameObject(BGameObject gameObject, bool removeInThisQuad = false)
        {
            if (removeInThisQuad)
            {
                if (!mGameObjects.Remove(gameObject)) return false;
                DecrementCountOfNodeAndParents();
                var currentQuadTree = this;
                while (currentQuadTree != null && currentQuadTree.mGameObjectCount <= NodeCapacity)
                {
                    currentQuadTree.MergeQuadTrees();
                    currentQuadTree = currentQuadTree.mParentQuadTree;
                }
                gameObject.CurrentQuadTree = null;
                foreach (var subtree in gameObject.CurrentSubQuadTrees)
                {
                    subtree.mGameObjectsFromUpperQuad.Remove(gameObject);
                }
                gameObject.CurrentSubQuadTrees.Clear();
                return true;

            }

            return gameObject.CurrentQuadTree != null &&
                   gameObject.CurrentQuadTree.RemoveGameObject(gameObject, true);
        }

        /// <summary>
        /// Updates the placement of the game object in the quad tree data structure. Use this whenever the position of the object has changed.
        /// </summary>
        /// <param name="gameObject">That object that is being moved in the quad tree</param>
        /// <returns>true, if moving is successful, otherwise false</returns>
        public bool UpdateGameObjectPosition(BGameObject gameObject)
        {
            if (gameObject.CurrentQuadTree != this)
            {
                return gameObject.CurrentQuadTree.UpdateGameObjectPosition(gameObject);
            }
            if (IsGameObjectOutOfBounds(gameObject))
            {
                if (!mRootQuadTree.InsertGameObject(gameObject)) return false;
                var currentQuadTree = gameObject.CurrentQuadTree;
                RemoveGameObject(gameObject, true);
                gameObject.CurrentQuadTree = currentQuadTree;
                InsertInSubQuadTrees(gameObject);
            }
            else if (mSubQuadTrees[0] != null && mSubQuadTrees[GetQuadrantIndex(gameObject.Position)].InsertGameObject(gameObject))
            {
                var currentQuadTree = gameObject.CurrentQuadTree;
                RemoveGameObject(gameObject, true);
                gameObject.CurrentQuadTree = currentQuadTree;
                InsertInSubQuadTrees(gameObject);
            }
            else
            {
                foreach (var subtree in gameObject.CurrentSubQuadTrees)
                {
                    subtree.mGameObjectsFromUpperQuad.Remove(gameObject);
                }
                gameObject.CurrentSubQuadTrees.Clear();
                InsertInSubQuadTrees(gameObject);
            }

            return true;
        }

        /// <summary>
        /// Returns a list of the objects that touch this location.
        /// </summary>
        /// <param name="location">The position where the objects are searched</param>
        /// <param name="useTextureBoxes">determines whether the collision boxes or the texture boxes are being used for the search</param>
        /// <param name="type">The type that the objects are being filtered by.</param>
        /// <param name="onlyThisQuad">If true, it only returns the objects in this quad. (Should never be used outside of the quad tree)</param>
        /// <param name="fromUpperQuad">If true, the objects from the upper quads are returned. (Should never be used outside of the quad tree)</param>
        /// <returns>The list of the objects associated with the location</returns>
        public List<BGameObject> GetGameObjects(Vector2 location, bool useTextureBoxes, Type type = null, bool onlyThisQuad = false, bool fromUpperQuad = false)
        {
            var gameObjectsInLocation = new List<BGameObject>();

            // Check if location out of bounds
            if (IsLocationOutOfBounds(location))
            {
                return gameObjectsInLocation;
            }

            var gameObjects = fromUpperQuad ? mGameObjectsFromUpperQuad : mGameObjects;
            // Add object of this quad tree
            foreach (var gameObject in gameObjects)
            {
                if (LocationContainsGameObject(location, gameObject, useTextureBoxes) && (type == null || type.IsInstanceOfType(gameObject))) gameObjectsInLocation.Add(gameObject);
            }

            if (mSubQuadTrees[0] == null || onlyThisQuad) return gameObjectsInLocation;

            // Add objects from sub quad trees
            for (var index = 0; index < 4; index++)
            {
                gameObjectsInLocation.AddRange(mSubQuadTrees[index].GetGameObjects(location, useTextureBoxes, type));
            }

            return gameObjectsInLocation;
        }

        /// <summary>
        /// Returns a list of the objects that are in this rectangle.
        /// </summary>
        /// <param name="lowerBound">upper left corner of the rectangle</param>
        /// <param name="upperBound">bottom right corner of the rectangle</param>
        /// <param name="useTextureBoxes">determines whether the collision boxes or the texture boxes are being used for the search</param>
        /// <param name="type">The type that the objects are being filtered by.</param>
        /// <param name="onlyThisQuad">If true, it only returns the objects in this quad. (Should never be used outside of the quad tree)</param>
        /// <param name="fromUpperQuad">If true, the objects from the upper quads are returned. (Should never be used outside of the quad tree)</param>
        /// <returns>The list of the objects associated with the rectangle</returns>
        public List<BGameObject> GetGameObjects(Vector2 lowerBound, Vector2 upperBound, bool useTextureBoxes, Type type = null, bool onlyThisQuad = false, bool fromUpperQuad = false)
        {
            var gameObjectsInLocation = new List<BGameObject>();

            // Check if region out of bounds
            if (!AreRegionsIntersecting(lowerBound, upperBound, mLowerBound, mUpperBound))
            {
                return gameObjectsInLocation;
            }

            var gameObjects = fromUpperQuad ? mGameObjectsFromUpperQuad : mGameObjects;
            // Add object of this quad tree
            foreach (var gameObject in gameObjects)
            {
                var center = useTextureBoxes ? GetTextureBoxPosition(gameObject) : gameObject.CollisionBoxCenter;
                var size = useTextureBoxes ? gameObject.TextureBoxSize : gameObject.CollisionBoxSize;
                if (AreRegionsIntersecting(lowerBound,
                    upperBound,
                    center - size / 2,
                    center + size / 2) && (type == null || type.IsInstanceOfType(gameObject))) gameObjectsInLocation.Add(gameObject);
            }

            if (mSubQuadTrees[0] == null || onlyThisQuad) return gameObjectsInLocation;

            // Add objects from sub quad trees
            for (var index = 0; index < 4; index++)
            {
                gameObjectsInLocation.AddRange(mSubQuadTrees[index].GetGameObjects(lowerBound, upperBound, useTextureBoxes, type));
            }

            return gameObjectsInLocation;
        }

        /// <summary>
        /// Returns a list of the objects that are in this circle.
        /// </summary>
        /// <param name="circleCenter">center of the circle</param>
        /// <param name="circleRadius">radius of the circle</param>
        /// <param name="useTextureBoxes">determines whether the collision boxes or the texture boxes are being used for the search</param>
        /// <param name="type">The type that the objects are being filtered by.</param>
        /// <param name="onlyThisQuad">If true, it only returns the objects in this quad. (Should never be used outside of the quad tree)</param>
        /// <param name="fromUpperQuad">If true, the objects from the upper quads are returned. (Should never be used outside of the quad tree)</param>
        /// <returns>The list of the objects associated with the circle</returns>
        public List<BGameObject> GetGameObjects(Vector2 circleCenter, float circleRadius, bool useTextureBoxes, Type type = null, bool onlyThisQuad = false, bool fromUpperQuad = false)
        {
            var gameObjectsInLocation = new List<BGameObject>();

            // Check if region out of bounds
            if (!AreRectangleAndCircleIntersecting(mLowerBound, mUpperBound, circleCenter, circleRadius))
            {
                return gameObjectsInLocation;
            }

            var gameObjects = fromUpperQuad ? mGameObjectsFromUpperQuad : mGameObjects;
            // Add object of this quad tree
            foreach (var gameObject in gameObjects)
            {
                var center = useTextureBoxes ? GetTextureBoxPosition(gameObject) : gameObject.CollisionBoxCenter;
                var size = useTextureBoxes ? gameObject.TextureBoxSize : gameObject.CollisionBoxSize;
                if (AreRectangleAndCircleIntersecting(
                        center - size / 2,
                        center + size / 2,
                        circleCenter,
                        circleRadius) &&
                    (type == null || type.IsInstanceOfType(gameObject))) gameObjectsInLocation.Add(gameObject);
            }

            if (mSubQuadTrees[0] == null || onlyThisQuad) return gameObjectsInLocation;

            // Add objects from sub quad trees
            for (var index = 0; index < 4; index++)
            {
                gameObjectsInLocation.AddRange(mSubQuadTrees[index].GetGameObjects(circleCenter, circleRadius, useTextureBoxes, type));
            }

            return gameObjectsInLocation;
        }

        /// <summary>
        /// Returns a complete list of objects in the quad tree
        /// </summary>
        /// <param name="type">The type that the objects are being filtered by.</param>
        /// <returns>The list of the objects in the whole quad tree</returns>
        public List<BGameObject> GetGameObjects(Type type = null)
        {
            var gameObjects = new List<BGameObject>();
            if (type == null)
            {
                gameObjects.AddRange(mGameObjects);
            }
            else
            {
                foreach (var gameObject in gameObjects)
                {
                    if (type.IsInstanceOfType(gameObject))
                    {
                        gameObjects.Add(gameObject);
                    }
                }
            }

            if (mSubQuadTrees[0] != null)
            {
                for (var index = 0; index < 4; index++)
                {
                    gameObjects.AddRange(mSubQuadTrees[index].GetGameObjects(type));
                }
            }

            return gameObjects;
        }

        /// <summary>
        /// Returns a list of all objects that collide with gameObject if it were at a certain position.
        /// </summary>
        /// <param name="gameObject">The object whose collisions are being looked at</param>
        /// <param name="position">The position where the collisions are looked at</param>
        /// <returns>The list of objects that collide with gameObject</returns>
        public List<ICollidable> GetCollidingObjects(BGameObject gameObject, Vector2 position)
        {
            var currentQuadTree = this;
            while (currentQuadTree.IsBoxOutOfBounds(position + gameObject.CollisionBoxOffset, gameObject.CollisionBoxSize) && currentQuadTree.mParentQuadTree != null)
            {
                currentQuadTree = currentQuadTree.mParentQuadTree;
            }

            var collidingObjects =
                currentQuadTree.GetGameObjects(position + gameObject.CollisionBoxOffset - gameObject.CollisionBoxSize / 2,
                    position + gameObject.CollisionBoxOffset + gameObject.CollisionBoxSize / 2,
                    false,
                    typeof(ICollidable));
            collidingObjects.AddRange(currentQuadTree.GetGameObjects(position + gameObject.CollisionBoxOffset - gameObject.CollisionBoxSize / 2,
                position + gameObject.CollisionBoxOffset + gameObject.CollisionBoxSize / 2,
                false,
                typeof(ICollidable), true, true));
            collidingObjects.Remove(gameObject);
            return ConvertListToType<BGameObject, ICollidable>(collidingObjects);
        }

        public List<ICollidable> GetCollidingObjects(BGameObject gameObject, float range)
        {
            var currentQuadTree = this;
            while (currentQuadTree.IsBoxOutOfBounds(gameObject.CollisionBoxCenter, 2 * new Vector2(range, range)) && currentQuadTree.mParentQuadTree != null)
            {
                currentQuadTree = currentQuadTree.mParentQuadTree;
            }

            var collidingObjects =
                currentQuadTree.GetGameObjects(gameObject.CollisionBoxCenter, range,
                    false,
                    typeof(ICollidable));
            collidingObjects.AddRange(currentQuadTree.GetGameObjects(gameObject.CollisionBoxCenter, range,
                false,
                typeof(ICollidable), true, true));
            collidingObjects.Remove(gameObject);
            return ConvertListToType<BGameObject, ICollidable>(collidingObjects);
        }

        public static bool AreGameObjectsColliding(BGameObject gameObject1, BGameObject gameObject2)
        {
            return AreRegionsIntersecting(gameObject1.CollisionBoxCenter - gameObject1.CollisionBoxSize / 2,
                gameObject1.CollisionBoxCenter + gameObject1.CollisionBoxSize / 2,
                gameObject2.CollisionBoxCenter - gameObject2.CollisionBoxSize / 2,
                gameObject2.CollisionBoxCenter + gameObject2.CollisionBoxSize / 2);
        }

        public static List<Vector2> GetCollidingWaterTiles(BGameObject gameObject)
        {
            return GetCollidingWaterTiles(gameObject, gameObject.Position);
        }

        private static List<Vector2> GetCollidingWaterTiles(BGameObject gameObject, Vector2 position)
        {
            var waterTiles = new List<Vector2>();
            var lowerBound = position + gameObject.CollisionBoxOffset - gameObject.CollisionBoxSize / 2;
            var upperBound = position + gameObject.CollisionBoxOffset + gameObject.CollisionBoxSize / 2;
            for (var i = (int)lowerBound.X; i < upperBound.X; i++)
            {
                for (var j = (int)lowerBound.Y; j < upperBound.Y; j++)
                {
                    if (Game1.sTileMap.GetTileType(i, j) == Tile.Water)
                    {
                        waterTiles.Add(new Vector2(i, j));
                    }
                }
            }

            return waterTiles;
        }

        public List<IAttackable> GetAttackablesInRange(Vector2 position, float range)
        {
            return ConvertListToType<BGameObject, IAttackable>(GetGameObjects(position,
                range,
                false,
                typeof(IAttackable)));
        }

        public static Vector2 ClampPosition(BGameObject gameObject)
        {
            if (!IsBoxOutOfBounds(gameObject.CollisionBoxCenter,
                gameObject.CollisionBoxSize,
                sGlobalCollisionBoxBoundaryCenter,
                sGlobalCollisionBoxBoundarySize)) return Vector2.Zero;
            var correctionVector = Vector2.Zero;
            correctionVector +=
                MathHelper.Clamp(
                    sGlobalCollisionBoxBoundaryCenter.X - sGlobalCollisionBoxBoundarySize.X / 2 -
                    (gameObject.CollisionBoxCenter.X - gameObject.CollisionBoxSize.X / 2),
                    0,
                    float.PositiveInfinity) * Vector2.UnitX;
            correctionVector +=
                MathHelper.Clamp(
                    sGlobalCollisionBoxBoundaryCenter.Y - sGlobalCollisionBoxBoundarySize.Y / 2 -
                    (gameObject.CollisionBoxCenter.Y - gameObject.CollisionBoxSize.Y / 2),
                    0,
                    float.PositiveInfinity) * Vector2.UnitY;
            correctionVector -=
                MathHelper.Clamp(
                    gameObject.CollisionBoxCenter.X + gameObject.CollisionBoxSize.X / 2 -
                    (sGlobalCollisionBoxBoundaryCenter.X + sGlobalCollisionBoxBoundarySize.X / 2),
                    0,
                    float.PositiveInfinity) * Vector2.UnitX;
            correctionVector -=
                MathHelper.Clamp(
                    gameObject.CollisionBoxCenter.Y + gameObject.CollisionBoxSize.Y / 2 -
                    (sGlobalCollisionBoxBoundaryCenter.Y + sGlobalCollisionBoxBoundarySize.Y / 2),
                    0,
                    float.PositiveInfinity) * Vector2.UnitY;
            return correctionVector;
        }

        private void SubdivideQuadTree()
        {
            if (mSubQuadTrees[0] != null && mCurrentDepth >= MaxDepth) return;
            mSubQuadTrees[0] = new QuadTree(mCenterCoordinates + new Vector2(-mDimensionLength / 2, -mDimensionLength / 2),
                    mDimensionLength / 2, mRootQuadTree, this);
            mSubQuadTrees[1] = new QuadTree(mCenterCoordinates + new Vector2(mDimensionLength / 2, -mDimensionLength / 2),
                mDimensionLength / 2, mRootQuadTree, this);
            mSubQuadTrees[2] = new QuadTree(mCenterCoordinates + new Vector2(-mDimensionLength / 2, mDimensionLength / 2),
                mDimensionLength / 2, mRootQuadTree, this);
            mSubQuadTrees[3] = new QuadTree(mCenterCoordinates + new Vector2(mDimensionLength / 2, mDimensionLength / 2),
                mDimensionLength / 2, mRootQuadTree, this);

            var movedGameObjects = new List<BGameObject>();
            foreach (var gameObject in mGameObjects)
            {
                if (mSubQuadTrees[GetQuadrantIndex(gameObject.Position)].InsertGameObject(gameObject))
                {
                    movedGameObjects.Add(gameObject);
                }
                else
                {
                    InsertInSubQuadTrees(gameObject);
                }
            }

            foreach (var gameObject in mGameObjectsFromUpperQuad)
            {
                for (int i = 0; i < 4; i++)
                {
                    mSubQuadTrees[i].InsertInSubQuadTrees(gameObject);
                }
            }

            foreach (var gameObject in movedGameObjects)
            {
                mGameObjects.Remove(gameObject);
                DecrementCountOfNodeAndParents();
            }
        }

        private void MergeQuadTrees()
        {
            if (mSubQuadTrees[0] == null) return;
            for (var index = 0; index < 4; index++)
            {
                mSubQuadTrees[index].MergeQuadTrees();
                foreach (var gameObject in mSubQuadTrees[index].mGameObjects)
                {
                    gameObject.CurrentQuadTree = this;
                }
                mGameObjects.AddRange(mSubQuadTrees[index].mGameObjects);
                mSubQuadTrees[index] = null;
            }
        }

        private static bool LocationContainsGameObject(Vector2 location, BGameObject gameObject, bool useTextureBoxes)
        {
            var center = useTextureBoxes ? GetTextureBoxPosition(gameObject) : gameObject.CollisionBoxCenter;
            var size = useTextureBoxes ? gameObject.TextureBoxSize : gameObject.CollisionBoxSize;

            return Math.Abs(center.X - location.X) <= size.X / 2 &&
                   Math.Abs(center.Y - location.Y) <= size.Y / 2;
        }

        private static bool AreRegionsIntersecting(Vector2 lowerBound1,
            Vector2 upperBound1,
            Vector2 lowerBound2,
            Vector2 upperBound2)
        {
            // Check for intersection by checking if their x or y intervals intersect
            return AreIntervalsIntersecting(lowerBound1.X, upperBound1.X, lowerBound2.X, upperBound2.X) &&
                   AreIntervalsIntersecting(lowerBound1.Y, upperBound1.Y, lowerBound2.Y, upperBound2.Y);
        }

        private static bool AreRectangleAndCircleIntersecting(Vector2 rectLowerBound,
            Vector2 rectUpperBound,
            Vector2 circleCenter,
            float circleRadius)
        {
            var closestPoint =
                new Vector2(
                    MathHelper.Clamp(circleCenter.X, rectLowerBound.X, rectUpperBound.X), 
                    MathHelper.Clamp(circleCenter.Y, rectLowerBound.Y, rectUpperBound.Y));
            return Vector2.Distance(circleCenter, closestPoint) < circleRadius;
        }

        private static bool AreIntervalsIntersecting(float lowerBound1,
            float upperBound1,
            float lowerBound2,
            float upperBound2)
        {
            return !(upperBound1 <= lowerBound2 || upperBound2 <= lowerBound1);
        }

        private int GetQuadrantIndex(Vector2 location)
        {
            if (location.Y < mCenterCoordinates.Y)
            {
                if (location.X < mCenterCoordinates.X)
                {
                    return 0;
                }
                return 1;
            }

            if (location.X < mCenterCoordinates.X) return 2;
            return 3;
        }

        private bool IsGameObjectOutOfBounds(BGameObject gameObject)
        {
            return IsBoxOutOfBounds(gameObject.CollisionBoxCenter, gameObject.CollisionBoxSize) ||
                   IsBoxOutOfBounds(GetTextureBoxPosition(gameObject), gameObject.TextureBoxSize) ||
                   IsBoxOutOfBounds(gameObject.CollisionBoxCenter,
                       gameObject.CollisionBoxSize,
                       sGlobalCollisionBoxBoundaryCenter,
                       sGlobalCollisionBoxBoundarySize);
        }

        private bool IsLocationOutOfBounds(Vector2 location)
        {
            return Math.Abs(location.X - mCenterCoordinates.X) > mDimensionLength ||
                   Math.Abs(location.Y - mCenterCoordinates.Y) > mDimensionLength;
        }

        private bool IsBoxOutOfBounds(Vector2 center, Vector2 size)
        {
            return IsBoxOutOfBounds(center, size, mCenterCoordinates, 2 * mDimensionLength * Vector2.One);
        }

        private static bool IsBoxOutOfBounds(Vector2 center, Vector2 size, Vector2 boundsCenter, Vector2 boundsSize)
        {
            return center.X + size.X / 2 > boundsCenter.X + boundsSize.X / 2||
                   center.X - size.X / 2 < boundsCenter.X - boundsSize.X / 2 ||
                   center.Y + size.Y / 2 > boundsCenter.Y + boundsSize.Y / 2 ||
                   center.Y - size.Y / 2 < boundsCenter.Y - boundsSize.Y / 2;
        }

        private static Vector2 GetTextureBoxPosition(BGameObject gameObject)
        {
            return gameObject.Position + gameObject.TextureOffset + new Vector2(gameObject.Width, gameObject.Height) / 64;
        }

        private void DecrementCountOfNodeAndParents(int count = 1)
        {
            var currentQuadTree = this;
            while (currentQuadTree != null)
            {
                currentQuadTree.mGameObjectCount -= count;
                currentQuadTree = currentQuadTree.mParentQuadTree;
            }
        }

        private static List<TTarget> ConvertListToType<TOrigin, TTarget>(List<TOrigin> list)
        {
            var newList = new List<TTarget>();
            foreach (var obj in list)
            {
                if (obj is TTarget tObj) newList.Add(tObj);
            }

            return newList;
        }

        private void InsertInSubQuadTrees(BGameObject gameObject)
        {
            if (!AreRegionsIntersecting(gameObject.CollisionBoxCenter - gameObject.CollisionBoxOffset,
                gameObject.CollisionBoxCenter + gameObject.CollisionBoxOffset,
                mLowerBound,
                mUpperBound)) return;
            if (gameObject.CurrentQuadTree != this)
            {
                mGameObjectsFromUpperQuad.Add(gameObject);
                gameObject.CurrentSubQuadTrees.Add(this);
            }

            if (mSubQuadTrees[0] == null) return;

            for (var i = 0; i < 4; i++)
            {
                mSubQuadTrees[i].InsertInSubQuadTrees(gameObject);
            }
        }

        public void DrawQuadTree(SpriteBatch spriteBatch)
        {
            // if there is no sub trees the quad tree consists of just one quad
            if (mSubQuadTrees[0] == null) return;
            // Draw x-axis
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int)((mCenterCoordinates.X - mDimensionLength) * 32),
                    (int) (mCenterCoordinates.Y * 32),
                    (int) (2 * mDimensionLength * 32),
                    4),
                Color.White);
            // Draw y-axis
            spriteBatch.Draw(TextureManager.GetTexture("white"),
                new Rectangle((int) (mCenterCoordinates.X * 32),
                    (int) ((mCenterCoordinates.Y - mDimensionLength) * 32),
                    4,
                    (int) (2 * mDimensionLength * 32)),
                Color.White);

            // Draw sub quad trees
            for (var index = 0; index < 4; index++)
            {
                mSubQuadTrees[index].DrawQuadTree(spriteBatch);
            }
        }
    }
}
