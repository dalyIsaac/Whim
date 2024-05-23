namespace Whim;

internal static class WorkspaceUtils
{
	public static WorkspaceId OrActiveWorkspace(this WorkspaceId WorkspaceId, IContext ctx) =>
		ctx.WorkspaceManager.ActiveWorkspace.Id;
}
