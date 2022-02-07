using System;
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
	public IWindow? FocusedWindow { get; }

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
	/// Deactivates the workspace.
	/// </summary>
	public void Deactivate();

	/// <summary>
	/// Focuses on the first window in the workspace.
	/// </summary>
	public void FocusFirstWindow();

	/// <summary>
	/// Focuses the <see paramref="window"/> in the <see paramref="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">
	/// The origin window
	/// </param>
	public void FocusWindowInDirection(Direction direction, IWindow window);

	/// <summary>
	/// Swaps the <see paramref="window"/> in the <see paramref="direction"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">
	/// The window to swap. If null, the currently focused window is swapped.
	/// </param>
	public void SwapWindowInDirection(Direction direction, IWindow? window = null);
	#endregion
}
