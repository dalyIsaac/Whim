namespace Whim;

internal static class WorkspaceUtils
{
	public static void SetActiveLayoutEngine(WorkspaceSector sector, int workspaceIdx, int layoutEngineIdx)
	{
		ImmutableWorkspace workspace = sector.Workspaces[workspaceIdx];

		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		sector.Workspaces = sector.Workspaces.SetItem(
			workspaceIdx,
			workspace with
			{
				ActiveLayoutEngineIndex = layoutEngineIdx
			}
		);

		sector.WorkspacesToLayout.Add(workspace.Id);
		sector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				PreviousLayoutEngine = workspace.LayoutEngines[previousLayoutEngineIdx],
				CurrentLayoutEngine = workspace.LayoutEngines[layoutEngineIdx]
			}
		);
	}

	public static WhimException WorkspaceDoesNotExist() =>
		new("Provided workspace did not exist in collection, could not remove");
}
