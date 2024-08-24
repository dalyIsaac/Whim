namespace Whim;

/// <summary>
/// Manage the movement of <see cref="IWindow"/>s between <see cref="IWorkspace"/>s and <see cref="IMonitor"/>s.
/// </summary>
[Obsolete("Use transforms and pickers to interact with the store instead.")]
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
	[Obsolete("Use the transform ActivateWorkspaceTransform instead.")]
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
	[Obsolete("Use the transform ActivateAdjacentWorkspaceTransform instead.")]
	void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Triggers all active workspaces to update their layout.
	/// Active workspaces are those that are visible on a monitor.
	/// </summary>
	[Obsolete("Use the transform LayoutAllActiveWorkspacesTransform instead.")]
	void LayoutAllActiveWorkspaces();

	/// <summary>
	/// Focus the Windows desktop's window.
	/// </summary>
	/// <param name="monitor"></param>
	[Obsolete("Use the transform FocusMonitorDesktopTransform instead.")]
	void FocusMonitorDesktop(IMonitor monitor);

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
	[Obsolete("Use the transform MoveWindowEdgesInDirectionTransform instead.")]
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
	[Obsolete("Use the transform MoveWindowToAdjacentWorkspaceTransform instead.")]
	void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="monitor"/>.
	/// </summary>
	/// <param name="monitor">The monitor to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	[Obsolete("Use the transform MoveWindowToMonitorTransform instead.")]
	void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the next monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	[Obsolete("Use the transform MoveWindowToAdjacentMonitorTransform instead.")]
	void MoveWindowToNextMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the previous monitor.
	/// </summary>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	[Obsolete("Use the transform MoveWindowToAdjacentMonitorTransform instead.")]
	void MoveWindowToPreviousMonitor(IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">
	/// The point to move the window to. The point is in the coordinate space of the monitors,
	/// not the unit square.
	/// </param>
	[Obsolete("Use the transform MoveWindowToPointTransform instead.")]
	void MoveWindowToPoint(IWindow window, IPoint<int> point);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="workspace"/>.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <param name="window">
	/// The window to move. If <see langword="null"/>, this will default to
	/// the focused/active window.
	/// </param>
	[Obsolete("Use the transform MoveWindowToWorkspaceTransform instead.")]
	void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null);

	/// <summary>
	/// Merges the windows of the given <paramref name="source"/> into the given <paramref name="target"/>.
	/// </summary>
	/// <param name="source">The workspace to remove.</param>
	/// <param name="target">The workspace to merge the windows into.</param>
	[Obsolete("Use the transform MergeWorkspaceWindowsTransform instead.")]
	void MergeWorkspaceWindows(IWorkspace source, IWorkspace target);

	/// <summary>
	/// Swap the given <paramref name="workspace"/> with the adjacent monitor.
	/// </summary>
	/// <param name="workspace"></param>
	/// <param name="reverse">
	/// When <see langword="true"/>, swaps workspace with the previous monitor, otherwise with the next. Defaults to <see langword="false" />.
	/// </param>
	[Obsolete("Use the transform SwapWorkspaceWithAdjacentMonitorTransform instead.")]
	void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false);
}
