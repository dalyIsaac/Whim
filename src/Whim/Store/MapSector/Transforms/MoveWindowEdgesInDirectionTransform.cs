using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="WindowHandle"/> by the given <paramref name="PixelsDeltas"/>.
/// </summary>
/// <param name="Edges">The edges to change.</param>
/// <param name="PixelsDeltas">
/// The deltas (in pixels) to change the given <paramref name="Edges"/> by. When a value is
/// positive, then the edge will move in the direction of the <paramref name="Edges"/>.
/// The <paramref name="PixelsDeltas"/> are in the coordinate space of the monitors, not the
/// unit square.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window to move. If not provided, the last focused window will be used.
/// </param>
/// <returns>Whether the window's edges were moved.</returns>
public record MoveWindowEdgesInDirectionTransform(
	Direction Edges,
	IPoint<int> PixelsDeltas,
	HWND WindowHandle = default
) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		HWND windowHandle = WindowHandle.OrLastFocusedWindow(ctx);
		if (windowHandle == default)
		{
			return Result.FromException<Unit>(StoreExceptions.NoValidWindow());
		}

		Result<IWindow> windowResult = ctx.Store.Pick(PickWindowByHandle(windowHandle));
		if (!windowResult.TryGet(out IWindow window))
		{
			return Result.FromException<Unit>(windowResult.Error!);
		}

		// Get the containing workspace.
		Result<IWorkspace> workspaceResult = ctx.Store.Pick(PickWorkspaceByWindow(windowHandle));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		// Get the containing monitor.
		Result<IMonitor> monitorResult = ctx.Store.Pick(PickMonitorByWindow(windowHandle));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Unit>(monitorResult.Error!);
		}

		Logger.Debug($"Moving window {windowHandle} to workspace {workspace}");

		// Normalize `PixelsDeltas` into the unit square.
		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(PixelsDeltas);

		Logger.Debug($"Normalized point: {normalized}");
		workspace.MoveWindowEdgesInDirection(Edges, normalized, window, deferLayout: false);
		return Unit.Result;
	}
}
