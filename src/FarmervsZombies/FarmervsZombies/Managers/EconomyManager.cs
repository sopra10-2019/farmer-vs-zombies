using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarmervsZombies.GameObjects;

namespace FarmervsZombies.Managers
{
    internal sealed class EconomyManager
    {
        /// <summary>
        /// If you wanna add Actions that change Player Money,
        /// add them to the dictionaries: mCostsDictionary and mProfitDictionary
        ///
        /// Use EconomyManager in your class like this:
        /// 
        /// EconomyManager.Instance
        ///
        /// GoldDecrease() is a bool function, if it returns false, the action cannot be executed.
        /// 
        ///action()
        ///     if (!EconomyManager.Instance.GoldDecrease("ActionBuildTower")) return;
        ///     // DO "ACTION_BUILD_TOWER"
        ///
        /// 
        /// GoldIncrease() is a void function that takes a string.
        /// So far you don't need an object as argument when you destroy buildings, the string is enough!
        /// 
        /// action()
        ///     EconomyManager.Instance.GoldIncrease("ActionHarvest");
        ///     // DO "ACTION_HARVEST"
        ///
        /// GoldIncrease() with a string and a Game Object
        /// If you wanna implement a "Harvest" Action, please give the Object as an argument
        /// 
        /// action()
        ///     var Object = ObjectManager.Instance.CheckTile(mWorldPosition);
        ///     if (Object == null) return;
        ///     EconomyManager.Instance.GoldIncrease("ActionHarvest", Object);
        /// </summary>

        private static EconomyManager sInstance;

        private int GoldAmountInitial { get; set; }
        public int GoldAmount { get; set; }

        // seeds 
        public int SeedAmount1 { get; set; }
        public int SeedAmount2 { get; set; }
        public int SeedAmount3 { get; set; }


        // Difficulty Start gold
        private const int GoldLevel1 = 350;
        private const int GoldLevel2 = 250;
        private const int GoldLevel3 = 150;

        // Profit = Stage * Factor
        // Wheat1 @ Stage3 == 5$
        // Wheat2 @ Stage3 == 40$
        // Corn @ Stage3 == 150$
        private const int Wheat1Factor = 1;
        private const int Wheat2Factor = 8;
        private const int CornFactor = 30;
        public static EconomyManager Instance => sInstance ?? (sInstance = new EconomyManager());


        private readonly Dictionary<string, int> mCostsDictionary = new Dictionary<string, int>()
        {
            // "Action Type" , "Costs in Gold/Seeds"
            {"ActionBuildTower", 25},
            {"ActionUpgradeTower", 100},
            {"ActionBuildFence", 3},
            {"ActionUpgradeFence", 50},
            {"ActionBuildFarmhouse", 100},
            {"ActionBuildTrap", 5},
            {"ActionPlow", 0},
            {"ActionPlantGrass", 0},
            {"ActionPlantWheat1", 1},
            {"ActionPlantWheat2", 1},
            {"ActionPlantCorn", 1},
            {"ActionBuyWheat1", 1},
            {"ActionBuyWheat2", 5},
            {"ActionBuyCorn", 10},
            {"ActionDestroyBoulder", 50},
            {"ActionDestroyTree", 50},
            {"ActionBuyAnimalChicken", 100},
            {"ActionBuyAnimalCow", 200},
            {"ActionBuyAnimalPig", 250},
            {"ActionBuyAttackChicken", 300},
            {"ActionBuyAttackCow", 350},
            {"ActionBuyAttackPig", 400},
            {"ActionRemoveWasteland", 50}
        };

        private readonly Dictionary<string, int> mProfitDictionary = new Dictionary<string, int>()
        {
            // "Action Type" , "Profits in Gold/Seeds"
            // For this code to work, ActionHarvest has to stay the only entry with Profit = 0 !
            {"ActionHarvest", 0},
            {"ActionDestroyTower", 5},
            {"ActionDestroyTower2", 45},
            {"ActionDestroyTrap", 2},
            {"ActionDestroyFence", 1},
            {"ActionDestroyFence2", 10},
            {"ActionDestroyFarmhouse", 50},
            {"ActionEgg", 5},
            {"ActionMilk", 25},
            {"ActionPork", 1},
            {"ActionSlaughterChicken", 50}, // Reduced by 1/3 because of calculations
            {"ActionSlaughterCow", 150},
            {"ActionSlaughterPig", 200},
            {"ActionSlaughterAttackChicken", 50},
            {"ActionSlaughterAttackCow", 150},
            {"ActionSlaughterAttackPig", 200},
            {"ActionKillChicken", 150},
            {"ActionKillCow", 450},
            {"ActionKillPig", 600},
            {"CheatGold", 1000}
        };

        private readonly Dictionary<int, int> mStageDictionary = new Dictionary<int, int>()
        {
            // "Stage" , "Profit-Multiplier in Gold"
            {0, 0},
            {1, 1},
            {2, 3},
            {3, 5}
        };

        public void Initialize()
        {
            switch (Game1.Difficulty)
            {
                case 1:
                    GoldAmountInitial = GoldLevel1;
                    break;
                case 2:
                    GoldAmountInitial = GoldLevel2;
                    break;
                case 3:
                    GoldAmountInitial = GoldLevel3;
                    break;
            }
            GoldAmount = GoldAmountInitial;
            SeedAmount1 = 0;
            SeedAmount2 = 0;
            SeedAmount3 = 0;
        }

        public void UpdateEconomy()
        {
        }

