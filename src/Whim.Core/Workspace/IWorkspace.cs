using System;
using System.Collections.Generic;

namespace Whim.Core.Workspace;

/// <summary>
/// Workspaces contain windows to be organized by layout engines.
/// </summary>
public interface IWorkspace : IEnumerable<IWindow>, ICommandable
{
	/// <summary>
	/// Triggered when the workspace is renamed.
	/// </summary>
	public event EventHandler<WorkspaceRenameEventArgs> WorkspaceRenamed;

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
	#endregion

	/// <summary>
	/// The currently focused window in the workspace. <c>null</c>
	/// if no windows are focused in the workspace.
	/// </summary>
	public IWindow? FocusedWindow { get; }
}
