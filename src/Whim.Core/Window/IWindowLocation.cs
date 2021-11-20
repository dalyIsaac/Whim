namespace Whim.Core.Window
{
    public interface IWindowLocation
    {
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }
        public WindowState WindowState { get; }
        public IWindow Window { get; }
    }
}