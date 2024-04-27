using DotNext;

namespace Whim;

internal static class WorkspaceUtils
{
	public static Result<Empty> AddWindow(
		IContext ctx,
		MutableRootSector mutableRootSector,
		int workspaceIdx,
		IWindow window
	)
	{
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceDoesNotExist());
		}

		WorkspaceSector sector = mutableRootSector.Workspaces;
		ImmutableWorkspace workspace = sector.Workspaces[workspaceIdx];

		if (workspace.Windows.Contains(window))
		{
			return Empty.Result;
		}

		workspace = workspace with { Windows = workspace.Windows.Add(window) };

		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(idx, workspace.LayoutEngines[idx].AddWindow(window))
			};
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspaceIdx, workspace);

		return Empty.Result;
	}

	public static Result<Empty> RemoveWorkspace(IContext ctx, MutableRootSector mutableRootSector, int idx)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		if (sector.Workspaces.Count - 1 < mutableRootSector.Monitors.Monitors.Length)
		{
			return Result.FromException<Empty>(new WhimException("There must be a workspace for each monitor"));
		}

		ImmutableWorkspace oldWorkspace = sector.Workspaces[idx];
		sector.Workspaces = sector.Workspaces.RemoveAt(idx);

		IWorkspace oldMutableWorkspace = sector.MutableWorkspaces[idx];
		sector.MutableWorkspaces = sector.MutableWorkspaces.RemoveAt(idx);

		ctx.Butler.MergeWorkspaceWindows(oldMutableWorkspace, sector.MutableWorkspaces[^1]);
		ctx.Butler.Activate(sector.MutableWorkspaces[^1]);

		sector.QueueEvent(new WorkspaceRemovedEventArgs() { Workspace = oldWorkspace });
		sector.WorkspacesToLayout.Remove(oldWorkspace.Id);

		return Empty.Result;
	}

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
