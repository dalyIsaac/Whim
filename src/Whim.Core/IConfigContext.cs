namespace Whim.Core
{
    public interface IConfigContext
    {
        public IWorkspaceManager WorkspaceManager { get; }
        public IMonitorManager MonitorManager { get; }
    }
}