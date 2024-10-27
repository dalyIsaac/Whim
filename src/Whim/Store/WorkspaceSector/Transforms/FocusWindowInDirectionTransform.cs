namespace Whim;

/// <summary>
/// Focus the window adjacent to the given <paramref name="WindowHandle"/> in the workspace with
/// the given <paramref name="WorkspaceId"/> in the provided <paramref name="Direction"/>.
///
/// Returns whether the active layout engine changed.
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to focus the window in. Defaults to the active workspace.
/// </param>
/// <param name="Direction">
/// The direction to search for the adjacent window to focus. Defaults to <see cref="Direction.None"/>.
/// </param>
/// <param name="WindowHandle">
/// The handle of the window from which to focus the adjacent window. Default to the last focused
/// window if not provided.
/// If provided, this window must exist in the workspace.
/// </param>
/// <example>
/// To focus the window to the left of the specified window:
/// <code>
/// context.Store.Dispatch(new FocusWindowInDirectionTransform(workspaceId, Direction.Left, windowHandle));
/// </code>
///
/// To focus the last focused window in the workspace:
/// <code>
/// context.Store.Dispatch(new FocusWindowInDirectionTransform(workspaceId, Direction.Left));
/// </code>
/// </example>
public record FocusWindowInDirectionTransform(
	WorkspaceId WorkspaceId = default,
	Direction Direction = Direction.None,
	HWND WindowHandle = default
)
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
		ILayoutEngine layoutEngine = workspace.GetActiveLayoutEngine();
		ILayoutEngine newLayoutEngine = layoutEngine.FocusWindowInDirection(Direction, window);

		return newLayoutEngine == layoutEngine
			? workspace
			: workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(workspace.ActiveLayoutEngineIndex, newLayoutEngine),
			};
	}
}
