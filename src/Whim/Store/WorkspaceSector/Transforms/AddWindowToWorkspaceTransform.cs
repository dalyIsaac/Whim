using DotNext;

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
		workspace = workspace with
		{
			// We add the window with a dummy window position. When the workspace gets laid out, then the WindowPosition
			// will become populated correctly.
			WindowPositions = workspace.WindowPositions.SetItem(Window.Handle, new WindowPosition())
		};

		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(idx, workspace.LayoutEngines[idx].AddWindow(Window))
			};
		}

		return workspace;
	}
}
