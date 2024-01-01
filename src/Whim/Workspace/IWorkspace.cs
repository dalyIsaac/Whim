using System;
using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace : IDisposable
{
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
	void NextLayoutEngine();

	/// <summary>
	/// Rotate to the previous layout engine.
	/// </summary>
	void PreviousLayoutEngine();

	/// <summary>
	/// Tries to set the layout engine to one with the <c>name</c>.
	/// </summary>
	/// <param name="name">The name of the layout engine to make active.</param>
	/// <returns></returns>
	bool TrySetLayoutEngine(string name);

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
	/// <param name="window"></param>
	void AddWindow(IWindow window);

	/// <summary>
	/// Removes the window from the workspace.
	/// </summary>
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
	/// Focuses on the first window in the workspace.
	/// </summary>
	[Obsolete("Use FocusLastFocusedWindow instead.")]
	void FocusFirstWindow();

	/// <summary>
	/// Focuses on the last window in the workspace. If <see cref="LastFocusedWindow"/> is null,
	/// then we try focus the first window. If there are no windows, then we focus the Windows
	/// desktop window.
	/// </summary>
	void FocusLastFocusedWindow();

	/// <summary>
	/// Focuses the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">
	/// The origin window
	/// </param>
	void FocusWindowInDirection(Direction direction, IWindow? window = null);

	/// <summary>
	/// Swaps the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">
	/// The window to swap. If null, the currently focused window is swapped.
	/// </param>
	void SwapWindowInDirection(Direction direction, IWindow? window = null);

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
	void MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow? window = null);

	/// <summary>
	/// Moves or adds the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// The point has a coordinate space of [0, 1] for both x and y (the unit square).
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	void MoveWindowToPoint(IWindow window, IPoint<double> point);
	#endregion

	#region MinimizeWindow
	/// <summary>
	/// Called when a window is being minimized - i.e., the window size will become
	/// <see cref="WindowSize.Minimized"/>.
	///
	/// Will minimize a window in the <see cref="ActiveLayoutEngine"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	void MinimizeWindowStart(IWindow window);

	/// <summary>
	/// Called when a window is being unminimized - i.e., the window size will no longer be
	/// <see cref="WindowSize.Minimized"/>.
	///
	/// Will unminimize a window in the <see cref="ActiveLayoutEngine"/>.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	void MinimizeWindowEnd(IWindow window);
	#endregion

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
	void PerformCustomLayoutEngineAction(LayoutEngineCustomAction action);

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
	void PerformCustomLayoutEngineAction<T>(LayoutEngineCustomAction<T> action);
	#endregion
}
