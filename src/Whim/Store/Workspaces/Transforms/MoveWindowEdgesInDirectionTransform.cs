using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> by the given <paramref name="PixelsDeltas"/>.
/// </summary>
/// <param name="Edges">The edges to change.</param>
/// <param name="PixelsDeltas">
/// The deltas (in pixels) to change the given <paramref name="Edges"/> by. When a value is
/// positive, then the edge will move in the direction of the <paramref name="Edges"/>.
/// The <paramref name="PixelsDeltas"/> are in the coordinate space of the monitors, not the
/// unit square.
/// </param>
/// <param name="Window"></param>
/// <returns>Whether the window's edges were moved.</returns>
public record MoveWindowEdgesInDirectionTransform(Direction Edges, IPoint<int> PixelsDeltas, IWindow? Window = null)
	: Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		IWindow? window = Window ?? ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			return Result.FromException<Empty>(new WhimException("No window was found"));
		}

		// Get the containing workspace.
		IWorkspace? workspace = ctx.Butler.Pantry.GetWorkspaceForWindow(window);
		if (workspace == null)
		{
			return Result.FromException<Empty>(new WhimException($"Could not find workspace for window {window}"));
		}

		// Get the containing monitor.
		IMonitor? monitor = ctx.Butler.Pantry.GetMonitorForWorkspace(workspace);
		if (monitor == null)
		{
			return Result.FromException<Empty>(new WhimException($"Could not find monitor for workspace {workspace}"));
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Normalize `PixelsDeltas` into the unit square.
		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(PixelsDeltas);

		Logger.Debug($"Normalized point: {normalized}");
		workspace.MoveWindowEdgesInDirection(Edges, normalized, window, deferLayout: false);
		return Empty.Result;
	}
}
