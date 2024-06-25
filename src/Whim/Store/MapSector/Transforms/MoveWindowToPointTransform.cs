namespace Whim;

/// <summary>
/// Moves the window with <paramref name="WindowHandle"/> to the given <paramref name="Point"/>.
/// </summary>
/// <param name="WindowHandle">
/// The handle of the window to move.
/// </param>
/// <param name="Point">
/// The point to move the window to. The point is in the coordinate space of the monitors,
/// not the unit square.
/// </param>
public record MoveWindowToPointTransform(HWND WindowHandle, IPoint<int> Point) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the monitor.
		IMonitor targetMonitor = ctx.MonitorManager.GetMonitorAtPoint(Point);

		// Get the target workspace.
		Result<IWorkspace> targetWorkspaceResult = ctx.Store.Pick(PickWorkspaceByMonitor(targetMonitor.Handle));
		if (!targetWorkspaceResult.TryGet(out IWorkspace targetWorkspace))
		{
			return Result.FromException<Unit>(targetWorkspaceResult.Error!);
		}

		// Get the old workspace.
		Result<IWorkspace> oldWorkspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(WindowHandle));
		if (!oldWorkspaceResult.TryGet(out IWorkspace oldWorkspace))
		{
			return Result.FromException<Unit>(oldWorkspaceResult.Error!);
		}

		// Normalize `point` into the unit square.
		IPoint<double> normalized = targetMonitor.WorkingArea.NormalizeAbsolutePoint(Point);

		Logger.Debug(
			$"Moving window {WindowHandle} to workspace {targetWorkspace} in monitor {targetMonitor} at normalized point {normalized}"
		);

		IWindow window = rootSector.WindowSector.Windows[WindowHandle];

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (targetWorkspace.Id != oldWorkspace.Id)
		{
			rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
				WindowHandle,
				targetWorkspace.Id
			);
			oldWorkspace.RemoveWindow(window: window);
			oldWorkspace.DoLayout();
		}

		targetWorkspace.MoveWindowToPoint(window, normalized, deferLayout: false);

		rootSector.WorkspaceSector.WindowHandleToFocus = window.Handle;

		return Unit.Result;
	}
}
