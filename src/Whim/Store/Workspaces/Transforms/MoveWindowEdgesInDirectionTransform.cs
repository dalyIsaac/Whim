using System;
using DotNext;

namespace Whim;

/// <summary>
/// Change the <paramref name="Window"/>'s <paramref name="Edges"/> direction by
/// the specified <paramref name="Deltas"/> for the workspace with given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window">
/// The window to change the edge of. If null, the currently focused window is used.
/// </param>
/// <param name="Edges">
/// The edges to change.
/// </param>
/// <param name="Deltas">
/// The deltas (in pixels) to change the given <paramref name="Edges"/> by. When a value is
/// positive, then the edge will move in the direction of the <paramref name="Edges"/>.
/// The <paramref name="PixelsDeltas"/> are in the coordinate space of the monitors, not the
/// unit square.
/// </param>
public record MoveWindowEdgesInDirectionTransform(
	Guid WorkspaceId,
	IWindow? Window,
	Direction Edges,
	IPoint<int> Deltas
) : BaseWorkspaceWindowTransform(WorkspaceId, Window, true)
{
	private protected override Result<Result<ImmutableWorkspace>> WindowOperation(
		WorkspaceSector sector,
		ImmutableWorkspace workspace,
		IWindow window
	)
	{
		// Get the containing monitor.
		Result<IMonitor> monitorResult = ctx.Store.Pick(Pickers.GetMonitorForWindow(window));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Empty>(monitorResult.Error!);
		}

		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(PixelsDeltas);

		ILayoutEngine oldEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newEngine = oldEngine.MoveWindowEdgesInDirection(Edges, Deltas, window);

		return oldEngine == newEngine
			? workspace
			: workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newEngine)
			};
	}
}
