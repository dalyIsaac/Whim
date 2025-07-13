global using WorkspaceId = System.Guid;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace
{
	/// <summary>
	/// The unique id of the workspace.
	/// </summary>
	WorkspaceId Id { get; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which is currently active.
	/// </summary>
	int ActiveLayoutEngineIndex { get; }

	/// <summary>
	/// The index of the layout engine in <see cref="LayoutEngines"/> which was previously active.
	/// </summary>
	int PreviousLayoutEngineIndex { get; }

	/// <summary>
	/// The index of the last focused window in the workspace.
	/// WARNING: When the value is 0, it means that no window is focused. Check this with <see cref="HWND.IsNull"/>.
	/// </summary>
	HWND LastFocusedWindowHandle { get; }

	/// <summary>
	/// All the layout engines currently in the workspace.
	/// </summary>
	ImmutableList<ILayoutEngine> LayoutEngines { get; }

	/// <summary>
	/// Map of windows to their <see cref="WindowPosition"/>s.
	/// </summary>
	ImmutableDictionary<HWND, WindowPosition> WindowPositions { get; }

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	string Name { get; }
}
