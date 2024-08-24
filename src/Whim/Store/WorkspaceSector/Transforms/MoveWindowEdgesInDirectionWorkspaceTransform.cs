namespace Whim;

internal record MoveWindowEdgesInDirectionWorkspaceTransform(
	WorkspaceId WorkspaceId,
	Direction Edges,
	IPoint<double> Deltas,
	HWND WindowHandle
)
	: BaseWorkspaceWindowTransform(
		WorkspaceId,
		WindowHandle,
		DefaultToLastFocusedWindow: false,
		IsWindowRequiredInWorkspace: true,
		SkipDoLayout: false
	)
{
	private protected override Result<Workspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace,
		IWindow window
	)
	{
		ILayoutEngine oldEngine = workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
		ILayoutEngine newEngine = oldEngine.MoveWindowEdgesInDirection(Edges, Deltas, window);

		return oldEngine == newEngine
			? workspace
			: workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newEngine),
			};
	}
}
