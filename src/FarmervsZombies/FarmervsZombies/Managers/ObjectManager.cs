using System;
using System.Collections.Generic;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Pathfinding.GameGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Managers
{
    /// <summary>
    /// Contains all game objects.
    /// 
    /// How to use:
    /// ObjectManager.Instance.method()
    /// </summary>
    internal sealed class ObjectManager
    {
        private static ObjectManager sInstance;

        private readonly QuadTree mQuadTree =
            new QuadTree(new Vector2((float) Game1.MapWidth / 2, (float) Game1.MapHeight / 2),
                Math.Max(Game1.MapWidth, Game1.MapHeight) / 2 + 1);

        private readonly List<BGameObject> mRemovalQueue = new List<BGameObject>();
        private readonly List<BMovableGameObject> mMovableObjects = new List<BMovableGameObject>();
        private readonly List<Tower> mTowers = new List<Tower>();
        private readonly List<Fence> mFences = new List<Fence>();
        private readonly List<PreviewFence> mPreviewFences = new List<PreviewFence>();
        private readonly List<ISelectable> mSelected = new List<ISelectable>();
        private readonly List<Zombie> mZombies = new List<Zombie>();
        private readonly List<MoonwalkZombie> mMoonwalkZombies = new List<MoonwalkZombie>();
        private readonly List<BAnimal> mAnimals = new List<BAnimal>();
        private readonly List<AttackChicken> mAttackChickens = new List<AttackChicken>();
        private readonly List<Wheat> mWheat = new List<Wheat>();
        private Farmer mFarmer;
        private Farmhouse mFarmhouse;
        private Graveyard mGraveyard;
        private bool mDrawQuadTree;
        public bool EndScreen { private get; set; }

        public static ObjectManager Instance => sInstance ?? (sInstance = new ObjectManager());

        public void Add(BGameObject obj)
        {
            if (EndScreen)
            {
                AddMoonwalkZombie(obj);
                return;
            }

            mQuadTree.InsertGameObject(obj);

            if (obj is Farmer farmer)
            {
                if (mFarmer == null) mFarmer = farmer;
                else mQuadTree.RemoveGameObject(obj);
            }
            else if (obj is Farmhouse house)
            {
                if (mFarmhouse == null) mFarmhouse = house;
                else mQuadTree.RemoveGameObject(obj);
            }
            else if (obj is Tower tower)
            {
                mTowers.Add(tower);
            }
            else if (obj is Fence fence)
            {
                mFences.Add(fence);
            }
            else if (obj is PreviewFence previewFence)
            {
                mPreviewFences.Add(previewFence);
            }
            else if (obj is Zombie zombie && !(obj is TreasureZombie))
            {
                mZombies.Add(zombie);
            }
            else if (obj is BAnimal animal)
            {
                mAnimals.Add(animal);
                Game1.sStatistics.SetAnimalsLiving(mAnimals.Count);
                Game1.sStatistics.IncreaseAnimals();
                if (animal is AttackChicken chicken)
                {
                    mAttackChickens.Add(chicken);
                }
            }
            else if (obj is Wheat wheat)
            {
                mWheat.Add(wheat);
            }
            else if (obj is Graveyard graveyard)
            {
                mGraveyard = graveyard;
            }
            

            if (obj is BMovableGameObject movable)
            {
                mMovableObjects.Add(movable);
            }

            if (obj is IPathCollidable)
            {
                for (var i = (int)obj.Position.X; i < Math.Ceiling(obj.Position.X + obj.CollisionBoxSize.X); i++)
                {
                    for (var j = (int)obj.Position.Y; j < Math.Ceiling(obj.Position.Y + obj.CollisionBoxSize.Y); j++)
                    {
                        if (obj is IAttackable) GameGraph.AddCollision(new Vector2(i, j), 20);
                        else GameGraph.AddCollision(new Vector2(i, j));
                    }
                }
                InvalidatePaths();
            }
        }

        private void AddMoonwalkZombie(BGameObject obj)
        {
            if (obj is MoonwalkZombie zombie)
            {
                mMoonwalkZombies.Add(zombie);
            }
        }

        public List<BGameObject> CheckTile(Vector2 position)
        {
            var gameObjects = mQuadTree.GetGameObjects(position, position + new Vector2(1, 1), false);

            return gameObjects;
        }

        public List<BGameObject> CheckTile(Vector2 position, Type type)
        {
            var gameObjects = mQuadTree.GetGameObjects(position, position + new Vector2(1, 1), false, type);

            return gameObjects;
        }

        public void AddSelected(BGameObject obj)
        {
            mSelected.Add(obj);
        }

        public void ClearSelected()
        {
            foreach (var i in mSelected)
            {
                i.Deselect();
            }

            mSelected.Clear();
        }

        public void Remove(BGameObject obj)
        {
            if (EndScreen)
            {
                RemoveMoonwalkZombie(obj);
                return;
            }

            AnimationManager.RemoveAnimations(obj);
            mQuadTree.RemoveGameObject(obj);
            if (obj is Farmer && mFarmer != null)
            {
                mFarmer = null;
            }
            else if(obj is Farmhouse)
            {
                if (mFarmhouse != null)
                {
                    mFarmhouse = null;
                    mQuadTree.RemoveGameObject(obj);
                }
            }
            else if (obj is Tower tower)
            {
                mTowers.Remove(tower);
            }
            else if (obj is Fence fence)
            {
                mFences.Remove(fence);
            }
            else if (obj is PreviewFence previewFence)
            {
                mPreviewFences.Remove(previewFence);
            }
            else if (obj is Zombie zombie)
            {
                mZombies.Remove(zombie);
                Game1.sStatistics.IncreaseZombiesKilled();
            }
            else if (obj is BAnimal animal)
            {
                mAnimals.Remove(animal);
                Game1.sStatistics.SetAnimalsLiving(mAnimals.Count);
                if (animal is AttackChicken chicken)
                {
                    mAttackChickens.Remove(chicken);
                }
            }
            else if (obj is Wheat wheat)
            {
                mWheat.Remove(wheat);
            }
            else if (obj is Graveyard && mGraveyard != null)
            {
                mGraveyard = null;
                mQuadTree.RemoveGameObject(obj);
            }

            if (obj is IPathCollidable)
            {
                for (var i = (int)obj.Position.X; i < Math.Ceiling(obj.Position.X + obj.CollisionBoxSize.X); i++)
                {
                    for (var j = (int)obj.Position.Y; j < Math.Ceiling(obj.Position.Y + obj.CollisionBoxSize.Y); j++)
                    {
                        GameGraph.RemoveCollision(new Vector2(i, j));
                    }
                }
                InvalidatePaths();
            }
        }

        private void RemoveMoonwalkZombie(BGameObject obj)
        {
            if (obj is MoonwalkZombie zombie)
            {
                mMoonwalkZombies.Remove(zombie);
            }
        }

        public void ClearMoonwalkers()
        {
            mMoonwalkZombies.Clear();
        }

        public void QueueRemoval(BGameObject obj)
        {
            mRemovalQueue.Add(obj);
        }

        private void InvalidatePaths()
        {
            foreach (var movable in mMovableObjects)
            {
                movable.InvalidatePath();
            }
        }

        public IEnumerable<BGameObject> GetList()
        {
            return mQuadTree.GetGameObjects();
        }

        public QuadTree GetQuadTree()
        {
            return mQuadTree;
        }

        public IEnumerable<IAttackable> TargetsInRange(Vector2 position, float range)
        {
            // Create list of all IAttackables in the specified range
            return mQuadTree.GetAttackablesInRange(position, range);
        }

        public void ClearPreviewFences()
        {
            foreach (var previewFence in mPreviewFences)
            {
                QueueRemoval(previewFence);
            }
        }

        public Farmer GetFarmer()
        {
            return mFarmer;
        }

        public Farmhouse GetFarmhouse()
        {
            return mFarmhouse;
        }

        public Graveyard GetGraveyard()
        {
            return mGraveyard;
        }

        public IEnumerable<Zombie> GetZombies()
        {
            return mZombies;
        }

        public IEnumerable<BAnimal> GetAnimals()
        {
            return mAnimals;
        }

        public IEnumerable<Tower> GetTowers()
        {
            return mTowers;
        }

        public IEnumerable<Wheat> GetWheat()
        {
            return mWheat;
        }

        public IEnumerable<Fence> GetFences()
        {
            return mFences;
        }

        public IEnumerable<ISelectable> GetSelected()
        {
            return mSelected;
        }

        public void Update(GameTime gameTime)
        {
            if (EndScreen)
            {
                foreach (var zombie in mMoonwalkZombies)
                {
                    zombie.Update(gameTime);
                }

                return;
            }

            var movables = new List<BMovableGameObject>();
            foreach (var obj in mQuadTree.GetGameObjects())
            {
                obj.Update(gameTime);
                if (obj is BMovableGameObject movableObj) movables.Add(movableObj);
            }

            foreach (var obj in movables)
            {
                obj.Move(gameTime);
            }

            foreach (var tower in mTowers)
            {
                var towerPosition = tower.Position;
                var attackablesInRange = mQuadTree.GetGameObjects(new Vector2(towerPosition.X, towerPosition.Y),
                    tower.Radius,
                    true,
                    typeof(IAttackable));
                var enemiesInRange = new List<BGameObject>();
                foreach (var obj in attackablesInRange)
                {
                    var attackable = (IAttackable)obj;
                    if (!attackable.Team)
                    {
                        enemiesInRange.Add((BGameObject)attackable);
                    }
                }

                tower.SetTargets(enemiesInRange);

                var removeProjectiles = new List<Projectile>();
                foreach (var projectile in tower.Projectiles)
                {
                    var hitObjects = mQuadTree.GetGameObjects(projectile.Position,
                        projectile.Position + new Vector2(projectile.Width / 32.0f, projectile.Height / 32.0f),
                        true,
                        typeof(IAttackable));
                    var hitEnemies = new List<IAttackable>();
                    foreach (var obj in hitObjects)
                    {
                        var attackable = (IAttackable) obj;
                        if (!attackable.Team)
                        {
                            hitEnemies.Add(attackable);
                            obj.Damage(tower.ProjectileDamage);
                        }
                    }

                    if (hitEnemies.Count > 0)
                    {
                        removeProjectiles.Add(projectile);
                    }
                }

                foreach (var projectile in removeProjectiles)
                {
                    tower.Projectiles.Remove(projectile);
                }
            }

            foreach (var chicken in mAttackChickens)
            {
                var removeProjectiles = new List<Projectile>();
                foreach (var projectile in chicken.Projectiles)
                {
                    var hitObjects = mQuadTree.GetGameObjects(projectile.Position,
                        projectile.Position + new Vector2(projectile.Width / 32.0f, projectile.Height / 32.0f),
                        true,
                        typeof(IAttackable));
                    var hitEnemies = new List<IAttackable>();
                    foreach (var obj in hitObjects)
                    {
                        var attackable = (IAttackable)obj;
                        if (chicken.Team != attackable.Team)
                        {
                            hitEnemies.Add(attackable);
                            obj.Damage(chicken.AttackDamage);
                        }
                    }

                    if (hitEnemies.Count > 0)
                    {
                        SoundManager.PlaySound("egg");
                        removeProjectiles.Add(projectile);
                    }
                }

                foreach (var projectile in removeProjectiles)
                {
                    chicken.Projectiles.Remove(projectile);
                }
            }

            foreach (var obj in mRemovalQueue)
            {
                Remove(obj);
            }

            mRemovalQueue.Clear();
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform, Rectangle visibleArea)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                null,
                SamplerState.PointClamp,
                null,
                null,
                null,
                camTransform);

            if (EndScreen)
            {
                foreach (var zombie in mMoonwalkZombies)
                {
                    zombie.Draw(spriteBatch, camTransform);
                }
            }

            foreach (var obj in mQuadTree.GetGameObjects(new Vector2(visibleArea.Left, visibleArea.Top) / 32,
                new Vector2(visibleArea.Right, visibleArea.Bottom) / 32,
                true))
            {
                if (EndScreen && !(obj is IPathCollidable)) continue;
                obj.Draw(spriteBatch, camTransform);
            }

            spriteBatch.End();
            if (mDrawQuadTree) DrawQuadTree(spriteBatch, camTransform);
        }

        private void DrawQuadTree(SpriteBatch spriteBatch, Matrix camTransform)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camTransform);
            mQuadTree.DrawQuadTree(spriteBatch);
            spriteBatch.End();
        }

        public void ToggleQuadTreeDraw(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ToggleQuadTreeDraw)) return;
            mDrawQuadTree = !mDrawQuadTree;
        }

        public void UnloadAll()
        {
            var gameObjects = mQuadTree.GetGameObjects();
            foreach (var gameObject in gameObjects)
            {
                Remove(gameObject);
            }
        }
    }
}