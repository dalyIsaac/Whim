namespace Whim;

internal class InternalContext : IInternalContext
{
	private bool _disposedValue;

	private readonly IContext _context;

	public ICoreNativeManager CoreNativeManager { get; }

	public IWindowMessageMonitor WindowMessageMonitor { get; }

	public IInternalWindowManager WindowManager => (IInternalWindowManager)_context.WindowManager;

	public IKeybindHook KeybindHook { get; }

	public IMouseHook MouseHook { get; }

	public InternalContext(IContext context)
	{
		_context = context;
		CoreNativeManager = new CoreNativeManager(context);
		WindowMessageMonitor = new WindowMessageMonitor(context, this);
		KeybindHook = new KeybindHook(context, this);
		MouseHook = new MouseHook(this);
	}

	public void PreInitialize()
	{
		WindowMessageMonitor.PreInitialize();
	}

	public void PostInitialize()
	{
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
