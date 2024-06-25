namespace Whim;

/// <summary>
/// Adds the given <paramref name="Window"/> to the workspace with the given <paramref name="WorkspaceId"/>
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
internal record AddWindowToWorkspaceTransform(WorkspaceId WorkspaceId, IWindow Window)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		ImmutableDictionary<HWND, WindowPosition> updatedPositions = workspace.WindowPositions.SetItem(
			Window.Handle,
			new WindowPosition()
		);

		ImmutableList<ILayoutEngine>.Builder newLayoutEngines = ImmutableList.CreateBuilder<ILayoutEngine>();
		foreach (ILayoutEngine layoutEngine in workspace.LayoutEngines)
		{
			newLayoutEngines.Add(layoutEngine.AddWindow(Window));
		}

		return workspace with
		{
			WindowPositions = updatedPositions,
			LayoutEngines = newLayoutEngines.ToImmutableList()
		};
	}
}
