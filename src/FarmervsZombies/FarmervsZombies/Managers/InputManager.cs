using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies.Managers
{
    /// <summary>
    /// Manages all incoming inputs and sends its information through events.
    /// 
    /// How to subscribe to events:
    /// Receive an event by declaring a method METHOD(object sender, InputState inputState)
    /// Add this method to a certain event handler with the following:
    /// mInputManager.EVENT += METHOD;
    /// EVENT here is to be replaced with the event that you want to receive
    /// METHOD here is the method implemented to receive the event
    ///
    /// How to use (action-based inputs):
    /// Add a new action to ActionType.cs file.
    /// Register this action with RegisterInputEvent(ActionType.ACTION, KEYEVENTS)
    /// Subscribe your method to InputManager.AnyActionEvent
    /// Add the following line at the start of your method:
    /// if (!inputState.IsActionActive(ActionType.ACTION)) return;
    /// </summary>
    internal static class InputManager
    {
        // mouse and keyboard states
        private static MouseState sPreviousMouseState;
        private static MouseState sCurrentMouseState;
        private static KeyboardState sPreviousKeyboardState;
        private static KeyboardState sCurrentKeyboardState;
        private static InputState sCurrentInputState;
        private static Dictionary<ActionType, List<KeyEvent>> sKeyActionMapping;
        private static Dictionary<ActionType, List<MouseEvent>> sMouseActionMapping;
        private static Dictionary<(MouseButton, MouseEventType), ActionType> sReservedMouseEvents;

        private static EventHandlerList sInputEventHandlerList;

        // unique keys for event handlers
        private static readonly object sLeftClickEventKey = new object();
        private static readonly object sRightClickEventKey = new object();
        private static readonly object sAnyActionEventKey = new object();
        private static readonly object sEscPressedEventKey = new object();

        public static bool Initialized { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public static void Initialize()
        {
            sPreviousMouseState = Mouse.GetState();
            sCurrentMouseState = Mouse.GetState();
            sPreviousKeyboardState = Keyboard.GetState();
            sCurrentKeyboardState = Keyboard.GetState();
            sCurrentInputState = new InputState(sCurrentMouseState, sCurrentKeyboardState, sPreviousMouseState, sPreviousKeyboardState, new ActionType[] { });
            sInputEventHandlerList = new EventHandlerList();
            sKeyActionMapping = new Dictionary<ActionType, List<KeyEvent>>();
            sMouseActionMapping = new Dictionary<ActionType, List<MouseEvent>>();
            sReservedMouseEvents = new Dictionary<(MouseButton, MouseEventType), ActionType>();

            foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
            {
                sKeyActionMapping[action] = new List<KeyEvent>();
                sMouseActionMapping[action] = new List<MouseEvent>();
            }

            Initialized = true;
        }

        /// <summary>
        /// Event handler for triggering an action
        /// </summary>
        public static event EventHandler<InputState> AnyActionEvent
        {
            add => sInputEventHandlerList.AddHandler(sAnyActionEventKey, value);
            remove => sInputEventHandlerList.RemoveHandler(sAnyActionEventKey, value);
        }

        /// <summary>
        /// Event handler for pressing ESC
        /// </summary>
        public static event EventHandler<InputState> EscPressed
        {
            add => sInputEventHandlerList.AddHandler(sEscPressedEventKey, value);
            remove => sInputEventHandlerList.RemoveHandler(sEscPressedEventKey, value);
        }

        private static void OnLeftClick(InputState inputState)
        {
            var handler = (EventHandler<InputState>)sInputEventHandlerList[sLeftClickEventKey];
            handler?.Invoke(null, inputState);
        }

        private static void OnRightClick(InputState inputState)
        {
            var handler = (EventHandler<InputState>)sInputEventHandlerList[sRightClickEventKey];
            handler?.Invoke(null, inputState);
        }

        private static void OnAnyActionEvent(InputState inputState)
        {
            var handler = (EventHandler<InputState>)sInputEventHandlerList[sAnyActionEventKey];
            handler?.Invoke(null, inputState);
        }

        private static void OnEscPressed(InputState inputState)
        {
            var handler = (EventHandler<InputState>)sInputEventHandlerList[sEscPressedEventKey];
            handler?.Invoke(null, inputState);
        }

        private static bool KeyPressed(Keys key)
        {
            return sCurrentKeyboardState.IsKeyDown(key) && !sPreviousKeyboardState.IsKeyDown(key);
        }

        private static bool KeyReleased(Keys key)
        {
            return !sCurrentKeyboardState.IsKeyDown(key) && sPreviousKeyboardState.IsKeyDown(key);
        }

        private static ButtonState GetButtonState(MouseButton button, MouseState mouseState)
        {
            switch (button)
            {
                case MouseButton.LeftButton: return mouseState.LeftButton;
                case MouseButton.MiddleButton: return mouseState.MiddleButton;
                case MouseButton.RightButton: return mouseState.RightButton;
                default: return ButtonState.Released;
            }

        }

        private static bool ButtonPressed(MouseButton button)
        {
            return GetButtonState(button, sCurrentMouseState) == ButtonState.Pressed
                   && GetButtonState(button, sPreviousMouseState) != ButtonState.Pressed;
        }

        private static bool ButtonReleased(MouseButton button)
        {
            return GetButtonState(button, sCurrentMouseState) != ButtonState.Pressed
                   && GetButtonState(button, sPreviousMouseState) == ButtonState.Pressed;
        }

        /// <summary>
        /// Returns current input state.
        /// </summary>
        /// <returns>current input state as an InputState object</returns>
        public static InputState GetCurrentInputState() => sCurrentInputState;

        /// <summary>
        /// Register an action to the input manager.
        /// </summary>
        /// <example>
        /// mInputManager.RegisterInputEvent(ActionType.ToggleFullscreen,
        ///     new KeyEvent(Keys.F, KeyEventType.OnButtonPressed) );
        /// </example>
        /// <param name="action">action to be registered</param>
        /// <param name="keyEvents">a list of key events that represent the inputs that trigger the action</param>
        public static void RegisterInputEvent(ActionType action, List<KeyEvent> keyEvents)
        {
            sKeyActionMapping[action] = keyEvents;
        }

        public static void RegisterInputEvent(ActionType action, KeyEvent keyEvent)
        {
            sKeyActionMapping[action] = new List<KeyEvent> { keyEvent };
        }

        public static void RegisterInputEvent(ActionType action, MouseEvent mouseEvent)
        {
            sMouseActionMapping[action] = new List<MouseEvent> { mouseEvent };
        }

        public static void UnregisterAllInputEvents()
        {
            foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
            {
                sKeyActionMapping[action].Clear();
                sMouseActionMapping[action].Clear();
            }
        }

        public static void ReserveInputEvent(ActionType action, MouseEvent mouseEvent)
        {
            if (sReservedMouseEvents.ContainsKey(mouseEvent.ToTuple()) && sReservedMouseEvents[mouseEvent.ToTuple()] != ActionType.EmptyAction) return;
            sReservedMouseEvents[mouseEvent.ToTuple()] = action;
        }

        public static void UnreserveInputEvent(MouseEvent mouseEvent)
        {
            sReservedMouseEvents[mouseEvent.ToTuple()] = ActionType.EmptyAction;
        }

        private static ActionType[] GetCurrentActions()
        {
            var actionTypes = new List<ActionType>();
            foreach (ActionType action in Enum.GetValues(typeof(ActionType)))
            {
                var keyEvents = sKeyActionMapping[action];
                var mouseEvents = sMouseActionMapping[action];
                var actionTriggered = false;
                foreach (var keyEvent in keyEvents)
                {
                    switch (keyEvent.mKeyEventType)
                    {
                        case KeyEventType.OnButtonDown:
                            actionTriggered = sCurrentKeyboardState.IsKeyDown(keyEvent.mKey) || actionTriggered;
                            break;
                        case KeyEventType.OnButtonPressed:
                            actionTriggered = KeyPressed(keyEvent.mKey) || actionTriggered;
                            break;
                        case KeyEventType.OnButtonReleased:
                            actionTriggered = KeyReleased(keyEvent.mKey) || actionTriggered;
                            break;
                    }
                }

                foreach (var mouseEvent in mouseEvents)
                {
                    if (sReservedMouseEvents.ContainsKey(mouseEvent.ToTuple()) &&
                        sReservedMouseEvents[mouseEvent.ToTuple()] != ActionType.EmptyAction &&
                        sReservedMouseEvents[mouseEvent.ToTuple()] != action) continue;
                    switch (mouseEvent.mMouseEventType)
                    {
                        case MouseEventType.OnButtonDown:
                            actionTriggered =
                                GetButtonState(mouseEvent.mButton, sCurrentMouseState) == ButtonState.Pressed
                                    || actionTriggered;
                            break;
                        case MouseEventType.OnButtonPressed:
                            actionTriggered = ButtonPressed(mouseEvent.mButton) || actionTriggered;
                            break;
                        case MouseEventType.OnButtonReleased:
                            actionTriggered = ButtonReleased(mouseEvent.mButton) || actionTriggered;
                            break;
                    }
                }

                if (!actionTriggered) continue;
                actionTypes.Add(action);
            }

            return actionTypes.ToArray();
        }

        /// <summary>
        /// Updates the input manager and triggers certain events
        /// </summary>
        public static void Update(bool active)
        {
            if (!active) return;

            sPreviousMouseState = sCurrentMouseState;
            sCurrentMouseState = Mouse.GetState();
            sPreviousKeyboardState = sCurrentKeyboardState;
            sCurrentKeyboardState = Keyboard.GetState();
            var currentActions = GetCurrentActions();
            sCurrentInputState = new InputState(sCurrentMouseState, sCurrentKeyboardState, sPreviousMouseState, sPreviousKeyboardState, currentActions);
            if (sCurrentMouseState.LeftButton == ButtonState.Pressed
                && sPreviousMouseState.LeftButton != ButtonState.Pressed)
            {
                OnLeftClick(GetCurrentInputState());
            }
            if (sCurrentMouseState.RightButton == ButtonState.Pressed
                && sPreviousMouseState.RightButton != ButtonState.Pressed)
            {
                OnRightClick(GetCurrentInputState());
            }

            if (currentActions.Length > 0)
            {
                OnAnyActionEvent(GetCurrentInputState());
            }

            if (KeyPressed(Keys.Escape))
            {
                OnEscPressed(GetCurrentInputState());
            }
        }
    }
}
