using System;
using DotNext;

namespace Whim;

/// <summary>
/// Set the last focused window in the workspace with given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
internal record SetLastFocusedWindowTransform(Guid WorkspaceId, IWindow Window)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, false)
{
	private protected override Result<ImmutableWorkspace> WindowOperation(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceSector sector,
		ImmutableWorkspace workspace,
		IWindow window
	) => workspace.LastFocusedWindow == window ? workspace : workspace with { LastFocusedWindow = window };
}
