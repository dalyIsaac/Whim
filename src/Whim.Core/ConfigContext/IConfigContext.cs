namespace Whim.Core.ConfigContext
{
    public interface IConfigContext
    {
        public IWorkspaceManager WorkspaceManager { get; }
        public IMonitorManager MonitorManager { get; }
    }
}