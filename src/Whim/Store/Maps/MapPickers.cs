using System.Collections.Generic;
using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Pure pickers for <see cref="IMapSector"/>.
/// </summary>
public static partial class Pickers
{
	/// <summary>
	/// Gets all the workspaces which are active on any monitor.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<IEnumerable<IWorkspace>> GetAllActiveWorkspaces =>
		static (IRootSector rootSector) => rootSector.Maps.MonitorWorkspaceMap.Values;

	/// <summary>
	/// Retrieves the workspace for the given monitor.
	/// </summary>
	/// <param name="monitor"></param>
	/// <returns></returns>
	public static PurePicker<Result<IWorkspace>> GetWorkspaceForMonitor(IMonitor monitor) =>
		(IRootSector rootSector) =>
			rootSector.Maps.MonitorWorkspaceMap.TryGetValue(monitor, out IWorkspace? workspace)
				? Result.FromValue(workspace)
				: Result.FromException<IWorkspace>(new WhimException("No workspace found for given monitor."));

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static PurePicker<Result<IWorkspace>> GetWorkspaceForWindow(IWindow window) =>
		(IRootSector rootSector) =>
			rootSector.Maps.WindowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace)
				? Result.FromValue(workspace)
				: Result.FromException<IWorkspace>(new WhimException("No workspace found for given window."));

	/// <summary>
	/// Retrieves the monitor for the given workspace.
	/// </summary>
	/// <param name="searchWorkspace">
	/// The workspace to get the monitor for.
	/// </param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> GetMonitorForWorkspace(IWorkspace searchWorkspace) =>
		(IRootSector rootSector) =>
		{
			foreach ((IMonitor monitor, IWorkspace workspace) in rootSector.Maps.MonitorWorkspaceMap)
			{
				if (workspace.Equals(searchWorkspace))
				{
					return Result.FromValue(monitor);
				}
			}

			return Result.FromException<IMonitor>(new WhimException("No monitor found for given workspace."));
		};

	/// <summary>
	///
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> GetMonitorForWindow(IWindow window) =>
		(IRootSector rootSector) =>
		{
			if (GetWorkspaceForWindow(window)(rootSector).TryGet(out IWorkspace? workspace))
			{
				return GetMonitorForWorkspace(workspace)(rootSector);
			}

			return Result.FromException<IMonitor>(new WhimException("No monitor found for given window."));
		};

	/// <summary>
	/// Gets the adjacent workspace for the given workspace.
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
	public static PurePicker<Result<IWorkspace>> GetAdjacentWorkspace(
		IWorkspace workspace,
		bool reverse = false,
		bool skipActive = false
	) =>
		(IRootSector rootSector) =>
		{
			ImmutableList<IWorkspace> workspaces = rootSector.Workspaces.Workspaces;

			int idx = workspaces.IndexOf(workspace);
			int delta = reverse ? -1 : 1;
			int nextIdx = (idx + delta).Mod(workspaces.Count);

			while (idx != nextIdx)
			{
				IWorkspace nextWorkspace = workspaces[nextIdx];
				Result<IMonitor> monitorResult = GetMonitorForWorkspace(nextWorkspace)(rootSector);

				if (!monitorResult.IsSuccessful || !skipActive)
				{
					return Result.FromValue(nextWorkspace);
				}

				nextIdx = (nextIdx + delta).Mod(workspaces.Count);
			}

			return Result.FromException<IWorkspace>(new WhimException("No adjacent workspace found."));
		};
}
