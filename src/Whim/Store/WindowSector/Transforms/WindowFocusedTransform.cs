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

		// If we know the window, use what the map sector knows instead of Windows.
		if (Window is not null && ctx.Store.Pick(PickMonitorByWindow(Window.Handle)).TryGet(out IMonitor monitor))
		{
			Logger.Debug($"Setting active monitor to {monitor}");
			monitorSector.ActiveMonitorHandle = monitor.Handle;
			monitorSector.LastWhimActiveMonitorHandle = monitor.Handle;
			return;
		}

		// We don't know the window, so get the foreground window.
		HWND hwnd = Window?.Handle ?? internalCtx.CoreNativeManager.GetForegroundWindow();
		Logger.Debug($"Focusing hwnd {hwnd}");

		if (hwnd.IsNull)
		{
			Logger.Debug($"Hwnd is desktop window, ignoring");
			return;
		}

		HMONITOR monitorHandle = internalCtx.CoreNativeManager.MonitorFromWindow(
			hwnd,
			MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST
		);

		foreach (IMonitor currentMonitor in monitorSector.Monitors)
		{
			if (!currentMonitor.Handle.Equals(monitorHandle))
			{
				continue;
			}

			Logger.Debug($"Setting active monitor to {currentMonitor}");
			monitorSector.ActiveMonitorHandle = currentMonitor.Handle;

			// The window isn't tracked by Whim, so don't update LastWhimActiveMonitorHandle.
			if (Window is not null)
			{
				monitorSector.LastWhimActiveMonitorHandle = currentMonitor.Handle;
			}
			break;
		}
	}

	private static void UpdateMapSector(IContext ctx, IWindow? window)
	{
		foreach (IWorkspace workspace in ctx.Store.Pick(PickWorkspaces()))
		{
			((IInternalWorkspace)workspace).WindowFocused(window);
		}

		// Only activate the workspace if the window is in a workspace, and the workspace isn't currently
		// active.
		if (window is null)
		{
			return;
		}

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(window.Handle));
		if (!workspaceResult.TryGet(out IWorkspace workspaceForWindow))
		{
			return;
		}

		if (ctx.Store.Pick(PickMonitorByWorkspace(workspaceForWindow.Id)).IsSuccessful)
		{
			return;
		}

		ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspaceForWindow.Id));
	}
}
