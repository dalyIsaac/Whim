using System;
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
	public Guid Id { get; }

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	public string Name { get; init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	public int ActiveLayoutEngineIndex { get; init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	public int PreviousLayoutEngineIndex { get; init; }

	/// <summary>
	/// The index of the last focused window in the workspace.
	/// </summary>
	public int? LastFocusedWindowIndex { get; init; }

	/// <summary>
	/// All the windows in the workspace.
	/// </summary>
	public ImmutableHashSet<IWindow> Windows { get; init; } = ImmutableHashSet<IWindow>.Empty;

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	public ImmutableList<ILayoutEngine> LayoutEngines { get; init; } = ImmutableList<ILayoutEngine>.Empty;

	internal ImmutableWorkspace(Guid id, string name)
	{
		Name = name;
	}
}
