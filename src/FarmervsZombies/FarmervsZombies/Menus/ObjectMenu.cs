using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Menus
{
    internal sealed class ObjectMenu : ActionMenu
    {
        private BGameObject mObject;

        // EditorMode on/off
#if DEBUG
        private const bool EditorModeIsActive = true;
#else
        private const bool EditorModeIsActive = false;
#endif
        private const int ShiftBuyMultiplier = 10;

        public ObjectMenu(TileMap tileMap, FogOfWar fog) : base(tileMap, fog)
        {
            Debug.WriteLine("Successfully created ObjectMenu.");
        }

        public bool Selection(InputState inputState)
        {
            if (!mActive) return false;

            var buttonHover = mButtons.Any(button => button.mButtonState != ActionMenuButton.BtnStates.Default);
            if (!FenceBuildMode && !buttonHover)
            {
                var objList = ObjectManager.Instance.GetQuadTree()
                    .GetGameObjects(new Vector2(inputState.mMouseWorldPosition.X, inputState.mMouseWorldPosition.Y),
                        true).Where(obj => obj.Selected).ToList();
                if (objList.Count != 0) SetObject(objList.First());
                else Clear();
            }
            else
            {
                return Selection();
            }

            return false;
        }

        private void SetObject(BGameObject obj)
        {
            mButtons.Clear();
            if (!obj.Selected)
            {
                Clear();
                return;
            }
            mWorldPosition = new Vector2(obj.Position.X, obj.Position.Y);
            mObject = obj;
            var buttons = GetButtons();
            var buttonPositions = GetButtonPositions(buttons.Count);
            for (var i = 0; i < buttons.Count; i++)
            {
                buttons[i].SetPosition(buttonPositions[i]);
                mButtons.Add(buttons[i]);
            }

            Visible = true;
        }

        // Add buttons here (Position of button does not matter. It will be set automatically)
        private List<ActionMenuButton> GetButtons()
        {
            var result = new List<ActionMenuButton>();
            if (mObject is Farmhouse)
            {
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("chicken_walk", 32, 32, 5),
                    ActionBuyAnimalChicken, "Huhn kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyAnimalChicken") + " Gold)"));

                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("cow_walk", 32, 32, 8),
                    ActionBuyAnimalCow, "Kuh kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyAnimalCow") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("pig_walk", 32, 32, 8),
                    ActionBuyAnimalPig, "Schwein kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyAnimalPig") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("wheat", 32, 32, 3),
                    ActionBuyWheat1, "Billiges Getreide kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyWheat1") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("wheat2", 32, 32, 3),
                    ActionBuyWheat2, "Hochwertiges Getreide kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyWheat2") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("icons", 32, 32, 2),
                    ActionBuyCorn, "Genmutierten Mais kaufen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuyCorn") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition,
                    TextureManager.GetTexture("icons", 32, 32, 19),
                    ActionDestroyFarmhouse, "Farmhaus zerstören (Profit: " + EconomyManager.Instance.GetProfit("ActionDestroyFarmhouse") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 10), CheatGold, "Umsatzsteuer-Karussel fahren (Profit: " + EconomyManager.Instance.GetProfit("CheatGold") + " Gold)"));
            }
            if (mObject is Fence fence)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("fence", 32, 32, 1), ActionBuildFence, "Zaun bauen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuildFence") + " Gold)"));
                if (fence.Level == 1) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 34), ActionUpgradeFence, "Zaun upgraden (Preis: " + EconomyManager.Instance.GetPrice("ActionUpgradeFence") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 19), ActionDestroyFence, "Zaun zerstören (Profit: " + EconomyManager.Instance.GetProfit(fence.Level == 1 ? "ActionDestroyFence" : "ActionDestroyFence2") + " Gold)"));
            }

            if (mObject is Tower tower)
            {
                string firemodeDescription;
                var nextFiremode = tower.GetNextMode();
                switch (nextFiremode)
                {
                    case Tower.FireMode.Closest:
                        firemodeDescription = "Nahe Gegner bevorzugen";
                        break;
                    case Tower.FireMode.Weakest:
                        firemodeDescription = "Schwache Gegner bevorzugen";
                        break;
                    case Tower.FireMode.Strongest:
                        firemodeDescription = "Starke Gegner bevorzugen";
                        break;
                    default:
                        firemodeDescription = "Nahe Gegner bevorzugen";
                        break;
                }
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 31), ActionSwitchTowerFireMode, "Feuermodus wechseln (" + firemodeDescription + ")"));
                if (((Tower)mObject).Level == 1) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 34), ActionUpgradeTower , "Turm upgraden (Preis: " + EconomyManager.Instance.GetPrice("ActionUpgradeTower") + " Gold)"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 19), ActionDestroyTower, "Turm zerstören (Profit: " + EconomyManager.Instance.GetProfit(tower.Level == 1 ? "ActionDestroyTower" : "ActionDestroyTower2") + " Gold)"));
            }

            if (mObject is Boulder)
            {
                if (EditorModeIsActive)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("terrain", 32, 32, 25), ActionRemoveBoulder, "Felsen zerstören"));
                }
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 19), ActionDestroyBoulder, "Felsen zerstören (Preis: "+ EconomyManager.Instance.GetPrice("ActionDestroyBoulder") + ")"));
            }

            if (mObject is Tree)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 28), ActionRemoveTree, "Baum fällen"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 19), ActionDestroyTree, "Baum fällen (Preis: " + EconomyManager.Instance.GetPrice("ActionDestroyTree") + ")"));
            }

            if (mObject is Wheat wheat)
            {
                if (wheat.Type == Wheat.WheatType.Wheat1) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 10), ActionHarvest, "Getreide ernten und verkaufen"));
                else if (wheat.Type == Wheat.WheatType.Wheat2) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 10), ActionHarvest, "Getreide ernten und verkaufen"));
                else if (wheat.Type == Wheat.WheatType.Corn) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 11), ActionHarvest, "Mais ernten und verkaufen"));
            }

            if (mObject is Trap)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 19), ActionDestroyTrap, "Falle zerstören (" + EconomyManager.Instance.GetProfit("ActionDestroyTrap") + " Gold)"));
            }

            if (mObject is Chicken chicken)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 17), CollectResources, "Gold sammeln (" + chicken.ResourcesHeld + ")"));
                if (!chicken.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 13), SlaughterAnimal, "Küken schreddern (" + (EconomyManager.Instance.GetProfit("ActionSlaughterChicken") * (chicken.LevelUp ? 3 : 1)) + " Gold, du Monster)"));
                }
                if (chicken.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 13), SlaughterAnimal, "Huhn schlachten (" + (EconomyManager.Instance.GetProfit("ActionSlaughterChicken") * (chicken.LevelUp ? 3 : 1)) + " Gold)"));
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 34), UpgradeChicken, "Huhn verbessern (" + EconomyManager.Instance.GetPrice("ActionBuyAttackChicken") + " Gold)"));
                }
            }

            if (mObject is Cow cow)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 18), CollectResources, "Gold sammeln (" + cow.ResourcesHeld + ")"));
                if (!cow.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 12), SlaughterAnimal, "Kalb schlachten (" + (EconomyManager.Instance.GetProfit("ActionSlaughterCow") * (cow.LevelUp ? 3 : 1)) + " Gold)"));
                }
                if (cow.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 12), SlaughterAnimal, "Kuh schlachten (" + (EconomyManager.Instance.GetProfit("ActionSlaughterCow") * (cow.LevelUp ? 3 : 1)) + " Gold)"));
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 34), UpgradeCow, "Kuh verbessern (" + EconomyManager.Instance.GetPrice("ActionBuyAttackCow") + " Gold)"));
                }
            }
            
            if (mObject is Pig pig)
            {
                if (!pig.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 16), SlaughterAnimal, "Schwein schlachten (" + (EconomyManager.Instance.GetProfit("ActionSlaughterPig") * (pig.LevelUp ? 3 : 1)) + " Gold)"));
                }
                if (pig.LevelUp)
                {
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 16), SlaughterAnimal, "Schwein schlachten (" + (EconomyManager.Instance.GetProfit("ActionSlaughterPig") * (pig.LevelUp ? 3 : 1)) + " Gold)"));
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 34), UpgradePig, "Schwein verbessern (" + EconomyManager.Instance.GetPrice("ActionBuyAttackPig") + " Gold)"));
                }
            }

            if (mObject is BAttackAnimal animal)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 29), ToggleAggression, animal.Aggressive ? "Aggression deaktivieren" : "Aggression aktivieren"));
            }

            if (mObject is AttackChicken attackChicken)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 31), ChickenAbility, attackChicken.RapidFire ? "Schnellfeuermodus deaktivieren" : "Schnellfeuermodus aktivieren"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 13), SlaughterAnimal, "Kampfhuhn schlachten (" + EconomyManager.Instance.GetProfit("ActionSlaughterAttackChicken") + " Gold)"));
            }

            if (mObject is AttackCow)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 31), CowAbility, "Berserkermodus aktivieren"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 12), SlaughterAnimal, "Kampfkuh schlachten (" + EconomyManager.Instance.GetProfit("ActionSlaughterAttackCow") + " Gold)"));
            }

            if (mObject is AttackPig)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 31), PigAbility, "Verspotten"));
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 16), SlaughterAnimal, "Kampfschwein schlachten (" + EconomyManager.Instance.GetProfit("ActionSlaughterAttackPig") + " Gold)"));
            }

            if (mObject is GraveyardTreasure treasure && !treasure.HasWatchers)
            {
                result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 31), CollectTreasure, "Schatz einsammeln"));
            }

            return result;
        }

        private List<Vector2> GetButtonPositions(int amount)
        {
            var centerPosition = new Vector2(mObject.Position.X + mObject.Width/64.0f, mObject.Position.Y);
            return GetButtonPositions(amount, centerPosition);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            if (!mActive || !Visible) return;
            foreach (var button in mButtons)
            {
                button.Draw(spriteBatch, camTransform);
            }

            foreach (var button in mButtons)
            {
                if (button.Tooltip == null) return;
                spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, null);
                button.Tooltip.Draw(spriteBatch);
                spriteBatch.End();
            }
        }

        public override void Update(InputState inputState, Menu.GameState gameState)
        {
            base.Update(inputState, gameState);
            if (Visible && mObject != null && mObject.Position != mWorldPosition)
            {
                mWorldPosition = mObject.Position;
                UpdateButtons();
            }
        }

        private void UpdateButtons()
        {
            var buttonPositions = GetButtonPositions(mButtons.Count);
            for (var i = 0; i < mButtons.Count; i++)
            {
                mButtons[i].SetPosition(buttonPositions[i]);
            }
        }

        // ------------------------------
        // Add Actions for buttons below
        // ------------------------------

        private void ActionSwitchTowerFireMode()
        {
            Clear();
            var selectedTowers = ObjectManager.Instance.GetSelected().Where(obj => obj is Tower).Cast<BGameObject>().ToList();
            foreach (var obj in selectedTowers)
            {
                var tower = (Tower)obj;
                tower.NextMode();
            }
        }

        private void ActionUpgradeTower()
        {
            Clear();
            var selectedTowers = ObjectManager.Instance.GetSelected().Where(obj => obj is Tower).Cast<BGameObject>().ToList();
            foreach (var obj in selectedTowers)
            {
                var tower = (Tower)obj;
                if (!tower.Upgradeable) continue;
                if (!EconomyManager.Instance.GoldDecrease("ActionUpgradeTower")) { Clear(); return; }
                tower.Upgrade();
            }
        }

        private void ActionDestroyTower()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedTowers = ObjectManager.Instance.GetSelected().Where(obj => obj is Tower).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyTower", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedTowers)) {Clear();return;}

            foreach (var obj in selectedTowers)
            {
                var tower = (Tower) obj;
                EconomyManager.Instance.GoldIncrease(tower.Level == 1 ? "ActionDestroyTower" : "ActionDestroyTower2",
                    tower);
                ObjectManager.Instance.Remove(tower);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyTrap()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedTraps = ObjectManager.Instance.GetSelected().Where(obj => obj is Trap).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyTower", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedTraps)) { Clear(); return; }

            foreach (var obj in selectedTraps)
            {
                var trap = (Trap)obj;
                EconomyManager.Instance.GoldIncrease("ActionDestroyTrap");
                ObjectManager.Instance.Remove(trap);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionBuildFence()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            FenceBuildMode = true;
            mFenceBuildPos1 = new Vector2((int)mObject.Position.X, (int)mObject.Position.Y);
            mFenceBuildPos2 = mFenceBuildPos1;
        }

        private void ActionUpgradeFence()
        {
            Clear();
            var selectedFences = ObjectManager.Instance.GetSelected().Where(obj => obj is Fence).Cast<BGameObject>().ToList();
            foreach (var obj in selectedFences)
            {
                var fence = (Fence) obj;
                if (!fence.Upgrade()) continue;
                if (EconomyManager.Instance.GoldDecrease("ActionUpgradeFence")) continue;
                Clear(); return;
            }
        }

        private void ActionDestroyFence()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedFences = ObjectManager.Instance.GetSelected().Where(obj => obj is Fence).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyFence", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedFences)) { Clear(); return; }

            foreach (var obj in selectedFences)
            {
                var fence = (Fence)obj;
                EconomyManager.Instance.GoldIncrease(fence.Level == 1 ? "ActionDestroyFence" : "ActionDestroyFence2", fence);
                ObjectManager.Instance.Remove(fence);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionDestroyFarmhouse()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyFarmhouse", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) {Clear(); return;}
            if (!Game1.sAchievements.Obdachlos)
            {
                Game1.sAchievements.Obdachlos = true;
                NotificationManager.AddNotification("Achievement freigeschaltet!", "Du hast das Achievement \"Obdachlos\" erreicht.", 6.0f);
            }
            var checkTile = ObjectManager.Instance.CheckTile(mWorldPosition, typeof(Farmhouse));
            if (checkTile.Count == 0) return;
            var obj = checkTile.First();
            EconomyManager.Instance.GoldIncrease("ActionDestroyFarmhouse", obj);
            SoundManager.PlaySound("destroy", 0.6f);
            ObjectManager.Instance.Remove(obj);
        }

        private void ActionBuyAnimalChicken()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 animals at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalChicken")) { Clear(); return; }
                    var chicken10 = new Chicken(mWorldPosition.X, mWorldPosition.Y + 3);
                    ObjectManager.Instance.Add(chicken10);
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalChicken")) { Clear(); return; }
            var chicken = new Chicken(mWorldPosition.X, mWorldPosition.Y + 3);
            ObjectManager.Instance.Add(chicken);
            SoundManager.PlaySound("chicken");
        }

        private void ActionBuyAnimalCow()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 animals at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalCow")) { Clear(); return; }
                    var cow10 = new Cow(mWorldPosition.X, mWorldPosition.Y + 2);
                    ObjectManager.Instance.Add(cow10);
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalCow")) { Clear(); return; }
            var cow = new Cow(mWorldPosition.X, mWorldPosition.Y + 2);
            ObjectManager.Instance.Add(cow);
            SoundManager.PlaySound("cow");
        }

        private void ActionBuyAnimalPig()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 animals at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {                    
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalPig")) { Clear(); return; }
                    var pig10 = new Pig(mWorldPosition.X, mWorldPosition.Y + 2);
                    ObjectManager.Instance.Add(pig10);
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyAnimalPig")) { Clear(); return; }
            var pig = new Pig(mWorldPosition.X, mWorldPosition.Y + 2);
            ObjectManager.Instance.Add(pig);
            SoundManager.PlaySound("pig");
        }

        private void ActionRemoveBoulder()
        {
            Clear();
            var selectedBoulder = ObjectManager.Instance.GetSelected().Where(obj => obj is Boulder).Cast<BGameObject>().ToList();
            foreach (var obj in selectedBoulder)
            {
                var boulder = (Boulder)obj;
                ObjectManager.Instance.Remove(boulder);
            }
        }

        private void ActionDestroyBoulder()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedBoulder = ObjectManager.Instance.GetSelected().Where(obj => obj is Boulder).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyBoulder", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedBoulder)) { Clear(); return; }
            foreach (var obj in selectedBoulder)
            {
                var boulder = (Boulder)obj;
                EconomyManager.Instance.GoldDecrease("ActionDestroyBoulder");
                ObjectManager.Instance.Remove(boulder);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }
        private void ActionRemoveTree()
        {
            Clear();
            var selectedTrees = ObjectManager.Instance.GetSelected().Where(obj => obj is Tree).Cast<BGameObject>().ToList();
            foreach (var obj in selectedTrees)
            {
                var tree = (Tree)obj;
                ObjectManager.Instance.Remove(tree);
            }
        }

        private void ActionDestroyTree()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedTrees = ObjectManager.Instance.GetSelected().Where(obj => obj is Tree).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionDestroyTree", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedTrees)) { Clear(); return; }
            foreach (var obj in selectedTrees)
            {
                var tree = (Tree)obj;
                EconomyManager.Instance.GoldDecrease("ActionDestroyTree");
                ObjectManager.Instance.Remove(tree);
            }
            SoundManager.PlaySound("destroy", 0.6f);
        }

        private void ActionHarvest()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedWheats = ObjectManager.Instance.GetSelected().Where(obj => obj is Wheat).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("ActionHarvest", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedWheats)) { Clear(); return; }
            foreach (var obj in selectedWheats)
            {
                var wheat = (Wheat)obj;
                EconomyManager.Instance.GoldIncrease("ActionHarvest", wheat);
                ObjectManager.Instance.Remove(wheat);
                Game1.sTileMap.mPlowedTiles.Add(obj.Position, 0);
            }
            SoundManager.PlaySound("harvest");
        }

        private void ActionBuyWheat1()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 wheats at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyWheat1")) { Clear(); return; }
                    EconomyManager.Instance.SeedAmount1 += 1;
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyWheat1")) { Clear(); return; }
            EconomyManager.Instance.SeedAmount1 += 1;
        }

        private void ActionBuyWheat2()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 wheats at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyWheat2")) { Clear(); return; }
                    EconomyManager.Instance.SeedAmount2 += 1;
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyWheat2")) { Clear(); return; }
            EconomyManager.Instance.SeedAmount2 += 1;
        }

        private void ActionBuyCorn()
        {
            Clear();
            // Shift Buy Multiplier (buy 10 wheats at once)
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    if (!EconomyManager.Instance.GoldDecrease("ActionBuyCorn")) { Clear(); return; }
                    EconomyManager.Instance.SeedAmount3 += 1;
                }
            }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuyCorn")) { Clear(); return; }
            EconomyManager.Instance.SeedAmount3 += 1;
        }

        private void CollectResources()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedAnimals = ObjectManager.Instance.GetSelected().Where(obj => obj is BAnimal).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("CollectResources", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedAnimals)) { Clear(); return; }
            foreach (var obj in selectedAnimals)
            {
                var animal = (BAnimal) obj;
                animal.CollectResources();
            }
        }

        private void SlaughterAnimal()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            var selectedAnimals = ObjectManager.Instance.GetSelected().Where(obj => obj is BAnimal).Cast<BGameObject>().ToList();
            if (!FarmerQueueManager.Instance.Queue("SlaughterAnimal", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f), selectedAnimals)) { Clear(); return; }
            foreach (var obj in selectedAnimals)
            {
                var animal = (BAnimal)obj;
                animal.Slaughter();
            }
        }

        private void UpgradeChicken()
        {
            Clear();
            var selectedChickens = ObjectManager.Instance.GetSelected().Where(obj => obj is Chicken).Cast<BGameObject>().ToList();
            foreach (var obj in selectedChickens)
            {
                var chicken = (Chicken)obj;
                chicken.Upgrade();
            }
        }

        private void UpgradeCow()
        {
            Clear();
            var selectedCows = ObjectManager.Instance.GetSelected().Where(obj => obj is Cow).Cast<BGameObject>().ToList();
            foreach (var obj in selectedCows)
            {
                var cow = (Cow)obj;
                cow.Upgrade();
            }
        }

        private void UpgradePig()
        {
            Clear();
            var selectedPigs = ObjectManager.Instance.GetSelected().Where(obj => obj is Pig).Cast<BGameObject>().ToList();
            foreach (var obj in selectedPigs)
            {
                var pig = (Pig)obj;
                pig.Upgrade();
            }
        }

        private void ChickenAbility()
        {
            Clear();
            var selectedAttackChickens = ObjectManager.Instance.GetSelected().Where(obj => obj is AttackChicken).Cast<BGameObject>().ToList();
            foreach (var obj in selectedAttackChickens)
            {
                var attackChicken = (AttackChicken)obj;
                attackChicken.SwitchAttackMode();
            }
        }

        private void CowAbility()
        {
            Clear();
            var selectedAttackCows = ObjectManager.Instance.GetSelected().Where(obj => obj is AttackCow).Cast<BGameObject>().ToList();
            foreach (var obj in selectedAttackCows)
            {
                var attackCow = (AttackCow)obj;
                attackCow.Enrage();
            }
        }

        private void PigAbility()
        {
            Clear();
            var selectedAttackPigs = ObjectManager.Instance.GetSelected().Where(obj => obj is AttackPig).Cast<BGameObject>().ToList();
            foreach (var obj in selectedAttackPigs)
            {
                var attackPig = (AttackPig)obj;
                attackPig.Taunt();
            }
        }

        private void ToggleAggression()
        {
            Clear();
            var selectedAttackAnimals = ObjectManager.Instance.GetSelected().Where(obj => obj is BAttackAnimal).Cast<BGameObject>().ToList();
            foreach (var obj in selectedAttackAnimals)
            {
                var attackAnimal = (BAttackAnimal)obj;
                attackAnimal.Aggressive = !attackAnimal.Aggressive;
            }
        }

        private void CollectTreasure()
        {
            Clear();
            if (!FarmerQueueManager.Instance.Queue("CollectTreasure", mObject)) { return; }
            if (mObject is GraveyardTreasure treasure)
            {
                treasure.CollectResources();
            }
        }

        private void CheatGold()
        {
            Clear();
            if (InputManager.GetCurrentInputState().IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                for (int i = 1; i < ShiftBuyMultiplier; i++)
                {
                    EconomyManager.Instance.GoldIncrease("CheatGold");
                }
            }
            EconomyManager.Instance.GoldIncrease("CheatGold");
        }
    }
}
