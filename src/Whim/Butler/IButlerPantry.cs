using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The Butler's pantry is responsible for mapping <see cref="IWindow"/>s to <see cref="IWorkspace"/>s
/// to <see cref="IMonitor"/>s.
/// </summary>
internal interface IButlerPantry
{
	bool RemoveWindow(IWindow window);

	bool RemoveMonitor(IMonitor monitor);

	void RemoveWorkspace(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget);

	IEnumerable<IWorkspace> GetAllActiveWorkspaces();

	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	IWorkspace? GetWorkspaceForWindow(IWindow window);

	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	IMonitor? GetMonitorForWindow(IWindow window);

	void SetMonitorWorkspace(IMonitor monitor, IWorkspace workspace);

	void SetWindowWorkspace(IWindow window, IWorkspace workspace);
}
