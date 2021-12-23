namespace Whim.Core;

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
	private bool disposedValue;

	public Logger Logger { get; }
	public IWorkspaceManager WorkspaceManager { get; }
	public IWindowManager WindowManager { get; }
	public IMonitorManager MonitorManager { get; }
	public IRouterManager RouterManager { get; }
	public IFilterManager FilterManager { get; }
	public IKeybindManager KeybindManager { get; }
	public IPluginManager PluginManager { get; }

	public ConfigContext()
	{
		Logger = Logger.Initialize();
		RouterManager = new RouterManager(this);
		FilterManager = new FilterManager(this);
		WindowManager = new WindowManager(this);
		MonitorManager = new MonitorManager(this);
		WorkspaceManager = new WorkspaceManager(this);
		KeybindManager = new KeybindManager(this);
		PluginManager = new PluginManager(this);
	}

	public void Initialize()
	{
		Logger.Debug("Initializing config context...");
		WindowManager.Initialize();
		WorkspaceManager.Initialize();
		KeybindManager.Initialize();
		PluginManager.Initialize();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				WindowManager.Dispose();
				KeybindManager.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}
}
