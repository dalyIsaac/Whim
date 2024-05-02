using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> to the given <paramref name="Point"/>.
/// </summary>
/// <param name="Window">The window to move.</param>
/// <param name="Point">
/// The point to move the window to. The point is in the coordinate space of the monitors,
/// not the unit square.
/// </param>
public record MoveWindowToPointTransform(IWindow Window, IPoint<int> Point) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Get the monitor.
		IMonitor targetMonitor = ctx.MonitorManager.GetMonitorAtPoint(Point);

		// Get the target workspace.
		Result<IWorkspace> targetWorkspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForMonitor(targetMonitor));
		if (!targetWorkspaceResult.TryGet(out IWorkspace targetWorkspace))
		{
			return Result.FromException<Empty>(targetWorkspaceResult.Error!);
		}

		// Get the old workspace.
		Result<IWorkspace> oldWorkspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForWindow(Window));
		if (!oldWorkspaceResult.TryGet(out IWorkspace oldWorkspace))
		{
			return Result.FromException<Empty>(oldWorkspaceResult.Error!);
		}

		// Normalize `point` into the unit square.
		IPoint<double> normalized = targetMonitor.WorkingArea.NormalizeAbsolutePoint(Point);

		Logger.Debug(
			$"Moving window {Window} to workspace {targetWorkspace} in monitor {targetMonitor} at normalized point {normalized}"
		);

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (!targetWorkspace.Equals(oldWorkspace))
		{
			rootSector.Maps.WindowWorkspaceMap = rootSector.Maps.WindowWorkspaceMap.SetItem(Window, targetWorkspace);
			oldWorkspace.RemoveWindow(Window);
			oldWorkspace.DoLayout();
		}

		targetWorkspace.MoveWindowToPoint(Window, normalized, deferLayout: false);

		// Trigger layouts.
		Window.Focus();

		return Empty.Result;
	}
}
