using System.Collections.Generic;

namespace Whim.Core.Monitor
{
    public interface IMonitorManager : IEnumerable<IMonitor>, ICommandable
    {
        public int Length { get; }
        public IMonitor FocusedMonitor { get; }
        public IMonitor GetMonitorAtPoint(int x, int y);
    }
}
