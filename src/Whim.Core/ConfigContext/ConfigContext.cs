namespace Whim.Core.ConfigContext;

/// <summary>
/// Implementation of <see cref="IConfigContext"/>. This is the core of Whim. <br/>
///
/// <c>ConfigContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>ConfigContext</c> also contains other associated state and functionality, like the
/// <see cref="WhimPath"/> and the <see cref="Logger"/>
/// </summary>
public class ConfigContext : IConfigContext
{
	/// <summary>
	/// The path where the Whim config and plugins are stored.
	/// </summary>
	public string WhimPath { get; }

	public Logger Logger { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }

	public ConfigContext()
	{
		WhimPath = Files.FileHelper.GetUserWhimPath();
		Logger = Logger.Initialize(WhimPath);
		WorkspaceManager = new WorkspaceManager();
		WindowManager = new WindowManager();
		MonitorManager = new MonitorManager();
	}
}
