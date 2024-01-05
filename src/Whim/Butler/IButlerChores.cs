namespace Whim;

/// <summary>
/// Manage the movement of <see cref="IWindow"/>s between <see cref="IWorkspace"/>s and <see cref="IMonitor"/>s.
/// </summary>
public interface IButlerChores
{
	/// <summary>
	/// Activates the given workspace in the active monitor, or the given monitor (if provided).
	/// </summary>
	/// <param name="workspace">The workspace to activate.</param>
	/// <param name="monitor">
	/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
	/// the active monitor.
	/// </param>
	void Activate(IWorkspace workspace, IMonitor? monitor = null);

	/// <summary>
	/// Activates the next (or previous) workspace in the given monitor.
	/// </summary>
	/// <param name="monitor">
	/// The monitor to activate the next workspace in. Defaults to <see cref="IMonitorManager.ActiveMonitor"/>.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous monitor, otherwise gets the next monitor. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Triggers all active workspaces to update their layout.
	/// Active workspaces are those that are visible on a monitor.
	/// </summary>
	void LayoutAllActiveWorkspaces();

	/// <summary>
	/// Moves the given <paramref name="window"/> by the given <paramref name="pixelsDeltas"/>.
	/// </summary>
	/// <param name="edges">The edges to change.</param>
	/// <param name="pixelsDeltas">
	/// The deltas (in pixels) to change the given <paramref name="edges"/> by. When a value is
	/// positive, then the edge will move in the direction of the <paramref name="edges"/>.
	/// The <paramref name="pixelsDeltas"/> are in the coordinate space of the monitors, not the
	/// unit square.
	/// </param>
	/// <param name="window"></param>
	/// <returns>Whether the window's edges were moved.</returns>
	bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the next (or previous) workspace.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to the focused/active window.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, moves to the previous workspace, otherwise moves to the next workspace. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="monitor"/>.
	/// </summary>
	/// <param name="monitor">The monitor to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the next monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToNextMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the previous monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToPreviousMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">
	/// The point to move the window to. The point is in the coordinate space of the monitors,
	/// not the unit square.
	/// </param>
	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="workspace"/>.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	/// <summary>
	/// Swap the given <paramref name="workspace"/> with the adjacent monitor.
	/// </summary>
	/// <param name="workspace"></param>
	/// <param name="reverse">
	/// When <see langword="true"/>, swaps workspace with the previous monitor, otherwise with the next. Defaults to <see langword="false" />.
	/// </param>
	void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false);
}
