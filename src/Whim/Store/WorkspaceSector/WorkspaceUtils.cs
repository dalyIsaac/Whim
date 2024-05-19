using DotNext;

namespace Whim;

internal static class WorkspaceUtils
{
	public static WorkspaceId OrActiveWorkspace(this WorkspaceId WorkspaceId, IContext ctx) =>
		ctx.WorkspaceManager.ActiveWorkspace.Id;

	public static bool ContainsWorkspaceId(this IWorkspaceManager workspaceManager, WorkspaceId workspaceId)
	{
		foreach (IWorkspace workspace in workspaceManager)
		{
			if (workspace.Id.Equals(workspaceId))
			{
				return true;
			}
		}
		return false;
	}
}
