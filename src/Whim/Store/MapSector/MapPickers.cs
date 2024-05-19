using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotNext;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

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
	public static Picker<IEnumerable<IWorkspace>> PickAllActiveWorkspaces() => new GetAllActiveWorkspacesPicker();

	/// <summary>
	/// Retrieves the workspace for the given monitor.
	/// </summary>
	/// <param name="monitorHandle"></param>
	/// <returns></returns>
	public static Picker<Result<IWorkspace>> PickWorkspaceForMonitor(HMONITOR monitorHandle) =>
		new GetWorkspaceForMonitorPicker(monitorHandle);

	/// <summary>
	/// Retrieves the workspace for the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public static Picker<Result<IWorkspace>> PickWorkspaceForWindow(HWND window) =>
		new GetWorkspaceForWindowPicker(window);

	/// <summary>
	/// Retrieves the monitor for the given workspace.
	/// </summary>
	/// <param name="searchWorkspaceId">
	/// The ID of the workspace to get the monitor for.
	/// </param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> PickMonitorForWorkspace(WorkspaceId searchWorkspaceId) =>
		(IRootSector rootSector) =>
		{
			HMONITOR monitorHandle = rootSector.MapSector.GetMonitorForWorkspace(searchWorkspaceId);

			if (monitorHandle == default)
			{
				return Result.FromException<IMonitor>(StoreExceptions.NoMonitorFoundForWorkspace(searchWorkspaceId));
			}

			return PickMonitorByHandle(monitorHandle)(rootSector);
		};

	/// <summary>
	/// Retrieves the monitor for the given window.
	/// </summary>
	/// <param name="windowHandle"></param>
	/// <returns></returns>
	public static PurePicker<Result<IMonitor>> PickMonitorForWindow(HWND windowHandle) =>
		(IRootSector rootSector) =>
		{
			if (rootSector.MapSector.WindowWorkspaceMap.TryGetValue(windowHandle, out WorkspaceId workspaceId))
			{
				return PickMonitorForWorkspace(workspaceId)(rootSector);
			}

			return Result.FromException<IMonitor>(StoreExceptions.NoMonitorFoundForWindow(windowHandle));
		};

	/// <summary>
	/// Gets the adjacent workspace for the given workspace.
	/// </summary>
	/// <param name="workspaceId">
	/// The workspace to get the adjacent workspace for.
	/// </param>
	/// <param name="reverse">
	/// When <see langword="true"/>, gets the previous workspace, otherwise gets the next workspace. Defaults to <see langword="false" />.
	/// </param>
	/// <param name="skipActive">
	/// When <see langword="true"/>, skips all workspaces that are active on any other monitor. Defaults to <see langword="false"/>.
	/// </param>
	/// <returns></returns>
	public static Picker<Result<IWorkspace>> PickAdjacentWorkspace(
		WorkspaceId workspaceId,
		bool reverse = false,
		bool skipActive = false
	) => new GetAdjacentWorkspacePicker(workspaceId, reverse, skipActive);
}

internal record GetAllActiveWorkspacesPicker : Picker<IEnumerable<IWorkspace>>
{
	internal override IEnumerable<IWorkspace> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		IRootSector rootSector
	)
	{
		WorkspaceId[] activeWorkspaceIds = rootSector.MapSector.MonitorWorkspaceMap.Values.ToArray();
		foreach (IWorkspace workspace in ctx.WorkspaceManager)
		{
			if (activeWorkspaceIds.Contains(workspace.Id))
			{
				yield return workspace;
			}
		}
	}
}

internal record GetWorkspaceForMonitorPicker(HMONITOR MonitorHandle) : Picker<Result<IWorkspace>>
{
	internal override Result<IWorkspace> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		if (!rootSector.MapSector.MonitorWorkspaceMap.TryGetValue(MonitorHandle, out WorkspaceId workspaceId))
		{
			return Result.FromException<IWorkspace>(StoreExceptions.MonitorNotFound(MonitorHandle));
		}

		return ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));
	}
}

internal record GetWorkspaceForWindowPicker(HWND WindowHandle) : Picker<Result<IWorkspace>>
{
	internal override Result<IWorkspace> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		WorkspaceId workspaceId = rootSector.MapSector.WindowWorkspaceMap[WindowHandle];
		return ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));
	}
}

internal record GetAdjacentWorkspacePicker(WorkspaceId WorkspaceId, bool Reverse, bool SkipActive)
	: Picker<Result<IWorkspace>>
{
	internal override Result<IWorkspace> Execute(IContext ctx, IInternalContext internalCtx, IRootSector rootSector)
	{
		IWorkspace[] workspaces = ctx.WorkspaceManager.ToArray();

		bool found = false;
		int idx = 0;
		foreach (IWorkspace workspace in workspaces)
		{
			if (workspace.Id == WorkspaceId)
			{
				found = true;
				break;
			}
			idx++;
		}

		if (!found)
		{
			return Result.FromException<IWorkspace>(StoreExceptions.WorkspaceNotFound(WorkspaceId));
		}

		int delta = Reverse ? -1 : 1;
		int nextIdx = (idx + delta).Mod(workspaces.Length);
		while (idx != nextIdx)
		{
			IWorkspace nextWorkspace = workspaces[nextIdx];
			Result<IMonitor> monitorResult = Pickers.PickMonitorForWorkspace(nextWorkspace.Id)(rootSector);

			if (!monitorResult.IsSuccessful || !SkipActive)
			{
				return Result.FromValue(nextWorkspace);
			}

			nextIdx = (nextIdx + delta).Mod(workspaces.Length);
		}

		return Result.FromException<IWorkspace>(new WhimException($"No adjacent workspace found to {WorkspaceId}"));
	}
}
