using Whim.Core.Commands;

namespace Whim.Core
{
    public interface IManager : ICommandable
    {
        public IWorkspaceManager WorkspaceManager { get; }
        public IMonitorManager MonitorManager { get;  }
    }
}