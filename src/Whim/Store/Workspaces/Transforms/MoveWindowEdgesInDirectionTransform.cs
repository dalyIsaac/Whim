using DotNext;

namespace Whim;

/// <summary>
/// Moves the given <paramref name="Window"/> by the given <paramref name="PixelsDeltas"/>.
/// </summary>
/// <param name="Workspace"></param>
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
	ImmutableWorkspace Workspace,
	IWindow? Window,
	Direction Edges,
	IPoint<int> Deltas
) : BaseWorkspaceWindowTransform(Workspace, Window, true)
{
	/// <inheritdoc/>
	protected override Result<ImmutableWorkspace> Operation(IWindow window)
	{
		// Get the containing monitor.
		Result<IMonitor> monitorResult = ctx.Store.Pick(Pickers.GetMonitorForWindow(window));
		if (!monitorResult.TryGet(out IMonitor monitor))
		{
			return Result.FromException<Empty>(monitorResult.Error!);
		}

		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(PixelsDeltas);

		ILayoutEngine oldEngine = Workspace.LayoutEngines[Workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newEngine = oldEngine.MoveWindowEdgesInDirection(Edges, Deltas, window);

		return oldEngine == newEngine
			? Workspace
			: Workspace with
			{
				LayoutEngines = Workspace.LayoutEngines.SetItem(Workspace.ActiveLayoutEngineIndex, newEngine)
			};
	}
}
