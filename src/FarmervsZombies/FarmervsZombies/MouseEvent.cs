namespace FarmervsZombies
{
    internal enum MouseEventType
    {
        OnButtonDown,
        OnButtonPressed,
        OnButtonReleased
    }

    internal enum MouseButton
    {
        LeftButton,
        RightButton,
        MiddleButton
    }

    internal sealed class MouseEvent
    {
        public readonly MouseButton mButton;
        public readonly MouseEventType mMouseEventType;

        public MouseEvent(MouseButton button, MouseEventType mouseEventType)
        {
            mButton = button;
            mMouseEventType = mouseEventType;
        }

        public (MouseButton, MouseEventType) ToTuple()
        {
            return (mButton, mMouseEventType);
        }
    }
}
