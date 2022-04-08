using System;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IConfigContext"/>. This is the core of Whim. <br/>
///
/// <c>ConfigContext</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>ConfigContext</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>.
/// </summary>
public class ConfigContext : IConfigContext
{
	public Logger Logger { get; set; }
	public IWorkspaceManager WorkspaceManager { get; set; }
	public IWindowManager WindowManager { get; set; }
	public IMonitorManager MonitorManager { get; set; }
	public IRouterManager RouterManager { get; set; }
	public IFilterManager FilterManager { get; set; }
	public IKeybindManager KeybindManager { get; set; }
	public IPluginManager PluginManager { get; set; }

	public event EventHandler<ShutdownEventArgs>? Shutdown;

	public ConfigContext(
		Logger? logger = null,
		IRouterManager? routerManager = null,
		IFilterManager? filterManager = null,
		IWindowManager? windowManager = null,
		IMonitorManager? monitorManager = null,
		IWorkspaceManager? workspaceManager = null,
		IKeybindManager? keybindManager = null,
		IPluginManager? pluginManager = null)
	{
		Logger = logger ?? new Logger();
		RouterManager = routerManager ?? new RouterManager(this);
		FilterManager = filterManager ?? new FilterManager(this);
		WindowManager = windowManager ?? new WindowManager(this);
		MonitorManager = monitorManager ?? new MonitorManager(this);
		WorkspaceManager = workspaceManager ?? new WorkspaceManager(this);
		KeybindManager = keybindManager ?? new KeybindManager(this);
		PluginManager = pluginManager ?? new PluginManager(this);
	}

	public void Initialize()
	{
		Logger.Initialize();

		Logger.Debug("Initializing config context...");

		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();
		KeybindManager.Initialize();

		PluginManager.PostInitialize();
	}


	public void Quit(ShutdownEventArgs? args = null)
	{
		Logger.Debug("Disposing config context...");
		WindowManager.Dispose();
		MonitorManager.Dispose();
		KeybindManager.Dispose();
		PluginManager.Dispose();

		Shutdown?.Invoke(this, args ?? new ShutdownEventArgs(ShutdownReason.User));
	}
}
