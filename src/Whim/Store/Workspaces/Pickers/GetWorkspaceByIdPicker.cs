using System;

namespace Whim;

/// <summary>
/// Get the workspace with the provided <paramref name="WorkspaceId"/>.
/// </summary>
/// <param name="WorkspaceId"></param>
public record GetWorkspaceByIdPicker(Guid WorkspaceId) : BaseWorkspacePicker<ImmutableWorkspace>(WorkspaceId)
{
	/// <inheritdoc/>
	protected override ImmutableWorkspace Operation(ImmutableWorkspace workspace) => workspace;
}
