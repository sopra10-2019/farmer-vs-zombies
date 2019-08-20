using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies
{
    internal sealed class InputState : EventArgs
    {
        // input information, made public to read information directly
        public readonly Vector2 mMouseWindowPosition;
        public readonly Vector2 mMouseWorldPosition;
        public readonly int mMouseScrollWheelValue;
        private readonly Keys[] mPressedKeys;
        private readonly Keys[] mCurrentlyPressedKeys;
        private readonly MouseButton[] mPressedButtons;
        private readonly ActionType[] mActionTypes;
        private readonly MouseButton[] mCurrentlyPressedButtons;

        public InputState(MouseState mouseState, KeyboardState keyboardState, MouseState previousMouseState, KeyboardState previousKeyboardState, ActionType[] actionTypes)
        {
            mMouseWindowPosition = new Vector2(mouseState.X, mouseState.Y);
            mMouseWorldPosition = Vector2.Transform(mMouseWindowPosition, Matrix.Invert(Camera.Transform)) / 32;
            mPressedKeys = keyboardState.GetPressedKeys();
            mActionTypes = actionTypes;
            mMouseScrollWheelValue = mouseState.ScrollWheelValue;

            // Get currently pressed keys
            var previousPressedKeys = previousKeyboardState.GetPressedKeys();
            var currentlyPressedKeys = new List<Keys>();
            foreach (var key in mPressedKeys)
            {
                if (!previousPressedKeys.Contains(key)) currentlyPressedKeys.Add(key);
            }

            mCurrentlyPressedKeys = currentlyPressedKeys.ToArray();
            var pressedButtons = new List<MouseButton>();
            if (mouseState.LeftButton == ButtonState.Pressed) pressedButtons.Add(MouseButton.LeftButton);
            if (mouseState.MiddleButton == ButtonState.Pressed) pressedButtons.Add(MouseButton.MiddleButton);
            if (mouseState.RightButton == ButtonState.Pressed) pressedButtons.Add(MouseButton.RightButton);
            mPressedButtons = pressedButtons.ToArray();
            var currentlyPressedButtons = new List<MouseButton>();
            if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                currentlyPressedButtons.Add(MouseButton.LeftButton);
            if (mouseState.MiddleButton == ButtonState.Pressed &&
                previousMouseState.MiddleButton == ButtonState.Released)
                currentlyPressedButtons.Add(MouseButton.MiddleButton);
            if (mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                currentlyPressedButtons.Add(MouseButton.RightButton);
            mCurrentlyPressedButtons = currentlyPressedButtons.ToArray();
        }

        public bool IsKeyPressed(Keys key)
        {
            return mPressedKeys.Contains(key);
        }

        public bool IsButtonPressed(MouseButton button)
        {
            return mPressedButtons.Contains(button);
        }

        public bool IsButtonCurrentlyPressed(MouseButton button)
        {
            return mCurrentlyPressedButtons.Contains(button);
        }

        public Keys[] GetCurrentlyPressedKeys()
        {
            return mCurrentlyPressedKeys;
        }

        public bool IsActionActive(ActionType action)
        {
            return mActionTypes.Contains(action);
        }
    }
}