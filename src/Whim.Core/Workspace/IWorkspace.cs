using System;
using System.Collections.Generic;

namespace Whim.Core;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace : ICommandable
{
	/// <summary>
	/// Triggered when the workspace is renamed.
	/// </summary>
	public event EventHandler<WorkspaceRenameEventArgs>? WorkspaceRenamed;

	/// <summary>
	/// The name of the workspace. When the <c>Name</c> is set, the
	/// <see cref="WorkspaceRenamed"/> event is triggered.
	/// </summary>
	public string Name { get; set; }

	#region Layout engine
	/// <summary>
	/// The active layout engine.
	/// </summary>
	public ILayoutEngine ActiveLayoutEngine { get; }

	/// <summary>
	/// Event for when the active layout engine has changed.
	/// </summary>
	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	// Methods to add and remove layout engines

	/// <summary>
	/// Adds a layout engine to the workspace.
	/// </summary>
	/// <param name="layoutEngine">The layout engine to add.</param>
	public void AddLayoutEngine(ILayoutEngine layoutEngine);

	/// <summary>
	/// Removes a layout engine from the workspace.
	/// </summary>
	/// <param name="layoutEngine">The layout engine to remove.</param>
	public bool RemoveLayoutEngine(ILayoutEngine layoutEngine);

	/// <summary>
	/// Removes the layout engine with name <paramref name="name"/> from the workspace.
	/// </summary>
	/// <param name="name">The name of the layout engine to remove.</param>
	public bool RemoveLayoutEngine(string name);

	/// <summary>
	/// Event for when a layout engine has been added to the workspace.
	/// </summary>
	public event EventHandler<LayoutEngineEventArgs>? LayoutEngineAdded;

	/// <summary>
	/// Event for when a layout engine has been removed from the workspace.
	/// </summary>
	public event EventHandler<LayoutEngineEventArgs>? LayoutEngineRemoved;

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
	/// Adds the window to the workspace.
	/// </summary>
	/// <param name="window"></param>
	public void AddWindow(IWindow window);

	/// <summary>
	/// Removes the window from the workspace.
	/// </summary>
	/// <param name="window"></param>
	public bool RemoveWindow(IWindow window);
	#endregion
}
