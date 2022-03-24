using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace : ICommandable
{
	/// <summary>
	/// The name of the workspace. When the <c>Name</c> is set, the
	/// <see cref="IWorkspaceManager.WorkspaceRenamed"/> event is triggered.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Initializes the workspace. This includes wrapping its layout engines with
	/// proxies from <see cref="IWorkspaceManager"/>.
	/// </summary>
	public void Initialize();

	#region Layout engine
	/// <summary>
	/// The active layout engine.
	/// </summary>
	public ILayoutEngine ActiveLayoutEngine { get; }

	/// <summary>
	/// Rotate to the next layout engine.
	/// </summary>
	public void NextLayoutEngine();

	/// <summary>
	/// Rotate to the previous layout engine.
	/// </summary>
	public void PreviousLayoutEngine();

	/// <summary>
	/// Tries to set the layout engine to one with the <c>name</c>.
	/// </summary>
	/// <param name="name">The name of the layout engine to make active.</param>
	/// <returns></returns>
	public bool TrySetLayoutEngine(string name);

	/// <summary>
	/// Trigger a layout.
	/// </summary>
	public void DoLayout();
	#endregion

	#region Windows
	public IEnumerable<IWindow> Windows { get; }

	/// <summary>
	/// The currently focused window.
	/// </summary>
	public IWindow? LastFocusedWindow { get; }

	/// <summary>
	/// Adds the window to the workspace.
	/// </summary>
	/// <param name="window"></param>
	public void AddWindow(IWindow window);

	/// <summary>
	/// Removes the window from the workspace.
	/// </summary>
	/// <param name="window"></param>
	public bool RemoveWindow(IWindow window);

	/// <summary>
	/// Indicates whether the workspace contains the window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public bool ContainsWindow(IWindow window);

	/// <summary>
	/// Deactivates the workspace.
	/// </summary>
	public void Deactivate();

	/// <summary>
	/// Focuses on the first window in the workspace.
	/// </summary>
	public void FocusFirstWindow();

	/// <summary>
	/// Focuses the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">
	/// The origin window
	/// </param>
	public void FocusWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Swaps the <paramref name="window"/> in the <paramref name="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">
	/// The window to swap. If null, the currently focused window is swapped.
	/// </param>
	public void SwapWindowInDirection(Direction direction, IWindow? window = null);

	/// <summary>
	/// Change the <paramref name="window"/>'s edge <paramref name="direction"/> by
	/// the specified <paramref name="fractionDelta"/>.
	/// </summary>
	/// <param name="edge">The edge to change.</param>
	/// <param name="fractionDelta">The percentage to change the edge by.</param>
	/// <param name="window">
	/// The window to change the edge of. If null, the currently focused window is
	/// used.
	/// </param>
	public void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow? window = null);

	/// <summary>
	/// Moves the given <paramref name="window"/> to the given <paramref name="point"/>.
	/// </summary>
	/// <param name="window">The window to move.</param>
	/// <param name="point">The point to move the window to.</param>
	/// <param name="isPhantom">Indicates whether the window being moved is a phantom window.</param>
	public void MoveWindowToPoint(IWindow window, IPoint<double> point, bool isPhantom = false);
	#endregion

	#region Phantom Windows
	/// <summary>
	/// Register a phantom window. This can only be done by the active layout engine.
	/// </summary>
	/// <param name="engine">The layout engine to register the phantom window to.</param>
	/// <param name="window">The phantom window to register.</param>
	public void RegisterPhantomWindow(ILayoutEngine engine, IWindow window);

	/// <summary>
	/// Unregister a phantom window. This can only be done by the active layout engine,
	/// and the phantom window must be registered to the same layout engine.
	/// </summary>
	/// <param name="engine">The layout engine to unregister the phantom window from.</param>
	/// <param name="window">The phantom window to unregister.</param>
	public void UnregisterPhantomWindow(ILayoutEngine engine, IWindow window, bool doLayout = false);
	#endregion
}
