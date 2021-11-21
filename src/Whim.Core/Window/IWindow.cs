namespace Whim.Core.Window
{
    public delegate void WindowAddDelegate(IWindow window, bool firstCreate);
    public delegate void WindowFocusDelegate(IWindow window);
    public delegate void WindowUpdateDelegate(IWindow window, WindowUpdateType updateType);
    public delegate void WindowDestroyDelegate(IWindow window);

    public interface IWindow
    {
        public string Title { get; }
        public string Class { get; }
        public IWindowLocation Location { get; }
        public int ProcessId { get; }
        public string ProcessFileName { get; }
        public string ProcessName { get; }
        public bool IsFocused { get; }
        public bool IsMinimized { get; }
        public bool IsMaximized { get; }
        public bool IsMouseMoving { get; }
        public IWindowManager WindowManager { get; }

        public event WindowAddDelegate WindowAdded;
        public event WindowUpdateDelegate WindowUpdated;
        public event WindowFocusDelegate WindowFocused;
        public event WindowDestroyDelegate WindowDestroyed;

        public void Focus();
        public void Hide();
        public void ShowNormal();
        public void ShowMaximized();
        public void ShowMinimized();
        public void ShowInCurrentState();

        public void BringToTop();
        public void Close();
    }
}