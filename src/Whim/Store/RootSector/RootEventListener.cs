namespace Whim;

/// <summary>
/// Listens for events which aren't specific to slices.
/// </summary>
internal class RootEventListener : IDisposable
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;
	private bool _disposedValue;

	public RootEventListener(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
	}

	public void Initialize()
	{
		_internalCtx.MouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
		_internalCtx.MouseHook.MouseLeftButtonDown += MouseHook_MouseLeftButtonDown;
	}

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e) =>
		_ctx.Store.Dispatch(new MouseLeftButtonUpTransform(e.Point));

	private void MouseHook_MouseLeftButtonDown(object? sender, MouseEventArgs e) =>
		_ctx.Store.Dispatch(new MouseLeftButtonDownTransform());

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_internalCtx.MouseHook.MouseLeftButtonUp -= MouseHook_MouseLeftButtonUp;
				_internalCtx.MouseHook.MouseLeftButtonDown -= MouseHook_MouseLeftButtonDown;
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
		GC.SuppressFinalize(this);
	}
}
