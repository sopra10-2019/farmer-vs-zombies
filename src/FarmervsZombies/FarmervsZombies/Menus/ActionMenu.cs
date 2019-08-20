using System;
using System.Collections.Generic;
using System.Linq;
using FarmervsZombies.GameObjects;
using FarmervsZombies.Managers;
using FarmervsZombies.MenuButtons;
using FarmervsZombies.Pathfinding;
using Microsoft.Xna.Framework;

namespace FarmervsZombies.Menus
{
    internal class ActionMenu
    {
        protected Vector2 mWorldPosition = Vector2.Zero;
        protected readonly TileMap mTileMap;
        protected readonly FogOfWar mFog;
        protected readonly List<ActionMenuButton> mButtons;
        protected bool mActive;
        public bool Visible { get; protected set; }
        private const float Radius = 2;
        private const double Angle = Math.PI / 5;
        private bool mBuilding;
        private bool mFenceBuildMode;
        public bool FenceBuildMode
        {
            get => mFenceBuildMode;
            set
            {
                mFenceBuildMode = value;
                if (value)
                    InputManager.ReserveInputEvent(ActionType.StopFenceBuilding,
                        new MouseEvent(MouseButton.RightButton, MouseEventType.OnButtonPressed));
                else
                    InputManager.UnreserveInputEvent(new MouseEvent(MouseButton.RightButton,
                        MouseEventType.OnButtonPressed));
            }
        }

        protected Vector2 mFenceBuildPos1 = Vector2.Zero;
        protected Vector2 mFenceBuildPos2 = Vector2.Zero;
        
        protected ActionMenu(TileMap tileMap, FogOfWar fog)
        {
            mTileMap = tileMap;
            mButtons = new List<ActionMenuButton>();
            mFog = fog;
        }

        protected bool Selection()
        {
            if (!mActive) return false;
            if (FenceBuildMode)
            {
                mBuilding = true;
                if (!FarmerQueueManager.Instance.Queue("ActionBuildFences", mFenceBuildPos1, this)) { return true; }
                BuildFences();
                return true;
            }

            var buttonHover = mButtons.Any(button => button.mButtonState != ActionMenuButton.BtnStates.Default);
            if (mButtons.Any() && !buttonHover)
            {
                Clear();
            }

            return buttonHover;
        }

        public void BuildFences()
        {
            ObjectManager.Instance.ClearPreviewFences();
            var fencePath = Pathfinder.GetFullPath(mFenceBuildPos1 + new Vector2(0.5f, 0.5f), mFenceBuildPos2 + new Vector2(0.5f, 0.5f));
            var current = mFenceBuildPos1 + new Vector2(0.5f, 0.5f);
            var builtAny = BuildAFence(current);
            while (!fencePath.TargetReached(current))
            {
                current = fencePath.Next(current);
                if (BuildAFence(current)) builtAny = true;
            }
            if (builtAny) SoundManager.PlaySound("building_short");
            mFenceBuildPos1 = mFenceBuildPos2;
            mBuilding = false;
        }

        private bool BuildAFence(Vector2 current)
        {
            if (!mFog.FieldIsVisible((int)current.X, (int)current.Y)) return false;
            if (ObjectManager.Instance
                    .CheckTile(new Vector2((int)current.X, (int)current.Y), typeof(IPathCollidable))
                    .Count != 0 ||
                mTileMap.GetTileType((int)current.X, (int)current.Y) != Tile.Grass) return false;
            if (!EconomyManager.Instance.GoldDecrease("ActionBuildFence"))
            {
                FenceBuildMode = false;
                return false;
            }
            ObjectManager.Instance.Add(new Fence((int)current.X, (int)current.Y));
            return true;
        }

        public void StopFenceBuilding(object sender, InputState inputState)
        {
            if (!inputState.IsActionActive(ActionType.StopFenceBuilding) || !FenceBuildMode) return;
            FenceBuildMode = false;
            ObjectManager.Instance.ClearPreviewFences();
        }

        public void Clear()
        {
            Visible = false;
            mButtons.Clear();
        }

        protected List<Vector2> GetButtonPositions(int amount, Vector2 centerPosition)
        {
            var result = new List<Vector2>();
            for (double i = 0; i <= (amount - 1) * Angle; i += Angle)
            {
                result.Add(new Vector2(
                    centerPosition.X + Radius * (float)Math.Sin(i - ((amount - 1) * Angle) / 2) -
                    (float)ActionMenuButton.Width / 64,
                    centerPosition.Y + Radius * -(float)Math.Cos(i - ((amount - 1) * Angle) / 2)));
            }
            return result;
        }

        public virtual void Update(InputState inputState, Menu.GameState gameState)
        {
            mActive = gameState == Menu.GameState.PlayGameMenu;

            if (FenceBuildMode && !mBuilding)
            {
                var mouseWorldPosition = InputManager.GetCurrentInputState().mMouseWorldPosition;
                var lastFenceBuildPos2 = mFenceBuildPos2;
                mFenceBuildPos2 = new Vector2((int)mouseWorldPosition.X, (int)mouseWorldPosition.Y);

                if (mFenceBuildPos2 != lastFenceBuildPos2)
                {
                    ObjectManager.Instance.ClearPreviewFences();
                    var fencePath = Pathfinder.GetFullPath(mFenceBuildPos1 + new Vector2(0.5f, 0.5f), mFenceBuildPos2 + new Vector2(0.5f, 0.5f));
                    var current = mFenceBuildPos1 + new Vector2(0.5f, 0.5f);
                    var checkTile = ObjectManager.Instance.CheckTile(new Vector2((int)current.X, (int)current.Y), typeof(ICollidable));
                    if (checkTile.Count == 0 || !checkTile.Exists(obj => !(obj is BMovableGameObject))) ObjectManager.Instance.Add(new PreviewFence((int)current.X, (int)current.Y));
                    while (!fencePath.TargetReached(current))
                    {
                        current = fencePath.Next(current);
                        checkTile = ObjectManager.Instance.CheckTile(new Vector2((int)current.X, (int)current.Y), typeof(ICollidable));
                        if (checkTile.Count == 0 || !checkTile.Exists(obj => !(obj is BMovableGameObject))) ObjectManager.Instance.Add(new PreviewFence((int)current.X, (int)current.Y));
                    }
                }
            }

            if (!mActive) return;
            foreach (var button in mButtons)
            {
                button.Update(inputState);
                if (mButtons.Count == 0) break;
            }
        }
    }
}
