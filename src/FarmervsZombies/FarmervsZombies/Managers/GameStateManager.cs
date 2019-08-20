using System;
using System.Collections.Generic;
using FarmervsZombies.Menus;
using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies.Managers
{
    internal static class GameStateManager
    {
        private static bool sRegistered;
        public static Menu.GameState State
        {
            get => sCurrentGameState;
            set { if(sCurrentGameState == sUpcomingGameState) sUpcomingGameState = value; }
        }

        private static Menu.GameState sCurrentGameState;
        private static Menu.GameState sUpcomingGameState;

        public static void Update()
        {
            if (!sRegistered) RegisterOtherInputEvents();
            if (sCurrentGameState != sUpcomingGameState)
            {
                if (sUpcomingGameState == Menu.GameState.PauseMenu)
                {
                    InputManager.UnregisterAllInputEvents();
                    OnPause();
                }
                else if (sUpcomingGameState == Menu.GameState.PlayGameMenu)
                {
                    RegisterInGameInputEvents();
                    RegisterOtherInputEvents();
                }
                else
                {
                    InputManager.UnregisterAllInputEvents();
                    RegisterOtherInputEvents();
                }
            }
            sCurrentGameState = sUpcomingGameState;
        }

        public static event EventHandler FinishInGameInputEvents;

        private static void OnPause()
        {
            var handler = FinishInGameInputEvents;
            handler?.Invoke(null, null);
        }

        private static void RegisterInGameInputEvents()
        {
            InputManager.RegisterInputEvent(ActionType.ToggleFullscreen,
                new KeyEvent(Keys.F5, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.MouseClick,
                new MouseEvent(MouseButton.LeftButton, MouseEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ClearBox,
                new MouseEvent(MouseButton.LeftButton, MouseEventType.OnButtonReleased));

            InputManager.RegisterInputEvent(ActionType.BuyStack,
                new KeyEvent(Keys.LeftShift, KeyEventType.OnButtonDown));

            var keyEvents = new List<KeyEvent>
            {
                new KeyEvent(Keys.W, KeyEventType.OnButtonDown),
                new KeyEvent(Keys.A, KeyEventType.OnButtonDown),
                new KeyEvent(Keys.D, KeyEventType.OnButtonDown),
                new KeyEvent(Keys.S, KeyEventType.OnButtonDown),
                new KeyEvent(Keys.Left, KeyEventType.OnButtonPressed),
                new KeyEvent(Keys.Right, KeyEventType.OnButtonPressed),
                new KeyEvent(Keys.Up, KeyEventType.OnButtonPressed),
                new KeyEvent(Keys.Down, KeyEventType.OnButtonPressed)

            };
            InputManager.RegisterInputEvent(ActionType.MoveFarmer, keyEvents);

            InputManager.RegisterInputEvent(ActionType.MoveToTarget,
                new KeyEvent(Keys.M, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.MoveToTarget,
                new MouseEvent(MouseButton.RightButton, MouseEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.SpawnNecromancer,
                new KeyEvent(Keys.N, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenChicken,
                new KeyEvent(Keys.C, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenCow,
                new KeyEvent(Keys.V, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenPig,
                new KeyEvent(Keys.B, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenAChicken,
                new KeyEvent(Keys.N, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenACow,
                new KeyEvent(Keys.M, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.GenAPig,
                new KeyEvent(Keys.J, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.CastAbility,
                new KeyEvent(Keys.K, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ToggleQuadTreeDraw,
                new KeyEvent(Keys.T, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ToggleFollowFarmer,
                new KeyEvent(Keys.Space, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.Charge,
                new KeyEvent(Keys.Z, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ToggleFogOfWar,
                new KeyEvent(Keys.F, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ToggleFps,
                new KeyEvent(Keys.F3, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.SetTarget,
                new MouseEvent(MouseButton.RightButton, MouseEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.StopMovement,
                new KeyEvent(Keys.X, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.StopFenceBuilding,
                new MouseEvent(MouseButton.RightButton, MouseEventType.OnButtonPressed));
        }

        private static void RegisterOtherInputEvents()
        {
            InputManager.RegisterInputEvent(ActionType.GeneratePerformanceDemo,
                new KeyEvent(Keys.F2, KeyEventType.OnButtonPressed));

            InputManager.RegisterInputEvent(ActionType.ExitEndScreen,
                new KeyEvent(Keys.Escape, KeyEventType.OnButtonPressed));

            sRegistered = true;
        }
    }
}
