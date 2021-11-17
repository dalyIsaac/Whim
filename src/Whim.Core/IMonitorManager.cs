namespace Whim.Core
{
    public interface IMonitorManager : ICommandable
    {
        public int Length { get; }
        public IMonitor FocusedMonitor { get; }
        public IMonitor GetMonitorAtPoint(int x, int y);
    }
}