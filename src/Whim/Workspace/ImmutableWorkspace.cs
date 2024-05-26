using System.Collections.Immutable;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public record ImmutableWorkspace
{
	/// <summary>
	/// The unique id of the workspace.
	/// </summary>
	public WorkspaceId Id { get; internal init; }

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	public string Name { get; internal init; } = string.Empty;

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	public int ActiveLayoutEngineIndex { get; internal init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	public int PreviousLayoutEngineIndex { get; internal init; }

	/// <summary>
	/// The index of the last focused window in the workspace.
	/// </summary>
	public int? LastFocusedWindowIndex { get; internal init; }

	/// <summary>
	/// All the windows in the workspace.
	/// </summary>
	public ImmutableHashSet<IWindow> Windows { get; internal init; } = ImmutableHashSet<IWindow>.Empty;

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	public ImmutableList<ILayoutEngine> LayoutEngines { get; internal init; } = ImmutableList<ILayoutEngine>.Empty;
}
