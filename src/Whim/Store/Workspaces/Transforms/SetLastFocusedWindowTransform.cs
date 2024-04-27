using DotNext;

namespace Whim;

/// <summary>
/// Set the last focused window in the provided workspace.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
public record SetLastFocusedWindowTransform(ImmutableWorkspace Workspace, IWindow? Window) : Transform
{
	internal override Result<Empty> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;

		int workspaceIdx = slice.Workspaces.IndexOf(Workspace);
		if (workspaceIdx == -1)
		{
			return Result.FromException<Empty>(WorkspaceUtils.WorkspaceDoesNotExist());
		}

		if (Window != null && !Workspace.Windows.Contains(Window))
		{
			return Result.FromException<Empty>(new WhimException("Window not in workspace"));
		}

		slice.Workspaces = slice.Workspaces.SetItem(workspaceIdx, Workspace with { LastFocusedWindow = Window });
		return Empty.Result;
	}
}
