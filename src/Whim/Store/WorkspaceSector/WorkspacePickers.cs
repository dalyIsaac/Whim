using System;
using System.Collections.Generic;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

public static partial class Pickers
{
	/// <summary>
	/// Get the workspace with the provided <paramref name="workspaceId"/>.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceById(WorkspaceId workspaceId) =>
		(IRootSector rootSector) => BaseWorkspacePicker(workspaceId, rootSector, workspace => workspace);

	/// <summary>
	/// Get all workspaces.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<IEnumerable<IWorkspace>> PickAllWorkspaces() =>
		static (IRootSector rootSector) => GetAllActiveWorkspaces(rootSector.WorkspaceSector);

	private static IEnumerable<IWorkspace> GetAllActiveWorkspaces(IWorkspaceSector workspaceSector)
	{
		foreach (WorkspaceId id in workspaceSector.WorkspaceOrder)
		{
			yield return workspaceSector.Workspaces[id];
		}
	}

	/// <summary>
	/// Get the workspace with the provided <paramref name="name"/>.
	/// </summary>
	/// <param name="name"></param>
	public static PurePicker<Result<IWorkspace>> PickWorkspaceByName(string name) =>
		(IRootSector rootSector) =>
		{
			foreach (IWorkspace workspace in rootSector.WorkspaceSector.Workspaces.Values)
			{
				if (workspace.Name == name)
				{
					return Result.FromValue(workspace);
				}
			}

			return Result.FromException<IWorkspace>(new WhimException($"Workspace with name {name} not found"));
		};

	/// <summary>
	/// Base picker to get something from a workspace, provided by <paramref name="operation"/>.
	/// </summary>
	/// <param name="workspaceId">The id of the workspace to get something from.</param>
	/// <param name="rootSector">The root sector.</param>
	/// <param name="operation">The operation to determine what to get.</param>
	/// <typeparam name="TResult">The result.</typeparam>
	/// <returns></returns>
	private static Result<TResult> BaseWorkspacePicker<TResult>(
		WorkspaceId workspaceId,
		IRootSector rootSector,
		Func<IWorkspace, TResult> operation
	) =>
		rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace)
			? Result.FromValue(operation(workspace))
			: Result.FromException<TResult>(StoreExceptions.WorkspaceNotFound(workspaceId));

	/// <summary>
	/// Get the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<IWorkspace> PickActiveWorkspace() =>
		static (IRootSector rootSector) =>
			rootSector.WorkspaceSector.Workspaces[
				rootSector.MapSector.MonitorWorkspaceMap[rootSector.MonitorSector.ActiveMonitorHandle]
			];

	/// <summary>
	/// Get the id of the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<WorkspaceId> PickActiveWorkspaceId() =>
		static (IRootSector rootSector) =>
			rootSector.MapSector.MonitorWorkspaceMap[rootSector.MonitorSector.ActiveMonitorHandle];

	/// <summary>
	/// Get the active layout engine in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<ILayoutEngine>> PickActiveLayoutEngine(WorkspaceId workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(
				workspaceId,
				rootSector,
				static workspace => workspace.LayoutEngines[workspace.ActiveLayoutEngineIndex]
			);

	/// <summary>
	/// Get all the windows in the provided workspace.
	/// </summary>
	/// <param name="workspaceId"></param>
	public static PurePicker<Result<IEnumerable<IWindow>>> PickAllWindowsInWorkspace(WorkspaceId workspaceId) =>
		(IRootSector rootSector) =>
			BaseWorkspacePicker(workspaceId, rootSector, workspace => GetWorkspaceWindows(rootSector, workspace));

	internal static IEnumerable<IWindow> GetWorkspaceWindows(IRootSector rootSector, IWorkspace workspace)
	{
		foreach (HWND hwnd in workspace.WindowPositions.Keys)
		{
			if (PickWindowByHandle(hwnd)(rootSector).TryGet(out IWindow window))
			{
				yield return window;
			}
		}
	}

	/// <summary>
	/// Get the last focused window in the provided workspace.
	/// </summary>
	/// <param name="workspaceId">The workspace to get the last focused window for.</param>
	public static PurePicker<Result<IWindow>> PickLastFocusedWindow(WorkspaceId workspaceId) =>
		(IRootSector rootSector) =>
		// This doesn't use BaseWorkspacePicker because it uses the Result from PickWindowByHandle.
		{
			if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out Workspace? workspace))
			{
				return Result.FromException<IWindow>(StoreExceptions.WorkspaceNotFound(workspaceId));
			}

			if (workspace.LastFocusedWindowHandle.IsNull)
			{
				return Result.FromException<IWindow>(new WhimException("No last focused window in workspace"));
			}

			return PickWindowByHandle(workspace.LastFocusedWindowHandle)(rootSector);
		};
}
