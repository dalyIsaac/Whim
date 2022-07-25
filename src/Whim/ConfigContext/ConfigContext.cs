using System;
using System.Reflection;

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
	public Logger Logger { get; private set; }
	public IWorkspaceManager WorkspaceManager { get; private set; }
	public IWindowManager WindowManager { get; private set; }
	public IMonitorManager MonitorManager { get; private set; }
	public IRouterManager RouterManager { get; private set; }
	public IFilterManager FilterManager { get; private set; }
	public ICommandManager CommandManager { get; private set; }
	public IPluginManager PluginManager { get; private set; }

	public event EventHandler<ExitEventArgs>? Exiting;
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// Create a new <see cref="IConfigContext"/>.
	/// </summary>
	public ConfigContext()
	{
		Logger = new Logger();
		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager();
		WindowManager = new WindowManager(this);
		MonitorManager = new MonitorManager(this);
		WorkspaceManager = new WorkspaceManager(this);
		CommandManager = new CommandManager();
		PluginManager = new PluginManager();
	}

	public void Initialize()
	{
		// Load the config context.
		DoConfig doConfig = ConfigLoader.LoadConfigContext();
		doConfig(this);

		// Initialize the managers.
		Logger.Initialize();

		Logger.Debug("Initializing...");
		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();
		CommandManager.Initialize();

		WindowManager.PostInitialize();
		PluginManager.PostInitialize();

		Logger.Debug("Completed initialization");
	}

	public void Exit(ExitEventArgs? args = null)
	{
		Logger.Debug("Exiting config context...");
		args ??= new ExitEventArgs(ExitReason.User);

		Exiting?.Invoke(this, args);

		PluginManager.Dispose();
		CommandManager.Dispose();
		WorkspaceManager.Dispose();
		WindowManager.Dispose();
		MonitorManager.Dispose();

		Logger.Debug("Mostly exited...");

		Logger.Dispose();
		Exited?.Invoke(this, args);
	}
}
