using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal record MoveWindowEdgesInDirectionWorkspaceTransform(
	WorkspaceId WorkspaceId,
	Direction Edges,
	IPoint<double> Deltas,
	HWND WindowHandle
) : BaseWorkspaceWindowTransform(WorkspaceId, WindowHandle, false)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ImmutableWorkspace workspace,
		IWindow window
	) =>
		workspace with
		{
			LayoutEngines = workspace.LayoutEngines.SetItem(
				workspace.ActiveLayoutEngineIndex,
				workspace
					.LayoutEngines[workspace.ActiveLayoutEngineIndex]
					.MoveWindowEdgesInDirection(Edges, Deltas, window)
			)
		};
}
