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
	internal IFileManager FileManager { get; }
	public Logger Logger { get; }
	public INativeManager NativeManager { get; }
	internal ICoreNativeManager CoreNativeManager { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	private readonly CommandManager _commandManager;
	public ICommandManager CommandManager => _commandManager;
	public IPluginManager PluginManager { get; }
	public IKeybindManager KeybindManager { get; }
	internal KeybindHook KeybindHook { get; }
	internal MouseHook MouseHook { get; }

	public event EventHandler<ExitEventArgs>? Exiting;
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// Create a new <see cref="IContext"/>.
	/// </summary>
	public Context()
	{
		FileManager = new FileManager();
		Logger = new Logger();

		CoreNativeManager = new CoreNativeManager(this);
		NativeManager = new NativeManager(this, CoreNativeManager);

		KeybindHook = new KeybindHook(this, CoreNativeManager);
		MouseHook = new MouseHook(coreNativeManager: CoreNativeManager);

		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager();
		WindowManager = new WindowManager(this, CoreNativeManager, MouseHook);
		MonitorManager = new MonitorManager(this, CoreNativeManager);
		WorkspaceManager = new WorkspaceManager(this);
		_commandManager = new CommandManager();
		PluginManager = new PluginManager(this, FileManager, _commandManager);
		KeybindManager = new KeybindManager(this);
	}

	public void Initialize()
	{
		// Load the core commands
		CoreCommands coreCommands = new(this);

		foreach (ICommand command in coreCommands.Commands)
		{
			_commandManager.AddPluginCommand(command);
		}

		foreach ((string name, IKeybind keybind) in coreCommands.Keybinds)
		{
			KeybindManager.Add(name, keybind);
		}

		// Load the user's config.
		ConfigLoader configLoader = new(FileManager);
		DoConfig doConfig = configLoader.LoadConfig();
		doConfig(this);

		// Initialize the managers.
		Logger.Initialize(FileManager);

		Logger.Debug("Initializing...");
		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();

		WindowManager.PostInitialize();
		PluginManager.PostInitialize();
		KeybindHook.PostInitialize();
		MouseHook.PostInitialize();

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
		MouseHook.Dispose();

		Logger.Debug("Mostly exited...");

		Logger.Dispose();
		Exited?.Invoke(this, args);
	}
}
