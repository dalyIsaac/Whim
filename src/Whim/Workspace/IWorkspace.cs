global using WorkspaceId = System.Guid;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace : IDisposable
{
	/// <summary>
	/// The unique id of the workspace.
	/// </summary>
	WorkspaceId Id { get; }

	/// <summary>
	/// The name of the workspace.
	/// </summary>
	/// <remarks>
	/// Until the legacy <see cref="Workspace"/> implementation is removed, this is called
	/// <c>BackingName</c> to avoid confusion with <see cref="Name"/>.
	/// Once the legacy <see cref="Workspace"/> implementation is removed, this will be
	/// called <c>Name</c>.
	/// </remarks>
	string BackingName { get; }

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
	/// The name of the workspace. When the <c>Name</c> is set, the
	/// <see cref="IWorkspaceManager.WorkspaceRenamed"/> event is triggered.
	/// </summary>
	string Name { get; set; }

	#region Layout engine
	/// <summary>
	/// The active layout engine.
	/// </summary>
	ILayoutEngine ActiveLayoutEngine { get; }

	/// <summary>
	/// Rotate to the next layout engine.
	/// </summary>
	/// <param name="nextIdx">The index of the layout engine to make active.</param>
	/// <returns>
	/// <see langword="true"/> if the layout engine is the one specified by <paramref name="nextIdx"/>.
	/// </returns>
	bool TrySetLayoutEngineFromIndex(int nextIdx);

	/// <summary>
	/// Cycle to the next or previous layout engine.
	/// </summary>
	/// <param name="reverse">
	/// When <see langword="true"/>, activate the previous layout, otherwise activate the next layout. Defaults to <see langword="false" />.
	/// </param>
	void CycleLayoutEngine(bool reverse = false);

	/// <summary>
	/// Activates previously active layout engine.
	/// </summary>
	void ActivatePreviouslyActiveLayoutEngine();

	/// <summary>
	/// Tries to set the layout engine to one with the <c>name</c>.
	/// </summary>
	/// <param name="name">The name of the layout engine to make active.</param>
	/// <returns></returns>
	bool TrySetLayoutEngineFromName(string name);

	/// <summary>
	/// Trigger a layout.
	/// </summary>
	void DoLayout();
	#endregion

	#region Windows
	/// <summary>
	/// The windows in the workspace.
	/// </summary>
	IEnumerable<IWindow> Windows { get; }

	/// <summary>
	/// The last focused window. This is still set when the window has lost focus (provided another
	/// window in the workspace does not have focus). This is useful in cases like when the
	/// command palette is opened and wants to perform an action on the last focused window.
	///
	/// To focus the last focused window, use <see cref="FocusLastFocusedWindow"/>.
	/// </summary>
	IWindow? LastFocusedWindow { get; }

	/// <summary>
	/// Adds the window to the workspace.
	/// </summary>
	/// <remarks>
	/// Be careful of calling this outside of Whim's core code. It may cause the workspace to
	/// become out of sync with the <see cref="IButlerPantry"/>.
	///
	/// <see cref="DoLayout"/> is not called in this method.
	/// </remarks>
	/// <param name="window"></param>
	/// <returns>Whether the <paramref name="window"/> was added.</returns>
	bool AddWindow(IWindow window);

	/// <summary>
	/// Removes the window from the workspace.
	/// </summary>
	/// <remarks>
	/// Be careful of calling this outside of Whim's core code. It may cause the workspace to
	/// become out of sync with the <see cref="IButlerPantry"/>.
	///
	/// <see cref="DoLayout"/> is not called in this method.
	/// </remarks>
	/// <param name="window"></param>
	/// <returns>True when the window was removed.</returns>
	bool RemoveWindow(IWindow window);

	/// <summary>
	/// Returns true when the workspace contains the provided <paramref name="window"/>.
	/// </summary>
	/// <param name="window">The window to check for.</param>
	/// <returns>True when the workspace contains the provided <paramref name="window"/>.</returns>
	bool ContainsWindow(IWindow window);

	/// <summary>
	/// Deactivates the workspace.
	/// </summary>
	void Deactivate();

	/// <summary>
	/// Gets the current state (as of the last <see cref="DoLayout"/>) of the window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// If the window is not in the workspace, or the workspace is not active,
	/// <c>null</c> is returned.
	/// </returns>
	IWindowState? TryGetWindowState(IWindow window);

	/// <summary>
	/// If <see cref="LastFocusedWindow"/> is not <see langword="null"/> or not minimized, then we focus the
	/// last window in the workspace.
	/// </summary>
	void FocusLastFocusedWindow();

	/// <summary>
	/// Focuses the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">
	/// The origin window
	/// </param>
	/// <param name="deferLayout">
	/// Whether to defer the layout until the next <see cref="DoLayout"/>. Defaults to <c>false</c>.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool FocusWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false);

	/// <summary>
	/// Swaps the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">
	/// The window to swap. If null, the currently focused window is swapped.
	/// </param>
	/// <param name="deferLayout">
	/// Whether to defer the layout until the next <see cref="DoLayout"/>. Defaults to <c>false</c>.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool SwapWindowInDirection(Direction direction, IWindow? window = null, bool deferLayout = false);

	/// <summary>
	/// Change the <paramref name="window"/>'s <paramref name="edges"/> direction by
	/// the specified <paramref name="deltas"/>.
	/// </summary>
	/// <param name="edges">The edges to change.</param>
	/// <param name="deltas">
	/// The deltas to change the given <paramref name="edges"/> by. When a value is positive, then
	/// the edge will move in the direction of the <paramref name="edges"/>.
	/// The <paramref name="deltas"/> have a coordinate space of [0, 1] for both x and y (the unit
	/// square).
	/// </param>
	/// <param name="window">
	/// The window to change the edge of. If null, the currently focused window is
	/// used.
	/// </param>
	/// <param name="deferLayout">
	/// Whether to defer the layout until the next <see cref="DoLayout"/>. Defaults to <c>false</c>.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool MoveWindowEdgesInDirection(
		Direction edges,
		IPoint<double> deltas,
		IWindow? window = null,
		bool deferLayout = false
	);

	/// <summary>
	/// Moves or adds the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// The point has a coordinate space of [0, 1] for both x and y (the unit square).
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	/// <param name="deferLayout">
	/// Whether to defer the layout until the next <see cref="DoLayout"/>. Defaults to <c>false</c>.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool MoveWindowToPoint(IWindow window, IPoint<double> point, bool deferLayout = false);
	#endregion

	/// <summary>
	/// Called when a window is being minimized - i.e., the window size will become
	/// <see cref="WindowSize.Minimized"/>.
	///
	/// Will minimize a window in the <see cref="ActiveLayoutEngine"/>.
	/// </summary>
	/// <remarks>
	/// Be careful of calling this outside of Whim's core code. It may cause the workspace to
	/// become out of sync with the <see cref="IButlerPantry"/>.
	///
	/// <see cref="DoLayout"/> is not called in this method.
	/// </remarks>
	/// <param name="window"></param>
	void MinimizeWindowStart(IWindow window);

	/// <summary>
	/// Called when a window is being unminimized - i.e., the window size will no longer be
	/// <see cref="WindowSize.Minimized"/>.
	///
	/// Will unminimize a window in the <see cref="ActiveLayoutEngine"/>.
	/// </summary>
	/// <remarks>
	/// Be careful of calling this outside of Whim's core code. It may cause the workspace to
	/// become out of sync with the <see cref="IButlerPantry"/>.
	///
	/// <see cref="DoLayout"/> is not called in this method.
	/// </remarks>
	/// <param name="window"></param>
	/// <returns></returns>
	void MinimizeWindowEnd(IWindow window);

	#region PerformCustomLayoutEngineAction
	/// <summary>
	/// Performs a custom action in a layout engine.
	/// </summary>
	/// <remarks>
	/// Layout engines need to handle the custom action in <see cref="ILayoutEngine.PerformCustomAction{T}" />.
	/// For more, see <see cref="ILayoutEngine.PerformCustomAction{T}" />.
	/// </remarks>
	/// <param name="action">
	/// Metadata about the action to perform, and the payload to perform it with.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool PerformCustomLayoutEngineAction(LayoutEngineCustomAction action);

	/// <summary>
	/// Performs a custom action in a layout engine.
	/// </summary>
	/// <remarks>
	/// Layout engines need to handle the custom action in <see cref="ILayoutEngine.PerformCustomAction{T}" />.
	/// For more, see <see cref="ILayoutEngine.PerformCustomAction{T}" />.
	/// </remarks>
	/// <typeparam name="T">
	/// The type of <paramref name="action" />'s payload.
	/// </typeparam>
	/// <param name="action">
	/// Metadata about the action to perform, and the payload to perform it with.
	/// </param>
	/// <returns>
	/// Whether the <see cref="ActiveLayoutEngine"/> changed.
	/// </returns>
	bool PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action);
	#endregion
}
