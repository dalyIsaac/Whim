namespace Whim.Core.ConfigContext;

/// <summary>
/// This is the core of Whim. <br/>
///
/// <c>IConfigContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>IConfigContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>
/// </summary>
public interface IConfigContext
{
	public Logger Logger { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
}
