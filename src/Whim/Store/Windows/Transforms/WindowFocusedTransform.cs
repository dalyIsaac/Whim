using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowFocusedTransform(IWindow? Window) : Transform()
{
	internal override void Execute(IContext ctx, IInternalContext internalCtx, RootSlice root)
	{
		// If we know the window, use what the Butler knows instead of Windows.
		if (Window is not null)
		{
			Logger.Debug($"Focusing window {Window}");

			if (ctx.Butler.Pantry.GetMonitorForWindow(Window) is IMonitor monitor)
			{
				Logger.Debug($"Setting active monitor to {monitor}");
				int idx = root.MonitorSlice.Monitors.IndexOf(monitor);
				root.MonitorSlice.ActiveMonitorIndex = idx;
				root.MonitorSlice.LastWhimActiveMonitorIndex = idx;

				return;
			}
		}

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

		for (int idx = 0; idx < root.MonitorSlice.Monitors.Length; idx++)
		{
			IMonitor monitor = root.MonitorSlice.Monitors[idx];

			if (!monitor.Handle.Equals(hMONITOR))
			{
				continue;
			}

			Logger.Debug($"Setting active monitor to {monitor}");
			root.MonitorSlice.ActiveMonitorIndex = idx;

			if (Window is not null)
			{
				root.MonitorSlice.LastWhimActiveMonitorIndex = idx;
			}

			break;
		}
	}
}
