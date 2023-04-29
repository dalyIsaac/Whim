using System;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IContext"/>. This is the core of Whim. <br/>
///
/// <c>Context</c> consists of managers which contain and control Whim's state, and thus
/// functionality. <br/>
///
/// <c>Context</c> also contains other associated state and functionality, like the
/// <see cref="Logger"/>.
/// </summary>
internal class Context : IContext
{
	public Logger Logger { get; }
	public INativeManager NativeManager { get; }
	internal ICoreNativeManager CoreNativeManager { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	public ICommandManager CommandManager { get; }
	public IPluginManager PluginManager { get; }
	public IKeybindManager KeybindManager { get; }
	internal KeybindHook KeybindHook { get; }

	public event EventHandler<ExitEventArgs>? Exiting;
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// Create a new <see cref="IContext"/>.
	/// </summary>
	public Context()
	{
		Logger = new Logger();
		NativeManager = new NativeManager(this);
		CoreNativeManager = new CoreNativeManager(this);
		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager();
		WindowManager = new WindowManager(this, CoreNativeManager);
		MonitorManager = new MonitorManager(this, CoreNativeManager);
		WorkspaceManager = new WorkspaceManager(this);
		CommandManager = new CommandManager();
		PluginManager = new PluginManager(this);
		KeybindManager = new KeybindManager(this);
		KeybindHook = new KeybindHook(this);
	}

	public void Initialize()
	{
		// Load the core commands
		CoreCommands coreCommands = new(this);

		foreach (ICommand command in coreCommands.Commands)
		{
			CommandManager.Add(command);
		}

		foreach ((string name, IKeybind keybind) in coreCommands.Keybinds)
		{
			KeybindManager.AddKeybind(name, keybind);
		}

		// Load the context.
		DoConfig doConfig = ConfigLoader.LoadContext();
		doConfig(this);

		// Initialize the managers.
		Logger.Initialize();

		Logger.Debug("Initializing...");
		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();

		WindowManager.PostInitialize();
		PluginManager.PostInitialize();
		KeybindHook.PostInitialize();

		Logger.Debug("Completed initialization");
	}

	public void Exit(ExitEventArgs? args = null)
	{
		Logger.Debug("Exiting context...");
		args ??= new ExitEventArgs() { Reason = ExitReason.User };

		Exiting?.Invoke(this, args);

		PluginManager.Dispose();
		WorkspaceManager.Dispose();
		WindowManager.Dispose();
		MonitorManager.Dispose();
		KeybindHook.Dispose();

		Logger.Debug("Mostly exited...");

		Logger.Dispose();
		Exited?.Invoke(this, args);
	}
}
