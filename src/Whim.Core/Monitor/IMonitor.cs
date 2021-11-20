namespace Whim.Core.Monitor
{
    public interface IMonitor
    {
        public string Name { get; }
        public int Width { get; }
        public int Height { get; }
        public int X { get; }
        public int Y { get; }
    }
}