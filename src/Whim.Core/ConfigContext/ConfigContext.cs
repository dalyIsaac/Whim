namespace Whim.Core.ConfigContext
{
    public class ConfigContext : IConfigContext
    {
        public IWorkspaceManager WorkspaceManager { get; }
        public IWindowManager WindowManager { get; }
        public IMonitorManager MonitorManager { get; }

        public ConfigContext()
        {
            WorkspaceManager = new WorkspaceManager();
            WindowManager = new WindowManager();
            MonitorManager = new MonitorManager();
        }
    }
}