using System.Collections.Immutable;

namespace Whim;

public class ImmutableWorkspace
{
	/// <summary>
	/// The name of the workspace.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	public int ActiveLayoutEngineIndex { get; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	public int PreviousLayoutEngineIndex { get; }

	/// <summary>
	/// The last focused window in the workspace.
	/// </summary>
	public IWindow? LastFocusedWindow { get; }

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	public ImmutableList<ILayoutEngine> LayoutEngines { get; } = ImmutableList<ILayoutEngine>.Empty;

	internal ImmutableWorkspace(string name)
	{
		Name = name;
	}
}
