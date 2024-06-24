namespace Whim;

internal static class WorkspaceUtils
{
	public static WorkspaceId OrActiveWorkspace(this WorkspaceId WorkspaceId, IContext ctx) =>
		WorkspaceId == default ? ctx.WorkspaceManager.ActiveWorkspace.Id : WorkspaceId;
}
