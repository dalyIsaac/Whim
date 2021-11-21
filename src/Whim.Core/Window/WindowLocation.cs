namespace Whim.Core.Window
{
    public class WindowLocation : IWindowLocation
    {
        public int X { get; }

        public int Y { get; }

        public int Width { get; }

        public int Height { get; }

        public WindowState WindowState { get; }


        public WindowLocation(int x, int y, int width, int height, WindowState windowState)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            WindowState = windowState;
        }

        public bool IsPointInside(int x, int y)
        {
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }
    }
}