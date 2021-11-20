using System.Collections.Generic;
using Whim.Core.Commands;

namespace Whim.Core
{
    public interface IMonitorManager : IEnumerable<IMonitor>, ICommandable
    {
        public int Length { get; }
        public IMonitor FocusedMonitor { get; }
        public IMonitor GetMonitorAtPoint(int x, int y);
    }
}
