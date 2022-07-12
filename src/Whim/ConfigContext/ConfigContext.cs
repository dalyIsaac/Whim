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
internal class ConfigContext : IConfigContext
{
	public Logger Logger { get; set; }
	public IWorkspaceManager WorkspaceManager { get; set; }
	public IWindowManager WindowManager { get; set; }
	public IMonitorManager MonitorManager { get; set; }
	public IRouterManager RouterManager { get; set; }
	public IFilterManager FilterManager { get; set; }
	public ICommandManager CommandManager { get; set; }
	public IPluginManager PluginManager { get; set; }

	public event EventHandler<QuitEventArgs>? Quitting;

	public ConfigContext(
		Logger? logger = null,
		IRouterManager? routerManager = null,
		IFilterManager? filterManager = null,
		IWindowManager? windowManager = null,
		IMonitorManager? monitorManager = null,
		IWorkspaceManager? workspaceManager = null,
		ICommandManager? commandManager = null,
		IPluginManager? pluginManager = null)
	{
		Logger = logger ?? new Logger();
		RouterManager = routerManager ?? new RouterManager(this);
		FilterManager = filterManager ?? new FilterManager();
		WindowManager = windowManager ?? new WindowManager(this);
		MonitorManager = monitorManager ?? new MonitorManager(this);
		WorkspaceManager = workspaceManager ?? new WorkspaceManager(this);
		CommandManager = commandManager ?? new CommandManager();
		PluginManager = pluginManager ?? new PluginManager();
	}

	public void Initialize()
	{
		Logger.Initialize();

		Logger.Debug("Initializing config context...");

		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();
		CommandManager.Initialize();

		WindowManager.PostInitialize();
		PluginManager.PostInitialize();
	}


	public void Quit(QuitEventArgs? args = null)
	{
		Logger.Debug("Disposing config context...");
		Quitting?.Invoke(this, args ?? new QuitEventArgs(QuitReason.User));

		WindowManager.Dispose();
		MonitorManager.Dispose();
		CommandManager.Dispose();
		PluginManager.Dispose();
	}
}
