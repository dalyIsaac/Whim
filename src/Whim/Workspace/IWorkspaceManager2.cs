using System.Collections.Generic;

namespace Whim;

// TODO: Order
/// <summary>
/// Container responsible for mapping <see cref="IWorkspace"/>s to <see cref="IMonitor"/>s and
/// <see cref="IWindow"/>s.
///
/// It is responsible for the creation and destruction of <see cref="IWorkspace"/>s.
/// </summary>
public interface IWorkspaceManager2 : IEnumerable<IWorkspace>
{
	void Initialize();

	IWorkspace? this[string workspaceName] { get; }

	IWorkspace? TryGet(string workspaceName);

	IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null);

	bool Remove(IWorkspace workspace);

	bool Remove(string workspaceName);

	bool Contains(IWorkspace workspace);

	bool RemoveWorkspace(IWorkspace workspace);

	bool RemoveWindow(IWindow window);

	bool RemoveMonitor(IMonitor monitor);

	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	void SetWorkspaceMonitor(IWorkspace workspace, IMonitor monitor);

	void SetWindowWorkspace(IWindow window, IWorkspace workspace);

	IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse, bool skipActive);

	IWorkspace? GetWorkspaceForWindow(IWindow window);
}
