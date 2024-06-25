namespace Whim;

/// <summary>
/// Updates the <see cref="WindowPosition"/>s in the <see cref="Workspace"/> to be <see cref="WindowSize.Minimized"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
internal record DeactivateWorkspaceTransform(WorkspaceId WorkspaceId)
	: BaseWorkspaceTransform(WorkspaceId, SkipDoLayout: true)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		ImmutableDictionary<HWND, WindowPosition> updatedPositions = workspace.WindowPositions;
		foreach ((HWND hwnd, WindowPosition pos) in workspace.WindowPositions)
		{
			WindowPosition newPos = pos with { WindowSize = WindowSize.Minimized };
			updatedPositions = updatedPositions.SetItem(hwnd, newPos);

			ctx.NativeManager.HideWindow(hwnd);
		}

		return workspace with
		{
			WindowPositions = updatedPositions
		};
	}
}
