using System.Collections.Generic;
using FarmervsZombies.GameObjects;
using Microsoft.Xna.Framework;
using System.Linq;
using FarmervsZombies.Menus;

namespace FarmervsZombies.Managers
{
    internal sealed class FarmerQueueManager
    {
        private static FarmerQueueManager sInstance;

        private string mActionToDo = "nothing";
        private BGameObject mObject;
        private List<BGameObject> mObjects;
        private ActionMenu mFenceMenu;
        private Vector2 mActionPosition;
        private const float InternalCooldown = 1f;
        private float mTimeSinceCheck = 2f;
        private bool mIsempty = true;
        private bool mInRange;
        // The range should be 0.05f * 32f -> for testing purposes bigger
        private float mFarmerActionRange = 0.07f * 32f;

        public static FarmerQueueManager Instance => sInstance ?? (sInstance = new FarmerQueueManager());


        public void Update(GameTime gameTime)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return;
            if (InputManager.GetCurrentInputState().IsButtonPressed(MouseButton.RightButton)
                && farmer.Selected)
            {
                EmptyFQueue();
            }
            if (mTimeSinceCheck > InternalCooldown && mActionToDo != "nothing")
            {
                CheckUpdate(mActionPosition);
                if (!mInRange)
                {
                    SendFarmer();
                }
                else
                {
                    FinallyDoAction();
                }

                mTimeSinceCheck = 0f;
            }
            
            mTimeSinceCheck += gameTime.ElapsedGameTime.Milliseconds / 1000f;
        }

