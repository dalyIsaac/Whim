namespace Whim;

/// <summary>
/// Moves the window with <paramref name="WindowHandle"/> to the monitor with <paramref name="MonitorHandle"/>.
/// </summary>
/// <param name="MonitorHandle">
/// The handle of the monitor to move the window to.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, this will default to the focused/active window.
/// </param>
public record MoveWindowToMonitorTransform(HMONITOR MonitorHandle, HWND WindowHandle = default) : WhimTransform
{
	internal override WhimResult<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;

		HWND windowHandle = WindowHandle.OrLastFocusedWindow(ctx);
		if (windowHandle == default)
		{
			return Result.FromException<Unit>(StoreExceptions.NoValidWindow());
		}

		Logger.Debug($"Moving window {windowHandle} to monitor {MonitorHandle}");

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByMonitor(MonitorHandle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		Result<IMonitor> oldMonitorResult = ctx.Store.Pick(PickMonitorByWindow(windowHandle));
		if (!oldMonitorResult.TryGet(out IMonitor oldMonitor))
		{
			return Result.FromException<Unit>(oldMonitorResult.Error!);
		}

		if (oldMonitor.Handle == MonitorHandle)
		{
			Logger.Debug($"Window {WindowHandle} is already on monitor {MonitorHandle}");
			return Unit.Result;
		}

		return ctx.Store.Dispatch(new MoveWindowToWorkspaceTransform(workspace.Id, windowHandle));
	}
}
