using System;
using System.Collections.Immutable;

namespace Whim;

public record ImmutableWorkspace
{
	/// <summary>
	/// The unique id of the workspace.
	/// </summary>
	internal Guid Id { get; }

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	internal string Name { get; init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	internal int ActiveLayoutEngineIndex { get; init; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	internal int PreviousLayoutEngineIndex { get; init; }

	/// <summary>
	/// The last focused window in the workspace.
	/// </summary>
	internal IWindow? LastFocusedWindow { get; init; }

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	internal ImmutableList<ILayoutEngine> LayoutEngines { get; init; } = ImmutableList<ILayoutEngine>.Empty;

	internal ImmutableWorkspace(Guid id, string name)
	{
		Name = name;
	}
}
