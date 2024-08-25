namespace Whim;

/// <summary>
/// Swap the <paramref name="WindowHandle"/> in the provided <paramref name="Direction"/> for the workspace
/// with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to swap the window in. Defaults to the active workspace.
/// </param>
/// <param name="Direction">
/// The direction to swap the window in.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window to swap. If not provided, the last focused window will be used.
/// If provided, this window must exist in the workspace.
/// </param>
public record SwapWindowInDirectionTransform(WorkspaceId WorkspaceId = default, Direction Direction = Direction.None, HWND WindowHandle = default)
	: BaseWorkspaceWindowTransform(
		WorkspaceId,
		WindowHandle,
		DefaultToLastFocusedWindow: true,
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
		ILayoutEngine newEngine = oldEngine.SwapWindowInDirection(Direction, window);

		return oldEngine == newEngine
			? workspace
			: workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newEngine),
			};
	}
}
