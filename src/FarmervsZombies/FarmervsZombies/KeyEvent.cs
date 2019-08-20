using Microsoft.Xna.Framework.Input;

namespace FarmervsZombies
{
    internal enum KeyEventType
    {
        OnButtonDown,
        OnButtonPressed,
        OnButtonReleased
    }

    /// <summary>
    /// Data Structure that represents a specific input (e.g. pressing the key 'f')
    /// </summary>
    internal sealed class KeyEvent
    {
        public readonly Keys mKey;
        public readonly KeyEventType mKeyEventType;

        public KeyEvent(Keys key, KeyEventType keyEventType)
        {
            mKey = key;
            mKeyEventType = keyEventType;
        }
    }
}
