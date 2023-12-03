namespace Whim;

internal class InternalContext : IInternalContext
{
	private bool _disposedValue;

	private readonly IContext _context;

	public ICoreSavedStateManager CoreSavedStateManager { get; }

	public ICoreNativeManager CoreNativeManager { get; }

	public IWindowMessageMonitor WindowMessageMonitor { get; }

	public IInternalWorkspaceManager WorkspaceManager => (IInternalWorkspaceManager)_context.WorkspaceManager;

	public IInternalMonitorManager MonitorManager => (IInternalMonitorManager)_context.MonitorManager;

	public IInternalWindowManager WindowManager => (IInternalWindowManager)_context.WindowManager;

	public IKeybindHook KeybindHook { get; }

	public IMouseHook MouseHook { get; }

	public IDeferWindowPosManager DeferWindowPosManager { get; }

	public InternalContext(IContext context)
	{
		_context = context;
		CoreSavedStateManager = new CoreSavedStateManager(context);
		CoreNativeManager = new CoreNativeManager(context);
		WindowMessageMonitor = new WindowMessageMonitor(context, this);
		KeybindHook = new KeybindHook(context, this);
		MouseHook = new MouseHook(context, this);
		DeferWindowPosManager = new DeferWindowPosManager(context, this);
	}

	public void PreInitialize()
	{
		WindowMessageMonitor.PreInitialize();
		CoreSavedStateManager.PreInitialize();
	}

	public void PostInitialize()
	{
		CoreSavedStateManager.PostInitialize();
		KeybindHook.PostInitialize();
		MouseHook.PostInitialize();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				CoreSavedStateManager.Dispose();
				WindowMessageMonitor.Dispose();
				KeybindHook.Dispose();
				MouseHook.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		System.GC.SuppressFinalize(this);
	}
}
