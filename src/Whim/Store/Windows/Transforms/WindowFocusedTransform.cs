using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowFocusedTransform(IWindow? Window) : Transform()
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx)
	{
		SetActiveMonitor(ctx, internalCtx);
	}

	/// <summary>
	/// Set the active monitor.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	private void SetActiveMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// If we know the window, use what the Butler knows instead of Windows.
		if (Window is not null)
		{
			Logger.Debug($"Focusing window {Window}");

			if (ctx.Butler.Pantry.GetMonitorForWindow(Window) is IMonitor monitor)
			{
				Logger.Debug($"Setting active monitor to {monitor}");
				int idx = ctx.Store.MonitorSlice.Monitors.IndexOf(monitor);
				ctx.Store.MonitorSlice.ActiveMonitorIndex = idx;
				ctx.Store.MonitorSlice.LastWhimActiveMonitorIndex = idx;

				return;
			}
		}

		// We don't know the window, so get the foreground window.
		HWND hwnd = Window?.Handle ?? internalCtx.CoreNativeManager.GetForegroundWindow();
		Logger.Debug($"Focusing hwnd {hwnd}");

		if (hwnd.IsNull)
		{
			Logger.Debug($"Hwnd is desktop window, ignoring");
			return;
		}

		HMONITOR hMONITOR = internalCtx.CoreNativeManager.MonitorFromWindow(
			hwnd,
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		for (int idx = 0; idx < ctx.Store.MonitorSlice.Monitors.Length; idx++)
		{
			IMonitor monitor = ctx.Store.MonitorSlice.Monitors[idx];

			if (!monitor.Handle.Equals(hMONITOR))
			{
				continue;
			}

			Logger.Debug($"Setting active monitor to {monitor}");
			ctx.Store.MonitorSlice.ActiveMonitorIndex = idx;

			if (Window is not null)
			{
				ctx.Store.MonitorSlice.LastWhimActiveMonitorIndex = idx;
			}

			break;
		}
	}
}
