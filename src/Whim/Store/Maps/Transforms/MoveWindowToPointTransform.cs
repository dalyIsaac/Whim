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
		IWorkspace? targetWorkspace = ctx.Butler.Pantry.GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			return Result.FromException<Empty>(
				new WhimException($"Monitor {targetMonitor} was not found to correspond to any workspace")
			);
		}

		// Get the old workspace.
		IWorkspace? oldWorkspace = ctx.Butler.Pantry.GetWorkspaceForWindow(Window);
		if (oldWorkspace == null)
		{
			return Result.FromException<Empty>(new WhimException($"Window {Window} was not found in any workspace"));
		}

		// Normalize `point` into the unit square.
		IPoint<double> normalized = targetMonitor.WorkingArea.NormalizeAbsolutePoint(Point);

		Logger.Debug(
			$"Moving window {Window} to workspace {targetWorkspace} in monitor {targetMonitor} at normalized point {normalized}"
		);

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (!targetWorkspace.Equals(oldWorkspace))
		{
			ctx.Butler.Pantry.SetWindowWorkspace(Window, targetWorkspace);
			oldWorkspace.RemoveWindow(Window);
			oldWorkspace.DoLayout();
		}

		targetWorkspace.MoveWindowToPoint(Window, normalized, deferLayout: false);

		// Trigger layouts.
		Window.Focus();

		return Empty.Result;
	}
}
