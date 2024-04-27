using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowFocusedTransform(IWindow? Window) : Transform()
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		SetActiveMonitor(ctx, internalCtx, mutableRootSector);

		// TODO: Test
		WindowFocusedEventArgs args = new() { Window = Window };
		internalCtx.ButlerEventHandlers.OnWindowFocused(args);
		mutableRootSector.Windows.QueueEvent(args);

		return Empty.Result;
	}

	/// <summary>
	/// Set the active monitor.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="mutableRootSector"></param>
	private void SetActiveMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector mutableRootSector)
	{
		MonitorSector sector = mutableRootSector.Monitors;

		// If we know the window, use what the Butler knows instead of Windows.
		if (Window is not null)
		{
			Logger.Debug($"Focusing window {Window}");

			if (ctx.Butler.Pantry.GetMonitorForWindow(Window) is IMonitor monitor)
			{
				Logger.Debug($"Setting active monitor to {monitor}");
				int idx = sector.Monitors.IndexOf(monitor);
				sector.ActiveMonitorIndex = idx;
				sector.LastWhimActiveMonitorIndex = idx;

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

		for (int idx = 0; idx < sector.Monitors.Length; idx++)
		{
			IMonitor monitor = sector.Monitors[idx];

			if (!monitor.Handle.Equals(hMONITOR))
			{
				continue;
			}

			Logger.Debug($"Setting active monitor to {monitor}");
			sector.ActiveMonitorIndex = idx;

			if (Window is not null)
			{
				sector.LastWhimActiveMonitorIndex = idx;
			}

			break;
		}
	}
}
