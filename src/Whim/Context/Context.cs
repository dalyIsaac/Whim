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
	private readonly InternalContext _internalContext;

	public IFileManager FileManager { get; }
	public IResourceManager ResourceManager { get; }
	public Logger Logger { get; }
	public UncaughtExceptionHandling UncaughtExceptionHandling { get; set; } = UncaughtExceptionHandling.Log;
	public INativeManager NativeManager { get; }

	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	private readonly CommandManager _commandManager;
	public ICommandManager CommandManager => _commandManager;
	public IPluginManager PluginManager { get; }
	public IKeybindManager KeybindManager { get; }
	public INotificationManager NotificationManager { get; }

	public IStore Store { get; }

	public IWindowProcessorManager WindowProcessorManager { get; }

	public event EventHandler<ExitEventArgs>? Exiting;
	public event EventHandler<ExitEventArgs>? Exited;

	/// <summary>
	/// Create a new <see cref="IContext"/>.
	/// </summary>
	public Context()
	{
		string[] args = Environment.GetCommandLineArgs();

		FileManager = new FileManager(args);
		Logger = new Logger();
		ResourceManager = new ResourceManager();
		_internalContext = new InternalContext(this);

		Store = new Store(this, _internalContext);

		NativeManager = new NativeManager(this, _internalContext);

		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager();

		_commandManager = [];
		PluginManager = new PluginManager(this, _commandManager);
		KeybindManager = new KeybindManager(this);
		NotificationManager = new NotificationManager(this);
		WindowProcessorManager = new WindowProcessorManager(this);
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

		DefaultFilteredWindows.LoadWindowsIgnoredByWhim(FilterManager);

		// Initialize before ResourceManager so user dicts take precedence over the default dict.
		ResourceManager.Initialize();

		// Load the user's config.
		ConfigLoader configLoader = new(FileManager);
		DoConfig doConfig = configLoader.LoadConfig();
		doConfig(this);

		// Initialize the logger first.
		Logger.Initialize(FileManager);

		// Initialize the managers.
		Logger.Debug("Initializing...");
		_internalContext.PreInitialize();
		PluginManager.PreInitialize();

		NotificationManager.Initialize();
		Store.Initialize();

		PluginManager.PostInitialize();
		_internalContext.PostInitialize();

		Logger.Debug("Completed initialization");
	}

	public Result<IWindow> CreateWindow(HWND hwnd) => Window.GetOrCreateWindow(this, _internalContext, hwnd);

	public void HandleUncaughtException(string procName, Exception exception)
	{
		Logger.Fatal($"Unhandled exception in {procName}: {exception}");

		switch (UncaughtExceptionHandling)
		{
			case UncaughtExceptionHandling.Shutdown:
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
		args ??= new ExitEventArgs { Reason = ExitReason.User };

		Exiting?.Invoke(this, args);

		PluginManager.Dispose();
		NotificationManager.Dispose();
		Store.Dispose();
		_internalContext.Dispose();

		Logger.Debug("Mostly exited...");

		Logger.Dispose();
		Exited?.Invoke(this, args);
	}
}
