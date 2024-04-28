using System;

namespace Whim;

/// <summary>
/// Get the last focused window in the provided workspace.
/// </summary>
/// <param name="WorkspaceId">The workspace to get the last focused window for.</param>
public record GetLastFocusedWindowPicker(Guid WorkspaceId) : BaseWorkspacePicker<IWindow?>(WorkspaceId)
{
	/// <inheritdoc/>
	protected override IWindow? Operation(ImmutableWorkspace workspace) => workspace.LastFocusedWindow;
}
