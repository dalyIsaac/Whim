using DotNext;

namespace Whim;

/// <summary>
/// Adds the given <paramref name="Window"/> to the provided <paramref name="Workspace"/>
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
public record AddWindowToWorkspaceTransform(ImmutableWorkspace Workspace, IWindow Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.Workspaces;

		int workspaceIdx = sector.Workspaces.IndexOf(Workspace);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		ImmutableWorkspace workspace = Workspace with { Windows = Workspace.Windows.Add(Window) };

		for (int idx = 0; idx < workspace.LayoutEngines.Count; idx++)
		{
			workspace = workspace with
			{
				LayoutEngines = workspace.LayoutEngines.SetItem(idx, workspace.LayoutEngines[idx].AddWindow(Window))
			};
		}

		sector.Workspaces = sector.Workspaces.SetItem(workspaceIdx, workspace);

		return Empty.Result;
	}
}
