using System.Threading.Tasks;

namespace Whim;

internal class InternalContext : IInternalContext
{
	private bool _disposedValue;

	private readonly IContext _context;

	public ParallelOptions ParallelOptions { get; } = new();

	public ICoreSavedStateManager CoreSavedStateManager { get; }

	public ICoreNativeManager CoreNativeManager { get; }

	public IWindowMessageMonitor WindowMessageMonitor { get; }

	public IInternalButler Butler => (IInternalButler)_context.Butler;

	public IInternalMonitorManager MonitorManager => (IInternalMonitorManager)_context.MonitorManager;

	public IInternalWindowManager WindowManager => (IInternalWindowManager)_context.WindowManager;

	public IKeybindHook KeybindHook { get; }

	public IMouseHook MouseHook { get; }

	public IDeferWorkspacePosManager DeferWorkspacePosManager { get; }

	public IButlerEventHandlers ButlerEventHandlers => ((Butler)_context.Butler).EventHandlers;

	public ISubscriber NotificationManager => (ISubscriber)_context.NotificationManager;

	public InternalContext(IContext context)
	{
		_context = context;
		CoreSavedStateManager = new CoreSavedStateManager(context);
		CoreNativeManager = new CoreNativeManager(context);
		WindowMessageMonitor = new WindowMessageMonitor(context, this);
		KeybindHook = new KeybindHook(context, this);
		MouseHook = new MouseHook(context, this);
		DeferWorkspacePosManager = new DeferWorkspacePosManager(context, this);
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

		_context.NativeManager.InvokeOnUIThread(Subscribe);
	}

	private void Subscribe()
	{
		MonitorManager.Subscribe();
		NotificationManager.Subscribe();
		WindowManager.Subscribe();
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
