namespace Whim.Core.Window
{
    public interface IWindowManager : ICommandable
    {
        public void Initialize();

        public event WindowAddDelegate WindowAdded;
        public event WindowUpdateDelegate WindowUpdated;
        public event WindowFocusDelegate WindowFocused;
        public event WindowDestroyDelegate WindowDestroyed;
    }
}