using System.Collections;
using System.Collections.Generic;

namespace Whim.Core.Workspace;

/// <summary>
/// Implementation of <see cref="IWorkspaceManager"/>.
/// </summary>
public class WorkspaceManager : IWorkspaceManager
{
	public Commander Commander { get; } = new();

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	private readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace? ActiveWorkspace { get; private set; }

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public void Add(IWorkspace workspace)
	{
		_workspaces.Add(workspace);
	}

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool Remove(IWorkspace workspace) {
		Logger.Debug("Removing workspace {0}", workspace.Name);
		return _workspaces.Remove(workspace);
	}

	public bool Remove(string workspaceName)
	{
		Logger.Debug("Removing workspace {0}", workspaceName);
		IWorkspace? workspace = _workspaces.Find(w => w.Name == workspaceName);
		return workspace != null && _workspaces.Remove(workspace);
	}

	public IWorkspace? TryGet(string workspaceName)
	{
		Logger.Debug("Trying to get workspace {0}", workspaceName);
		return _workspaces.Find(w => w.Name == workspaceName);
	}
}
