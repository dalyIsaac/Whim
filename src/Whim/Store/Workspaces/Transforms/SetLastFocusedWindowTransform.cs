namespace Whim;

/// <summary>
/// Set the last focused window in the provided workspace.
/// </summary>
/// <param name="Workspace"></param>
/// <param name="Window"></param>
public record SetLastFocusedWindowTransform(ImmutableWorkspace Workspace, IWindow Window)
	: BaseWorkspaceWindowTransform(Workspace, Window, false)
{
	/// <summary>
	/// Set the last focused window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	protected override ImmutableWorkspace Operation(IWindow window) => Workspace with { LastFocusedWindow = window };
}
