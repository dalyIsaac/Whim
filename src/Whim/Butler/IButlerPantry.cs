using System.Collections.Generic;

namespace Whim;

/// <summary>
/// The Butler's pantry is responsible for mapping <see cref="IWindow"/>s to <see cref="IWorkspace"/>s
/// to <see cref="IMonitor"/>s.
/// </summary>
public interface IButlerPantry
{
	/// <summary>
	/// Gets the adjacent workspace to the given workspace.
	/// </summary>
	/// <param name="workspace">
	/// The workspace to get the adjacent workspace for.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous workspace, otherwise gets the next workspace. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	/// <returns></returns>
	IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false);

	/// <summary>
	/// Gets all the workspaces which are active on any monitor.
	/// </summary>
	/// <returns></returns>
	IEnumerable<IWorkspace> GetAllActiveWorkspaces();

	/// <summary>
	/// Retrieves the monitor for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns><see langword="null"/> if the window is not in a workspace.</returns>
	IMonitor? GetMonitorForWindow(IWindow window);

	/// <summary>
	/// Retrieves the monitor for the active workspace.
	/// </summary>
	/// <param name="workspace"></param>
	/// <returns><see langword="null"/> if the workspace is not active.</returns>
	IMonitor? GetMonitorForWorkspace(IWorkspace workspace);

	/// <summary>
	/// Retrieves the active workspace for the given monitor.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns><see langword="null"/> if the monitor is not active.</returns>
	IWorkspace? GetWorkspaceForMonitor(IMonitor monitor);

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns><see langword="null"/> if the window is not in a workspace.</returns>
	IWorkspace? GetWorkspaceForWindow(IWindow window);
}
