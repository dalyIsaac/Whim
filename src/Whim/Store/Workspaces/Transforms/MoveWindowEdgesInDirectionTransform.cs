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
			return Result.FromException<Empty>(StoreExceptions.NoValidWindow());
		}

		// Get the containing workspace.
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.GetWorkspaceForWindow(window));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Empty>(workspaceResult.Error!);
		}

		// Get the containing monitor.
		Result<IMonitor> monitorResult = ctx.Store.Pick(Pickers.GetMonitorForWindow(window));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Empty>(monitorResult.Error!);
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Normalize `PixelsDeltas` into the unit square.
		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(PixelsDeltas);

		Logger.Debug($"Normalized point: {normalized}");
		workspace.MoveWindowEdgesInDirection(Edges, normalized, window, deferLayout: false);
		return Empty.Result;
	}
}
