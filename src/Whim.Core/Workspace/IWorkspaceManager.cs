using System.Collections.Generic;

namespace Whim.Core.Workspace;

/// <summary>
/// The manager for <see cref="IWorkspace"/>s.
/// </summary>
public interface IWorkspaceManager : IEnumerable<IWorkspace>, ICommandable
{
	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace? ActiveWorkspace { get; }

	/// <summary>
	/// The <see cref="IWorkspace"/> to add.
	/// </summary>
	/// <param name="workspaces"></param>
	public void Add(IWorkspace workspace);

	/// <summary>
	/// Remove the given workspace.
	/// </summary>
	/// <param name="workspace">The workspace to remove.</param>
	public bool Remove(IWorkspace workspace);

	/// <summary>
	/// Try remove a workspace, by the workspace name.
	/// </summary>
	/// <param name="workspaceName">The workspace name to try and remove.</param>
	/// <returns><c>true</c> when the workspace has been removed.</returns>
	public bool Remove(string workspaceName);
}
