using System;

namespace Whim;

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
	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	public IKeybindManager KeybindManager { get; }
	public IPluginManager PluginManager { get; }

	/// <summary>
	/// This will be called by Whim after your config has been read.
	/// Do not call it yourself.
	/// </summary>
	public void Initialize();

	/// <summary>
	/// This event is fired when the config context is shutting down.
	/// </summary>
	public event EventHandler<ShutdownEventArgs>? Shutdown;

	/// <summary>
	/// This is called to shutdown the config context.
	/// </summary>
	/// <param name="args">
	/// The shutdown event arguments. If this is not provided, we assume
	/// <see cref="ShutdownReason.User"/>.
	/// </param>
	public void Quit(ShutdownEventArgs? args = null);
}
