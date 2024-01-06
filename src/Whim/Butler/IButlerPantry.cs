using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The Butler's pantry is responsible for mapping <see cref="IWindow"/>s to <see cref="IWorkspace"/>s
/// to <see cref="IMonitor"/>s.
/// </summary>
internal interface IButlerPantry
{
	IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false);

	IEnumerable<IWorkspace> GetAllActiveWorkspaces();

	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	IMonitor? GetMonitorForWindow(IWindow window);

	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	IWorkspace? GetWorkspaceForWindow(IWindow window);

	void MergeWorkspaceWindows(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget);

	bool RemoveWindow(IWindow window);

	bool RemoveMonitor(IMonitor monitor);

	void SetMonitorWorkspace(IMonitor monitor, IWorkspace workspace);

	void SetWindowWorkspace(IWindow window, IWorkspace workspace);
}
