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
    internal sealed class TileMenu : ActionMenu
    {
        // EditorMode on/off
        private readonly bool EditorModeIsActive = false;

        public TileMenu(TileMap tileMap, FogOfWar fog) : base(tileMap, fog)
        {
            Debug.WriteLine("Successfully created TileMenu.");
        }

        public bool TileSelection(InputState inputState)
        {
            if (!mActive) return false;
            var buttonHover = mButtons.Any(button => button.mButtonState != ActionMenuButton.BtnStates.Default);
            if (!FenceBuildMode && !mButtons.Any())
            {
                if (buttonHover || inputState.mMouseWorldPosition.X < 0 || inputState.mMouseWorldPosition.X > mTileMap.Width ||
                        inputState.mMouseWorldPosition.Y < 0 || inputState.mMouseWorldPosition.Y > mTileMap.Height) return buttonHover;
                SetTile(inputState.mMouseWorldPosition);
            }
            else
            {
                return Selection();
            }

            return false;
        }

        private void SetTile(Vector2 worldPosition)
        {
            if (!mFog.FieldIsVisible((int) worldPosition.X, (int) worldPosition.Y)) return;
            mButtons.Clear();
            mWorldPosition = new Vector2((int)worldPosition.X, (int)worldPosition.Y);
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
            List<BGameObject> checkTile;
            switch (mTileMap.GetTileType((int)mWorldPosition.X, (int)mWorldPosition.Y))
            {
                case Tile.Grass:
                    checkTile = ObjectManager.Instance.CheckTile(mWorldPosition);
                    if (checkTile.Count == 0)
                    {
                        if (ObjectManager.Instance.GetFarmhouse() == null)
                        {
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 33), ActionBuildFarmhouse, "Farmhaus bauen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuildFarmhouse") + " Gold)"));
                        }
                        else
                        {
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 30), ActionBuildTower, "Turm bauen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuildTower") + " Gold)"));
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("fence", 32, 32, 1), ActionBuildFence, "Zaun bauen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuildFence") + " Gold)"));
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("terrain", 32, 32, 10), ActionPlow, "Feld pflügen"));
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("trap", 64, 64, 2), ActionBuildTrap, "Falle aufstellen (Preis: " + EconomyManager.Instance.GetPrice("ActionBuildTrap") + " Gold)"));
                        }

                        if (EditorModeIsActive)
                        {
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("water", 32, 32, 1), ActionPlantWater, "Wasser setzen"));
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("terrain", 32, 32, 25), ActionPlantBoulder, "Felsen setzen"));
                            result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 28), ActionPlantTree, "Baum pflanzen"));
                        }
                    }
                    break;
                case Tile.Dirt:
                    checkTile = ObjectManager.Instance.CheckTile(mWorldPosition, typeof(Wheat));
                    if (checkTile.Count == 0)
                    {
                            result.Add(new ActionMenuButton(mWorldPosition,
                                TextureManager.GetTexture("wheat", 32, 32, 3),
                                ActionPlantWheat1,
                                "Getreide 1 säen (Preis: " + EconomyManager.Instance.GetPrice("ActionPlantWheat1") +
                                ")"));
                            result.Add(new ActionMenuButton(mWorldPosition,
                                TextureManager.GetTexture("wheat2", 32, 32, 3),
                                ActionPlantWheat2,
                                "Getreide 2 säen (Preis: " + EconomyManager.Instance.GetPrice("ActionPlantWheat2") +
                                ")"));
                            result.Add(new ActionMenuButton(mWorldPosition,
                                TextureManager.GetTexture("icons", 32, 32, 2),
                                ActionPlantCorn,
                                "Mais säen (Preis: " + EconomyManager.Instance.GetPrice("ActionPlantCorn") + ")"));
                            result.Add(new ActionMenuButton(mWorldPosition,
                                TextureManager.GetTexture("icons", 32, 32, 32),
                                ActionPlantGrass,
                                "Gras pflanzen"));
                    }

                    break;
                case Tile.Water:
                    if (EditorModeIsActive) result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 32), ActionPlantGrass, "Gras pflanzen"));
                    break;
                case Tile.Wasteland:
                    result.Add(new ActionMenuButton(mWorldPosition, TextureManager.GetTexture("icons", 32, 32, 32), ActionRemoveWasteland, "Verdorbenen Boden entfernen (Preis: " + EconomyManager.Instance.GetPrice("ActionRemoveWasteland") + ")"));
                    break;

            }
            return result;
        }

        private List<Vector2> GetButtonPositions(int amount)
        {
            var centerPosition = new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y);
            return GetButtonPositions(amount, centerPosition);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            if (!mActive || !Visible) return;
            spriteBatch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, camTransform);
            if (mButtons.Count > 0) spriteBatch.Draw(TextureManager.GetTexture("tile_selected"), mWorldPosition * 32, Color.White);
            spriteBatch.End();
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

        // ------------------------------
        // Add Actions for buttons below
        // ------------------------------

        private void ActionBuildTower()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionBuildTower", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) {return;}
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildTower")) {return;}
            ObjectManager.Instance.Add(new Tower((int)mWorldPosition.X, (int)mWorldPosition.Y));
            SoundManager.PlaySound("building_short");
        }

        private void ActionBuildFence()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            FenceBuildMode = true;
            mFenceBuildPos1 = new Vector2((int)mWorldPosition.X, (int)mWorldPosition.Y);
            mFenceBuildPos2 = mFenceBuildPos1;
        }

        private void ActionBuildFarmhouse()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (ObjectManager.Instance.GetFarmhouse() != null) return;
            if (mWorldPosition.X < 3 || mWorldPosition.X > Game1.sTileMap.Width - 4 || mWorldPosition.Y < 3 ||
                mWorldPosition.Y > Game1.sTileMap.Height - 4)
            {
                SoundManager.PlaySound("test_sound2");
                NotificationManager.AddNotification("Du kannst dein Farmhaus nicht an den Rand bauen.", 2.0f);
                return;
            }

            var farmhouse = new Farmhouse((int) mWorldPosition.X, (int) mWorldPosition.Y);
            for (var i = (int)mWorldPosition.X - 1; i < (int)mWorldPosition.X + (int)farmhouse.CollisionBoxSize.X; i++)
            {
                for (var j = (int)mWorldPosition.Y; j < (int)mWorldPosition.Y + (int)farmhouse.CollisionBoxSize.Y + 1; j++)
                {
                    if (Game1.sTileMap.GetTileType(i, j) == Tile.Water)
                    {
                        SoundManager.PlaySound("test_sound2");
                        NotificationManager.AddNotification("Du kannst dein Farmhaus nicht so nah am Wasser bauen.", 2.0f);
                        return;
                    }
                }
            }

            if (ObjectManager.Instance.GetQuadTree().GetGameObjects(
                farmhouse.CollisionBoxCenter - farmhouse.CollisionBoxSize / 2 - Vector2.UnitX,
                farmhouse.CollisionBoxCenter + farmhouse.CollisionBoxSize / 2 + Vector2.UnitY,
                false,
                typeof(IPathCollidable)).Any())
            {
                SoundManager.PlaySound("test_sound2");
                NotificationManager.AddNotification("Du kannst dein Farmhaus nicht so nah an Bäumen/Steinen bauen.", 2.0f);
                return;
            }
            if (!FarmerQueueManager.Instance.Queue("ActionBuildFarmhouse", mWorldPosition)) { return; }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildFarmhouse")) { return; }
            ObjectManager.Instance.Add(farmhouse);
            SoundManager.PlaySound("building_short");
        }

        private void ActionBuildTrap()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionBuildTrap", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) { return; }
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildTrap")) { Clear(); return; }
            ObjectManager.Instance.Add(new Trap((int)mWorldPosition.X, (int)mWorldPosition.Y));
        }

        private void ActionPlow()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionPlow", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) { return; }
            if (!EconomyManager.Instance.GoldDecrease("ActionPlow")) { Clear(); return; }
            mTileMap.PlowTile((int)mWorldPosition.X, (int)mWorldPosition.Y);
            SoundManager.PlaySound("plow");
        }

        private void ActionPlantGrass()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionPlantGrass", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) { return; }
            if (!EconomyManager.Instance.GoldDecrease("ActionPlantGrass")) { Clear(); return; }
            mTileMap.PlantGrass((int)mWorldPosition.X, (int)mWorldPosition.Y);
        }

        private void ActionPlantWater()
        {
            Clear();
            mTileMap.PlantWater((int)mWorldPosition.X, (int)mWorldPosition.Y);
        }

        private void ActionPlantBoulder()
        {
            Clear();
            ObjectManager.Instance.Add(new Boulder((int)mWorldPosition.X, (int)mWorldPosition.Y));
        }

        private void ActionPlantTree()
        {
            Clear();
            ObjectManager.Instance.Add(new Tree((int)mWorldPosition.X, (int)mWorldPosition.Y));
        }

        private void ActionPlantWheat1()
        {
            ActionPlantWheat(Wheat.WheatType.Wheat1, "ActionPlantWheat1");
        }

        private void ActionPlantWheat2()
        {
            ActionPlantWheat(Wheat.WheatType.Wheat2, "ActionPlantWheat2");
        }

        private void ActionPlantCorn()
        {
            ActionPlantWheat(Wheat.WheatType.Corn, "ActionPlantCorn");
        }

        private void ActionPlantWheat(Wheat.WheatType wheatType, string action)
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!Game1.sTileMap.mPlowedTiles.ContainsKey(new Vector2(mWorldPosition.X, mWorldPosition.Y))) { return; }
            Game1.sTileMap.mPlowedTiles.Remove(new Vector2((int)mWorldPosition.X, (int)mWorldPosition.Y));
            if (!FarmerQueueManager.Instance.Queue(action, mWorldPosition))
            {
                Game1.sTileMap.mPlowedTiles.Add(new Vector2((int)mWorldPosition.X, (int)mWorldPosition.Y), 0);
                return;
            }

            if (!EconomyManager.Instance.SeedDecrease(action))
            {
                Game1.sTileMap.mPlowedTiles.Add(new Vector2((int)mWorldPosition.X, (int)mWorldPosition.Y), 0);
                Clear();
                return;
            }
            ObjectManager.Instance.Add(new Wheat((int)mWorldPosition.X, (int)mWorldPosition.Y, wheatType));
            SoundManager.PlaySound("harvest");
        }

        private void ActionRemoveWasteland()
        {
            Clear();
            if (ObjectManager.Instance.GetFarmer() != null) ObjectManager.Instance.GetFarmer().Select();
            if (!FarmerQueueManager.Instance.Queue("ActionRemoveWasteland", new Vector2(mWorldPosition.X + 0.5f, mWorldPosition.Y + 0.5f))) { return; }
            if (!EconomyManager.Instance.GoldDecrease("ActionRemoveWasteland")) { Clear(); return; }
            mTileMap.RemoveWasteland((int)mWorldPosition.X, (int)mWorldPosition.Y);
        }
    }
}
