using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Get all the windows in the provided workspace.
/// </summary>
/// <param name="WorkspaceId"></param>
public record GetAllWorkspaceWindowsPicker(Guid WorkspaceId) : BaseWorkspacePicker<IEnumerable<IWindow>>(WorkspaceId)
{
	/// <inheritdoc/>
	protected override IEnumerable<IWindow> Operation(ImmutableWorkspace workspace) => workspace.Windows;
}
