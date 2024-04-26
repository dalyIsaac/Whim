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
		return Empty.Result;
	}

	public static WhimException RemoveWorkspaceFailed() =>
		new("Provided workspace did not exist in collection, could not remove");
}
