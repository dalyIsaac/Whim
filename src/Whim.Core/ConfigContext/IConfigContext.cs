using System;

namespace Whim.Core;

/// <summary>
/// This is the core of Whim. <br/>
///
/// <c>IConfigContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>IConfigContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>
/// </summary>
public interface IConfigContext : IDisposable
{
	public Logger Logger { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
	public IRouterManager RouterManager { get; }
	public IKeybindManager KeybindManager { get; }
	public void Initialize();
}
