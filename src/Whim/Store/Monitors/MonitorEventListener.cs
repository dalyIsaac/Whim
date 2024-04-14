using System.Threading.Tasks;

namespace Whim;

// TODO: Initialize
internal class MonitorEventListener
{
	private readonly IContext _ctx;
	private readonly IInternalContext _internalCtx;

	public MonitorEventListener(IContext ctx, IInternalContext internalCtx)
	{
		_ctx = ctx;
		_internalCtx = internalCtx;
	}

	public void Initialize()
	{
		_internalCtx.WindowMessageMonitor.DisplayChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.WorkAreaChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.DpiChanged += WindowMessageMonitor_MonitorsChanged;
		_internalCtx.WindowMessageMonitor.SessionChanged += WindowMessageMonitor_SessionChanged;
		_internalCtx.MouseHook.MouseLeftButtonUp += MouseHook_MouseLeftButtonUp;
	}

	private void WindowMessageMonitor_MonitorsChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		_ctx.Store.Dispatch(new MonitorsChangedTransform());
	}

	private void MouseHook_MouseLeftButtonUp(object? sender, MouseEventArgs e)
	{
		_ctx.Store.Dispatch(new MouseLeftButtonUpTransform(e.Point));
	}

	private void WindowMessageMonitor_SessionChanged(object? sender, WindowMessageMonitorEventArgs e)
	{
		// If we update monitors too quickly, the reported working area can sometimes be the
		// monitor's bounds, which is incorrect. So, we wait a bit before updating the monitors.
		// This gives Windows some to figure out the correct working area.
		_ctx.NativeManager.TryEnqueue(async () =>
		{
			await Task.Delay(5000).ConfigureAwait(true);
			WindowMessageMonitor_MonitorsChanged(sender, e);
		});
	}
}
