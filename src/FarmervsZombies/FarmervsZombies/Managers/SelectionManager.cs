using System;
using System.Linq;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Menus;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FarmervsZombies.Managers
{
    internal sealed class SelectionManager
    {
        private Vector2 mMousePosition1;

        private bool mActive;

        private bool mBox;

        public readonly TileMenu mTileMenu;
        private readonly ObjectMenu mObjectMenu;

        public SelectionManager(TileMap tileMap, FogOfWar fog)
        {
            mTileMenu = new TileMenu(tileMap, fog);
            mObjectMenu = new ObjectMenu(tileMap, fog);
        }

        public void Initialize()
        {
            InputManager.AnyActionEvent += mTileMenu.StopFenceBuilding;
            InputManager.AnyActionEvent += mObjectMenu.StopFenceBuilding;
            GameStateManager.FinishInGameInputEvents += FinishBox;
        }

        public void MouseClick(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.MouseClick)) return;
            mActive = true;
            mMousePosition1 = inputState.mMouseWorldPosition;
            mBox = true;

            if (mTileMenu.FenceBuildMode || mTileMenu.Visible)
            {
                mBox = !mTileMenu.TileSelection(inputState);
                return;
            }

            if (mObjectMenu.FenceBuildMode || mObjectMenu.Visible)
            {
                mBox = !mObjectMenu.Selection(inputState);
                return;
            }

            if (!ObjectManager.Instance.GetSelected().Any() && !ObjectManager.Instance.GetQuadTree()
                    .GetGameObjects(new Vector2(mMousePosition1.X, mMousePosition1.Y), true).Any())
            {
                mBox = !mTileMenu.TileSelection(inputState);
                mObjectMenu.Clear();
            }
            else if (ObjectManager.Instance.GetSelected().Any() && ObjectManager.Instance.GetQuadTree()
                         .GetGameObjects(new Vector2(mMousePosition1.X, mMousePosition1.Y), true)
                         .Any(obj => obj.Selected))
            {
                mBox = !mObjectMenu.Selection(inputState);
                mTileMenu.Clear();
            }
            else
            {
                var selectedTowers = ObjectManager.Instance.GetSelected().Where(obj => obj is Tower).ToList();
                ObjectManager.Instance.ClearSelected();
                if (!mTileMenu.Visible && !mObjectMenu.Visible)
                    if (!UnitSelectionBox.Select(ObjectManager.Instance.GetQuadTree()
                            .GetGameObjects(new Vector2(mMousePosition1.X, mMousePosition1.Y), true),
                        true,
                        mMousePosition1))
                    {
                        mBox = !mTileMenu.TileSelection(inputState);
                        mObjectMenu.Clear();
                    }
                if (selectedTowers.Count > 0)
                {
                    var selectedEnemies = ObjectManager.Instance.TargetsInRange(InputManager.GetCurrentInputState().mMouseWorldPosition, 1).Where(obj => !obj.Team).Cast<BGameObject>().ToList();
                    foreach (var tower in selectedTowers)
                    {
                        ((Tower)tower).SetTargets(selectedEnemies, true);
                    }
                }
            }
        }

        public void ClearBox(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.ClearBox)) return;
            mActive = false;
            mBox = false;
            UnitSelectionBox.Instance.Width = 0;
            UnitSelectionBox.Instance.Height = 0;
        }

        private void FinishBox(object sender, EventArgs e)
        {
            mActive = false;
            mBox = false;
            UnitSelectionBox.Instance.Width = 0;
            UnitSelectionBox.Instance.Height = 0;
        }

        public void Update(InputState inputState, Menu.GameState gameState)
        {
            if ((UnitSelectionBox.Instance.Height > 0.2 || UnitSelectionBox.Instance.Width > 0.2) && mActive)
            {
                mTileMenu.Clear();
                mObjectMenu.Clear();
            }

            if (UnitSelectionBox.Instance.Height > 0.2 || UnitSelectionBox.Instance.Width > 0.2)
            {
                ObjectManager.Instance.ClearSelected();
                UnitSelectionBox.Select(ObjectManager.Instance.GetQuadTree().GetGameObjects(new Vector2(UnitSelectionBox.Instance.X, UnitSelectionBox.Instance.Y), new Vector2(UnitSelectionBox.Instance.X + UnitSelectionBox.Instance.Width, UnitSelectionBox.Instance.Y + UnitSelectionBox.Instance.Height), true), false, mMousePosition1);
            }

            if (mBox) UnitSelectionBox.Instance.Update(mMousePosition1);
            mTileMenu.Update(inputState, gameState);
            mObjectMenu.Update(inputState, gameState);
        }

        public void Draw(SpriteBatch spriteBatch, Matrix camTransform)
        {
            mTileMenu.Draw(spriteBatch, camTransform);
            mObjectMenu.Draw(spriteBatch, camTransform);
            if (mBox && (UnitSelectionBox.Instance.Height > 0.2 || UnitSelectionBox.Instance.Width > 0.2)) UnitSelectionBox.Instance.Draw(spriteBatch, camTransform);
        }

        public void Clear()
        {
            mActive = false;
            mBox = false;
            UnitSelectionBox.Instance.Width = 0;
            UnitSelectionBox.Instance.Height = 0;
            mTileMenu.Clear();
            mObjectMenu.Clear();
        }
    }
}