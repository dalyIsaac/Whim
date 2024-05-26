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
	public static PurePicker<Result<ImmutableWorkspace>> PickWorkspaceById(WorkspaceId workspaceId) =>
		(IRootSector rootSector) => BaseWorkspacePicker(workspaceId, rootSector, workspace => workspace);

	/// <summary>
	/// Get all workspaces.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<IEnumerable<ImmutableWorkspace>> PickAllWorkspaces() =>
		static (IRootSector rootSector) => GetAllActiveWorkspaces(rootSector.WorkspaceSector);

	private static IEnumerable<ImmutableWorkspace> GetAllActiveWorkspaces(IWorkspaceSector workspaceSector)
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
	public static PurePicker<Result<ImmutableWorkspace>> PickWorkspaceByName(string name) =>
		(IRootSector rootSector) =>
		{
			foreach (ImmutableWorkspace workspace in rootSector.WorkspaceSector.Workspaces.Values)
			{
				if (workspace.Name == name)
				{
					return Result.FromValue(workspace);
				}
			}

			return Result.FromException<ImmutableWorkspace>(new WhimException($"Workspace with name {name} not found"));
		};

	private static Result<TResult> BaseWorkspacePicker<TResult>(
		WorkspaceId workspaceId,
		IRootSector rootSector,
		Func<ImmutableWorkspace, TResult> operation
	) =>
		rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out ImmutableWorkspace? workspace)
			? Result.FromValue(operation(workspace))
			: Result.FromException<TResult>(StoreExceptions.WorkspaceNotFound(workspaceId));

	/// <summary>
	/// Get the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<ImmutableWorkspace> PickActiveWorkspace() =>
		(IRootSector rootSector) => rootSector.WorkspaceSector.Workspaces[rootSector.WorkspaceSector.ActiveWorkspaceId];

	/// <summary>
	/// Get the id of the active workspace.
	/// </summary>
	/// <returns></returns>
	public static PurePicker<WorkspaceId> PickActiveWorkspaceId() =>
		(IRootSector rootSector) =>
			rootSector.WorkspaceSector.Workspaces[rootSector.WorkspaceSector.ActiveWorkspaceId].Id;

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

	internal static IEnumerable<IWindow> GetWorkspaceWindows(IRootSector rootSector, ImmutableWorkspace workspace)
	{
		foreach (HWND hwnd in workspace.WindowHandles)
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
			if (!rootSector.WorkspaceSector.Workspaces.TryGetValue(workspaceId, out ImmutableWorkspace? workspace))
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
