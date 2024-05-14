using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowFocusedTransform(HWND Handle = default) : Transform()
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		SetActiveMonitor(ctx, internalCtx, mutableRootSector);
		return Unit.Result;
	}

	/// <summary>
	/// Set the active monitor.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="internalCtx"></param>
	/// <param name="root"></param>
	private void SetActiveMonitor(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		MonitorSector monitorSector = root.MonitorSector;

		// If we know the window, use what the Butler knows instead of Windows.
		IWindow? Window = Handle != default ? WindowUtils.GetWindow(root, Handle) : null;
		if (Window is not null)
		{
			Logger.Debug($"Focusing window {Window}");
			if (ctx.Butler.Pantry.GetMonitorForWindow(Window) is IMonitor monitor)
			{
				Logger.Debug($"Setting active monitor to {monitor}");
				monitorSector.ActiveMonitorHandle = monitor.Handle;
				monitorSector.LastWhimActiveMonitorHandle = monitor.Handle;
				return;
			}
		}

		// We don't know the window, so get the foreground window.
		HWND hwnd = Handle == default ? internalCtx.CoreNativeManager.GetForegroundWindow() : Handle;
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

		foreach (IMonitor monitor in monitorSector.Monitors)
		{
			if (!monitor.Handle.Equals(hMONITOR))
			{
				continue;
			}

			Logger.Debug($"Setting active monitor to {monitor}");
			monitorSector.ActiveMonitorHandle = monitor.Handle;

			if (Window is not null)
			{
				monitorSector.LastWhimActiveMonitorHandle = monitor.Handle;
			}
			break;
		}
	}
}
