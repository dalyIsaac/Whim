using DotNext;

namespace Whim;

internal static class WorkspaceUtils
{
	public static Result<Empty> Remove(IContext ctx, int idx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		if (slice.Workspaces.Count - 1 < ctx.Store.MonitorSlice.Monitors.Length)
		{
			return Result.FromException<Empty>(new WhimException("There must be a workspace for each monitor"));
		}

		ImmutableWorkspace oldWorkspace = slice.Workspaces[idx];
		slice.Workspaces = slice.Workspaces.RemoveAt(idx);

		IWorkspace oldMutableWorkspace = slice.MutableWorkspaces[idx];
		slice.MutableWorkspaces = slice.MutableWorkspaces.RemoveAt(idx);

		ctx.Butler.MergeWorkspaceWindows(oldMutableWorkspace, slice.MutableWorkspaces[^1]);
		ctx.Butler.Activate(slice.MutableWorkspaces[^1]);

		slice.QueueEvent(new WorkspaceRemovedEventArgs() { Workspace = oldWorkspace });
		slice.WorkspacesToLayout.Remove(oldWorkspace.Id);

		return Empty.Result;
	}

	public static void SetActiveLayoutEngine(WorkspaceSlice slice, int workspaceIdx, int layoutEngineIdx)
	{
		ImmutableWorkspace workspace = slice.Workspaces[workspaceIdx];

		int previousLayoutEngineIdx = workspace.ActiveLayoutEngineIndex;
		slice.Workspaces = slice.Workspaces.SetItem(
			workspaceIdx,
			workspace with
			{
				ActiveLayoutEngineIndex = layoutEngineIdx
			}
		);

		slice.WorkspacesToLayout.Add(workspace.Id);
		slice.QueueEvent(
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
