namespace Whim.Core.Window
{
    public class WindowManager : IWindowManager
    {
        public Commander Commander { get; } = new();

        public event WindowAddDelegate WindowAdded;
        public event WindowUpdateDelegate WindowUpdated;
        public event WindowFocusDelegate WindowFocused;
        public event WindowDestroyDelegate WindowDestroyed;

        public WindowManager()
        {
            // TODO
        }

        ~WindowManager()
        {
            // TODO: unhook events
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
            // TODO: hook and unhook events
        }
    }
}