        private void SendFarmer()
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (mObject != null)
            {
                farmer?.RequestPath(mObject.CollisionBoxCenter);
                return;
            }
            farmer?.RequestPath(mActionPosition);
        }

        private void FinallyDoAction()
        {
            switch (mActionToDo)
            {
                case "ActionBuildTower":
                    ActionBuildTower();
                    break;
                case "ActionBuildTrap":
                    ActionBuildTrap();
                    break;
                case "ActionBuildFences":
                    ActionBuildFences();
                    break;
                case "ActionBuildFarmhouse":
                    ActionBuildFarmhouse();
                    mFarmerActionRange = 0.07f * 32f; // reset bigger range after building farmhouse
                    break;
                case "ActionPlow":
                    ActionPlow();
                    break;
                case "ActionPlantGrass":
                    ActionPlantGrass();
                    break;
                case "ActionPlantWheat1":
                    ActionPlantWheat1();
                    break;
                case "ActionPlantWheat2":
                    ActionPlantWheat2();
                    break;
                case "ActionPlantCorn":
                    ActionPlantCorn();
                    break;
                case "ActionHarvest":
                    ActionHarvest();
                    break;
                case "ActionDestroyTower":
                    ActionDestroyTower();
                    break;
                case "ActionDestroyTrap":
                    ActionDestroyTrap();
                    break;
                case "ActionDestroyFence":
                    ActionDestroyFence();
                    break;
                case "ActionDestroyBoulder":
                    ActionDestroyBoulder();
                    break;
                case "ActionDestroyTree":
                    ActionDestroyTree();
                    break;
                case "ActionDestroyFarmhouse":
                    ActionDestroyFarmhouse();
                    mFarmerActionRange = 0.07f * 32f; // reset bigger range after destroying farmhouse
                    break;
                case "CollectResources":
                    CollectResources();
                    break;
                case "SlaughterAnimal":
                    SlaughterAnimal();
                    break;
                case "ActionRemoveWasteland":
                    ActionRemoveWasteland();
                    break;
                case "CollectTreasure":
                    CollectTreasure();
                    break;
            }

            EmptyFQueue();
        }

        private void ChangeQueue(string action, Vector2 position)
        {
            mActionToDo = action;
            mActionPosition = position;
            mIsempty = false;
        }

        public bool Queue(string action, Vector2 target)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return false;
            var farmerPosition = farmer.Position;
            var distance = target - farmerPosition;
            mInRange = distance.Length() < mFarmerActionRange;
            if (mInRange) return mInRange;
            ChangeQueue(action, target);
            SendFarmer();
            return mInRange;
        }

        public bool Queue(string action, Vector2 target, List<BGameObject> objects)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return false;
            var farmerPosition = farmer.Position;
            var distance = target - farmerPosition;
            mObjects = objects;
            mInRange = distance.Length() < mFarmerActionRange;
            if (mInRange) return mInRange;
            ChangeQueue(action, target);
            SendFarmer();
            return mInRange;
        }

        public bool Queue(string action, BGameObject objectToDo)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return false;
            mObject = objectToDo;
            if (mObject == null) return false;
            var farmerPosition = farmer.Position;
            var distance = mObject.Position - farmerPosition;
            mInRange = distance.Length() < mFarmerActionRange;
            if (mInRange) return mInRange;
            ChangeQueue(action, mObject.Position);
            SendFarmer();
            return mInRange;
        }

        public bool Queue(string action, Vector2 target, ActionMenu currentFenceBuildingMenu)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return false;
            mFenceMenu = currentFenceBuildingMenu;
            var farmerPosition = farmer.Position;
            var distance = target - farmerPosition;
            mInRange = distance.Length() < mFarmerActionRange;
            if (mInRange) return mInRange;
            ChangeQueue(action, target);
            SendFarmer();
            return mInRange;
        }

        public void EmptyFQueue()
        {
            if (mIsempty) return;
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer != null)
            {
                Instance.ChangeQueue("nothing", farmer.CollisionBoxCenter);
                farmer.RequestPath(farmer.CollisionBoxCenter);
            }
            mIsempty = true;
            mObject = null;
            mObjects = null;
        }

        private void CheckUpdate(Vector2 target)
        {
            var farmer = ObjectManager.Instance.GetFarmer();
            if (farmer == null) return;
            var farmerPosition = farmer.Position;
            if (mObject != null) target = mObject.CollisionBoxCenter;
            var distance = target - farmerPosition;
            if (mActionToDo == "ActionDestroyFarmhouse" || mActionToDo == "ActionBuildFarmhouse")
            {
                // bigger range for working with farmhouse
                mFarmerActionRange = 0.12f * 32f; 
            }
            mInRange = distance.Length() < mFarmerActionRange;
        }

        private void ActionBuildTower()
        {
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildTower")) return;
            ObjectManager.Instance.Add(new Tower((int)mActionPosition.X, (int)mActionPosition.Y));
            SoundManager.PlaySound("building_short");
        }

        private void ActionBuildTrap()
        {
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildTrap")) return;
            ObjectManager.Instance.Add(new Trap((int)mActionPosition.X, (int)mActionPosition.Y));
        }

        private void ActionBuildFences()
        {
            mFenceMenu?.BuildFences();
        }

        private void ActionBuildFarmhouse()
        {
            if (ObjectManager.Instance.GetFarmhouse() != null) return;
            var tileMap = Game1.sTileMap;
            var water1 = tileMap.GetTileType((int)mActionPosition.X + 1, (int)mActionPosition.Y);
            var water2 = tileMap.GetTileType((int)mActionPosition.X, (int)mActionPosition.Y + 1);
            var water3 = tileMap.GetTileType((int)mActionPosition.X + 1, (int)mActionPosition.Y + 1);
            var water4 = tileMap.GetTileType((int)mActionPosition.X, (int)mActionPosition.Y + 2);
            var water5 = tileMap.GetTileType((int)mActionPosition.X + 1, (int)mActionPosition.Y +2);
            var water6 = tileMap.GetTileType((int)mActionPosition.X - 1, (int)mActionPosition.Y + 1);
            var water7 = tileMap.GetTileType((int)mActionPosition.X - 1, (int)mActionPosition.Y + 2);
            var water8 = tileMap.GetTileType((int)mActionPosition.X - 1, (int)mActionPosition.Y + 3);
            if (water1 != Tile.Grass || water2 != Tile.Grass || water3 != Tile.Grass || water4 != Tile.Grass || water5 != Tile.Grass || water6 != Tile.Grass || water7 != Tile.Grass || water8 != Tile.Grass)
            {
                SoundManager.PlaySound("test_sound2");
                NotificationManager.AddNotification("Du kannst dein Farmhaus nicht so nah am Wasser bauen.", 2.0f);
                return;
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildFarmhouse")) { return; }
            ObjectManager.Instance.Add(new Farmhouse((int)mActionPosition.X, (int)mActionPosition.Y));
            SoundManager.PlaySound("building_short");
        }

        private void ActionPlow()
        {
            if (!EconomyManager.Instance.GoldDecrease("ActionPlow")) { return; }
            var tileMap = Game1.sTileMap;
            tileMap.PlowTile((int)mActionPosition.X, (int)mActionPosition.Y);
        }
        
        private void ActionPlantGrass()
        {
            if (!EconomyManager.Instance.GoldDecrease("ActionPlantGrass")) { return; }
            var tileMap = Game1.sTileMap;
            tileMap.PlantGrass((int)mActionPosition.X, (int)mActionPosition.Y);
        }

        private void ActionPlantWheat1()
        {
            if (!EconomyManager.Instance.SeedDecrease("ActionPlantWheat1")) {  return; }
            if (!Game1.sTileMap.mPlowedTiles.ContainsKey(new Vector2(mActionPosition.X, mActionPosition.Y))) return;
            ObjectManager.Instance.Add(new Wheat((int)mActionPosition.X, (int)mActionPosition.Y, Wheat.WheatType.Wheat1));
            Game1.sTileMap.mPlowedTiles.Remove(new Vector2((int)mActionPosition.X, (int)mActionPosition.Y));
            SoundManager.PlaySound("harvest");
        }

        private void ActionPlantWheat2()
        {
            if (!EconomyManager.Instance.SeedDecrease("ActionPlantWheat2")) {  return; }
            if(!Game1.sTileMap.mPlowedTiles.ContainsKey(new Vector2(mActionPosition.X, mActionPosition.Y))) return;
            ObjectManager.Instance.Add(new Wheat((int)mActionPosition.X, (int)mActionPosition.Y, Wheat.WheatType.Wheat2));
            Game1.sTileMap.mPlowedTiles.Remove(new Vector2((int)mActionPosition.X, (int)mActionPosition.Y));
            SoundManager.PlaySound("harvest");
        }

        private void ActionPlantCorn()
        {
            if (!EconomyManager.Instance.SeedDecrease("ActionPlantCorn")) {  return; }
            if (!Game1.sTileMap.mPlowedTiles.ContainsKey(new Vector2(mActionPosition.X, mActionPosition.Y))) return;
            ObjectManager.Instance.Add(new Wheat((int)mActionPosition.X, (int)mActionPosition.Y, Wheat.WheatType.Corn));
            Game1.sTileMap.mPlowedTiles.Remove(new Vector2((int)mActionPosition.X, (int)mActionPosition.Y));
            SoundManager.PlaySound("harvest");
        }

        private void ActionHarvest()
        {
            if (mObjects == null) return;
            foreach (var obj in mObjects)
            {
                var wheat = (Wheat)obj;
                EconomyManager.Instance.GoldIncrease("ActionHarvest", wheat);
                ObjectManager.Instance.Remove(wheat);
                Game1.sTileMap.mPlowedTiles.Add(obj.Position, 0);
            }
            SoundManager.PlaySound("harvest");
        }

        private void ActionDestroyTower()
        {
            foreach (var obj in mObjects)
            {
                var trap = (Tower)obj;
                EconomyManager.Instance.GoldIncrease("ActionDestroyTrap");
                ObjectManager.Instance.Remove(trap);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyTrap()
        {
            foreach (var obj in mObjects)
            {
                var trap = (Trap)obj;
                EconomyManager.Instance.GoldIncrease("ActionDestroyTrap");
                ObjectManager.Instance.Remove(trap);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }
        private void ActionDestroyFence()
        {
            foreach (var obj in mObjects)
            {
                var fence = (Fence)obj;
                EconomyManager.Instance.GoldIncrease(fence.Level == 1 ? "ActionDestroyFence" : "ActionDestroyFence2", fence);
                ObjectManager.Instance.Remove(fence);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyBoulder()
        {
            foreach (var obj in mObjects)
            {
                var boulder = (Boulder)obj;
                EconomyManager.Instance.GoldDecrease("ActionDestroyBoulder");
                ObjectManager.Instance.Remove(boulder);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyTree()
        {
            foreach (var obj in mObjects)
            {
                var tree = (Tree)obj;
                EconomyManager.Instance.GoldDecrease("ActionDestroyTree");
                ObjectManager.Instance.Remove(tree);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyFarmhouse()
        {
            if (!Game1.sAchievements.Obdachlos)
            {
                Game1.sAchievements.Obdachlos = true;
            }
            var checkTile = ObjectManager.Instance.CheckTile(mActionPosition, typeof(Farmhouse));
            if (checkTile.Count == 0) return;
            var obj = checkTile.First();
            EconomyManager.Instance.GoldIncrease("ActionDestroyFarmhouse", obj);
            SoundManager.PlaySound("destroy", 0.6f);
            ObjectManager.Instance.Remove(obj);
        }

        private void CollectResources()
        {
            foreach (var obj in mObjects)
            {
                var animal = (BAnimal)obj;
                animal.CollectResources();
            }
        }

        private void SlaughterAnimal()
        {
            foreach (var obj in mObjects)
            {
                var animal = (BAnimal)obj;
                animal.Slaughter();
                SoundManager.PlaySound("chopping_block");
            }
        }

        private void ActionRemoveWasteland()
        {
            if (!EconomyManager.Instance.GoldDecrease("ActionRemoveWasteland")) { return; }
            Game1.sTileMap.RemoveWasteland((int)mActionPosition.X, (int)mActionPosition.Y);
        }

        private void CollectTreasure()
        {
            if (mObject is GraveyardTreasure treasure)
            {
                treasure.CollectResources();
            }
        }
    }
}