        public void GoldIncrease(string action, BGameObject a)
        {
            var increaseGold = 0;

            if (mProfitDictionary.TryGetValue(action, out var result2))
            {
                increaseGold += result2;
            }
            else
            {
                Debug.WriteLine("Gold Amount was not changed. Could not find the specified key.");
            }

            if (result2 == 0) // Harvest Action
            {
                if (a is Chicken)
                {
                    GoldIncrease("ActionKillChicken");
                    return;
                }
                if (a is Cow)
                {
                    GoldIncrease("ActionKillCow");
                    return;
                }
                if (a is Pig)
                {
                    GoldIncrease("ActionKillPig");
                    return;
                }
                if (mStageDictionary.TryGetValue(CheckStage(a), out var result1))
                {
                    increaseGold += result1;
                }
                else
                {
                    Debug.WriteLine("Gold Amount was not changed. Could not find the specified stage of Wheat.");
                }
            
                if (result1 > 0) // Stage is not 0
                {
                    if (CheckType(a) == Wheat.WheatType.Wheat1)
                    {
                        increaseGold *= Wheat1Factor;
                    }

                    if (CheckType(a) == Wheat.WheatType.Wheat2)
                    {
                        increaseGold *= Wheat2Factor;
                    }

                    if (CheckType(a) == Wheat.WheatType.Corn)
                    {
                        increaseGold *= CornFactor;
                    }
                }
                else
                {
                    Debug.WriteLine("Gold Amount was not Changed, so no Type of Wheat was checked.");
                }
            }

            // Gold increased! Time for a sound
            Game1.sAchievements.GoldIncrease(increaseGold);
            GoldAmount += increaseGold;
            if (increaseGold > 0) SoundManager.PlaySound("coin");
        }

        public bool SeedDecrease(string action, bool notification = true)
        {
            if (mCostsDictionary.TryGetValue(action, out var result))
            {
                switch (action)
                {
                    case "ActionPlantWheat1" when result > SeedAmount1:
                        // Not enough Seed!
                        if (notification) NotificationManager.AddNotification("Nicht genügend billiges Getreide zur Verfügung.", 2);
                        if (notification) SoundManager.PlaySound("test_sound2");
                        return false;
                    case "ActionPlantWheat1":
                        // Enough Seed
                        SeedAmount1 -= result;
                        return true;
                    case "ActionPlantWheat2" when result > SeedAmount2:
                        // Not enough Seed!
                        if (notification) NotificationManager.AddNotification("Nicht genügend hochwertiges Getreide zur Verfügung.", 2);
                        if (notification) SoundManager.PlaySound("test_sound2");
                        return false;
                    case "ActionPlantWheat2":
                        // Enough Seed
                        SeedAmount2 -= result;
                        return true;
                    case "ActionPlantCorn" when result > SeedAmount3:
                        // Not enough Seed!
                        if (notification) SoundManager.PlaySound("test_sound2");
                        if (notification) NotificationManager.AddNotification("Nicht genügend Gen-manipulierter Mais zur Verfügung.", 2);
                        return false;
                    case "ActionPlantCorn":
                        // Enough Seed
                        SeedAmount3 -= result;
                        return true;
                }
            }

            Debug.WriteLine("Seed Amount was not changed. Could not find the specified key.");
            return false;
            
        }
        
        public void GoldIncrease(string action, bool sound = true)
        {
            if (mProfitDictionary.TryGetValue(action, out var result))
            {
                GoldAmount += result;
                Game1.sAchievements.GoldIncrease(result);
            }
            else
            {
                Debug.WriteLine("Gold Amount was not changed. Could not find the specified key.");
            }

            // Gold increased! Time for a sound
            if (sound && result > 0) SoundManager.PlaySound("coin");
        }
        

        public bool GoldDecrease(string action)
        {
            if (mCostsDictionary.TryGetValue(action, out var result))
            {
                if (result > GoldAmount)
                {
                    // Not enough Gold!
                    SoundManager.PlaySound("test_sound2");
                    NotificationManager.AddNotification("Es steht nicht genügend Gold zur Verfügung.", 2);
                    return false;
                }

                // Enough Gold
                GoldAmount -= result;
                return true;
            }

            Debug.WriteLine("Gold Amount was not changed. Could not find the specified key.");
            return false;
        }

        public int GetPrice(string action)
        {
            if (!mCostsDictionary.ContainsKey(action)) throw new ArgumentException("Action " + action + " does not exist.");
            return mCostsDictionary[action];
        }

        public int GetProfit(string action)
        {
            if (!mProfitDictionary.ContainsKey(action)) throw new ArgumentException("Action " + action + " does not exist.");
            return mProfitDictionary[action];
        }

        private static int CheckStage(BGameObject a)
        {
            var stage = 0;
            if (a == null) return stage;
            var list = ObjectManager.Instance.GetWheat();
            foreach (var wheat in list)
            {
                if (a.Position == wheat.Position)
                {
                    stage = wheat.mStage;
                }
            }

            return stage;
        }

        private static Wheat.WheatType CheckType(BGameObject o)
        {
            var type = Wheat.WheatType.Wheat1;
            if (o == null) return type;
            
            var wheats = ObjectManager.Instance.GetWheat();
            foreach (var wheat in wheats)
            {
                if (o.Position == wheat.Position)
                {
                    type = wheat.Type;
                }
            }
            return type;
        }

        public void ResetGold()
        {
            GoldAmount = GoldAmountInitial;
        }
    }
}
