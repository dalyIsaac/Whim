using System;

namespace Whim;

/// <summary>
/// Get the active layout engine in the provided workspace.
/// </summary>
/// <param name="WorkspaceId"></param>
public record GetActiveLayoutEnginePicker(Guid WorkspaceId) : BaseWorkspacePicker<ILayoutEngine>(WorkspaceId)
{
	/// <inheritdoc/>
	protected override ILayoutEngine Operation(ImmutableWorkspace workspace) =>
		workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex];
}
