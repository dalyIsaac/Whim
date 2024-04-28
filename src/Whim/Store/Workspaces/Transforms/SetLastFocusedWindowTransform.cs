using System;

namespace Whim;

/// <summary>
/// Set the last focused window in the workspace with given <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
/// <param name="Window"></param>
public record SetLastFocusedWindowTransform(Guid WorkspaceId, IWindow Window)
	: BaseWorkspaceWindowTransform(WorkspaceId, Window, false)
{
	/// <inheritdoc/>
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace, IWindow window) =>
		workspace.LastFocusedWindow == window ? workspace : workspace with { LastFocusedWindow = window };
}
