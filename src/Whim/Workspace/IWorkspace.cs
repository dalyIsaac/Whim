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

	/// <summary>
	/// Initializes the workspace. This includes wrapping its layout engines with
	/// proxies from <see cref="IWorkspaceManager"/>.
	/// </summary>
	void Initialize();

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
	/// The currently focused window.
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
	/// Gets the current location (as of the last <see cref="DoLayout"/>) of the window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns>
	/// If the window is not in the workspace, or the workspace is not focused,
	/// <c>null</c> is returned.
	/// </returns>
	IWindowState? TryGetWindowLocation(IWindow window);

	/// <summary>
	/// Focuses on the first window in the workspace.
	/// </summary>
	void FocusFirstWindow();

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
	/// Change the <paramref name="window"/>'s <paramref name="edge"/> direction by
	/// the specified <paramref name="delta"/>.
	/// </summary>
	/// <param name="edge">The edge to change.</param>
	/// <param name="delta">The percentage to change the edge by.</param>
	/// <param name="window">
	/// The window to change the edge of. If null, the currently focused window is
	/// used.
	/// </param>
	void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow? window = null);

	/// <summary>
	/// Moves or adds the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	void MoveWindowToPoint(IWindow window, IPoint<double> point);
	#endregion

	#region Phantom Windows
	/// <summary>
	/// Add a phantom window. This can only be done by the active layout engine.
	/// </summary>
	/// <remarks>
	/// Phantom windows are placeholder windows that represent a space in the
	/// current layout engine.
	///
	/// They are designed to let a layout engine reserve a space, for a new window,
	/// or for a window that is being moved around.
	/// </remarks>
	/// <param name="engine">The layout engine to add the phantom window to.</param>
	/// <param name="window">The phantom window to add.</param>
	void AddPhantomWindow(ILayoutEngine engine, IWindow window);

	/// <summary>
	/// Remove a phantom window. This can only be done by the active layout engine,
	/// and the phantom window must have been added to the same layout engine.
	/// </summary>
	/// <param name="engine">The layout engine to remove the phantom window from.</param>
	/// <param name="window">The phantom window to remove.</param>
	void RemovePhantomWindow(ILayoutEngine engine, IWindow window);
	#endregion
}
