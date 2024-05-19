using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal record WindowFocusedTransform(IWindow? Window) : Transform()
{
	internal override Result<Unit> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		SetActiveMonitor(ctx, internalCtx, mutableRootSector);
		UpdateMapSector(ctx, Window);

		mutableRootSector.WindowSector.QueueEvent(new WindowFocusedEventArgs() { Window = Window });

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
		if (Window is not null)
		{
			Logger.Debug($"Focusing window {Window}");

			if (ctx.Store.Pick(Pickers.PickMonitorByWindow(Window.Handle)).TryGet(out IMonitor monitor))
			{
				Logger.Debug($"Setting active monitor to {monitor}");
				monitorSector.ActiveMonitorHandle = monitor.Handle;
				monitorSector.LastWhimActiveMonitorHandle = monitor.Handle;
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

	private static void UpdateMapSector(IContext ctx, IWindow? window)
	{
		foreach (IWorkspace workspace in ctx.WorkspaceManager)
		{
			((IInternalWorkspace)workspace).WindowFocused(window);
		}

		if (window is null)
		{
			return;
		}

		if (!ctx.Store.Pick(Pickers.PickWorkspaceByWindow(window.Handle)).TryGet(out IWorkspace workspaceForWindow))
		{
			Logger.Debug($"Window {window} was not found in any workspace");
			return;
		}

		if (ctx.Store.Pick(Pickers.PickMonitorByWorkspace(workspaceForWindow.Id)).IsSuccessful)
		{
			Logger.Debug($"Window {window} is in an active workspace");
			return;
		}

		ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceForWindow.Id));
	}
}
