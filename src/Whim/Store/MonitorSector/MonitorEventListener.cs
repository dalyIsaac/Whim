using System.Threading.Tasks;

namespace Whim;

internal class MonitorEventListener(IContext ctx, IInternalContext internalCtx, int delayMs = 5000) : IDisposable
{
	private readonly IContext _ctx = ctx;
	private readonly IInternalContext _internalCtx = internalCtx;
	private readonly int _delayMs = delayMs;
	private bool _disposedValue;

	public void Initialize()
	{
		Logger.Information("Initializing MonitorEventListener");
		_internalCtx.WindowMessageMonitor.DisplayChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.WorkAreaChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.DpiChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.SessionChanged += WindowMessageMonitor_SessionChanged;
	}

	private void WindowMessageMonitor_MonitorsChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		_ctx.Store.WhimDispatch(new MonitorsChangedTransform());
	}

	private void WindowMessageMonitor_SessionChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		// If we update monitors too quickly, the reported working area can sometimes be the
		// monitor's bounds, which is incorrect. So, we wait a bit before updating the monitors.
		// This gives Windows some to figure out the correct working area.
		_ctx.NativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(_delayMs).ConfigureAwait(true);
			WindowMessageMonitor_MonitorsChanged(sender, e);
		});
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_internalCtx.WindowMessageMonitor.DisplayChanged -= WindowMessageMonitor_MonitorsChanged;
				_internalCtx.WindowMessageMonitor.WorkAreaChanged -= WindowMessageMonitor_MonitorsChanged;
				_internalCtx.WindowMessageMonitor.DpiChanged -= WindowMessageMonitor_MonitorsChanged;
				_internalCtx.WindowMessageMonitor.SessionChanged -= WindowMessageMonitor_SessionChanged;
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
