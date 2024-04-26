using System;
using System.Collections.Immutable;

namespace Whim;

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
	/// The last focused window in the workspace.
	/// </summary>
	public IWindow? LastFocusedWindow { get; init; }

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	public ImmutableList<ILayoutEngine> LayoutEngines { get; init; } = ImmutableList<ILayoutEngine>.Empty;

	internal ImmutableWorkspace(Guid id, string name)
	{
		Name = name;
	}
}
