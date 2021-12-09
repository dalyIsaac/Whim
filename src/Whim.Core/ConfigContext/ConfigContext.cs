namespace Whim.Core.ConfigContext;

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
	public IKeybindManager KeybindManager { get; }

	public ConfigContext()
	{
		Logger = Logger.Initialize();
		WorkspaceManager = new WorkspaceManager();
		WindowManager = new WindowManager();
		MonitorManager = new MonitorManager();
		RouterManager = new RouterManager(WorkspaceManager);
		KeybindManager = new KeybindManager();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				WindowManager.Dispose();
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
