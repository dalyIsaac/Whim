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
	private readonly IInternalContext _internalContext;
	public IFileManager FileManager { get; }
	public Logger Logger { get; }
	public UncaughtExceptionHandling UncaughtExceptionHandling { get; set; } = UncaughtExceptionHandling.Log;
	public INativeManager NativeManager { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	private readonly CommandManager _commandManager;
	public ICommandManager CommandManager => _commandManager;
	public IPluginManager PluginManager { get; }
	public IKeybindManager KeybindManager { get; }

	public event EventHandler<ExitEventArgs>? Exiting;
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// Create a new <see cref="IContext"/>.
	/// </summary>
	public Context()
	{
		FileManager = new FileManager();
		Logger = new Logger();
		_internalContext = new InternalContext(this);

		NativeManager = new NativeManager(this, _internalContext);

		// Initialize just the logger.
		Logger.Initialize(FileManager);

		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager();
		WindowManager = new WindowManager(this, _internalContext);
		MonitorManager = new MonitorManager(_internalContext);
		WorkspaceManager = new WorkspaceManager(this, _internalContext);
		_commandManager = new CommandManager();
		PluginManager = new PluginManager(this, _commandManager);
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
			KeybindManager.SetKeybind(name, keybind);
		}

		FilteredWindows.LoadWindowsIgnoredByWhim(FilterManager);

		// Load the user's config.
		ConfigLoader configLoader = new(FileManager);
		DoConfig doConfig = configLoader.LoadConfig();
		doConfig(this);

		// Initialize the managers.
		Logger.Debug("Initializing...");
		_internalContext.PreInitialize();
		PluginManager.PreInitialize();

		MonitorManager.Initialize();
		WindowManager.Initialize();
		WorkspaceManager.Initialize();

		WindowManager.PostInitialize();
		PluginManager.PostInitialize();
		_internalContext.PostInitialize();

		Logger.Debug("Completed initialization");
	}

	public void HandleUncaughtException(string procName, Exception exception)
	{
		Logger.Fatal($"Unhandled exception in {procName}: {exception}");

		switch (UncaughtExceptionHandling)
		{
			case UncaughtExceptionHandling.Notify:
				// TODO
				throw new NotImplementedException();

			case UncaughtExceptionHandling.ShutdownSilently:
				Exit(new ExitEventArgs() { Reason = ExitReason.Error, Message = exception.ToString() });
				break;

			case UncaughtExceptionHandling.ShutdownWithMessage:
				Exit(new ExitEventArgs() { Reason = ExitReason.Error, Message = exception.ToString() });
				break;

			case UncaughtExceptionHandling.Log:
			default:
				break;
		}
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
		_internalContext.Dispose();

		Logger.Debug("Mostly exited...");

		Logger.Dispose();
		Exited?.Invoke(this, args);
	}
}
