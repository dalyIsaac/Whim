namespace Whim.Core
{
    public class ConfigContext : IConfigContext
    {
        public IWorkspaceManager WorkspaceManager { get; }
        public IMonitorManager MonitorManager { get; }

        public ConfigContext()
        {
            WorkspaceManager = new WorkspaceManager();
            MonitorManager = new MonitorManager();
        }

        public ConfigContext(IWorkspaceManager workspaceManager)
        {
            WorkspaceManager = workspaceManager;
            MonitorManager = new MonitorManager();
        }

        public ConfigContext(IWorkspaceManager workspaceManager, IMonitorManager monitorManager)
        {
            WorkspaceManager = workspaceManager;
            MonitorManager = monitorManager;
        }
    }
}