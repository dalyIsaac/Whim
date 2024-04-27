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
	) =>
		WorkspaceUtils.AddWindow(
			ctx,
			mutableRootSector,
			mutableRootSector.Workspaces.Workspaces.IndexOf(Workspace),
			Window
		);
}
