using DotNext;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

/// <summary>
/// Activates the given workspace in the active monitor, or the given monitor (if provided).
/// </summary>
/// <param name="WorkspaceId">
/// The id of the workspace to activate.
/// </param>
/// <param name="MonitorHandle">
/// The handle of the monitor to activate the workspace in. If <see langword="null"/>, this will
/// default to the active monitor.
/// </param>
public record ActivateWorkspaceTransform(WorkspaceId WorkspaceId, HMONITOR MonitorHandle = default) : Transform
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		MapSector mapSector = rootSector.MapSector;

		Result<IWorkspace> workspaceResult = ctx.Store.Pick(Pickers.PickWorkspaceById(WorkspaceId));
		if (!workspaceResult.TryGet(out IWorkspace workspace))
		{
			return Result.FromException<Unit>(workspaceResult.Error!);
		}

		HMONITOR targetMonitorHandle = MonitorHandle;
		if (targetMonitorHandle == default)
		{
			targetMonitorHandle = rootSector.MonitorSector.ActiveMonitorHandle;
		}

		Result<IMonitor> targetMonitorResult = ctx.Store.Pick(Pickers.PickMonitorByHandle(targetMonitorHandle));
		if (!targetMonitorResult.TryGet(out IMonitor targetMonitor))
		{
			return Result.FromException<Unit>(targetMonitorResult.Error!);
		}

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = ctx.Store.Pick(Pickers.PickWorkspaceByMonitor(targetMonitorHandle)).OrDefault();

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = ctx.Store.Pick(Pickers.PickMonitorByWorkspace(WorkspaceId)).OrDefault();

		if (targetMonitorHandle == loserMonitor?.Handle)
		{
			Logger.Debug("Workspace is already activated");
			return Unit.Result;
		}

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(targetMonitorHandle, WorkspaceId);

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && loserMonitor.Handle != targetMonitorHandle)
		{
			mapSector.MonitorWorkspaceMap = mapSector.MonitorWorkspaceMap.SetItem(loserMonitor.Handle, oldWorkspace.Id);
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			mapSector.QueueEvent(
				new MonitorWorkspaceChangedEventArgs()
				{
					Monitor = oldWorkspaceValue.monitor,
					PreviousWorkspace = workspace,
					CurrentWorkspace = oldWorkspaceValue.workspace
				}
			);
		}
		else
		{
			oldWorkspace?.Deactivate();

			// Temporarily focus the monitor's desktop HWND, to prevent another window from being focused.
			ctx.Store.Dispatch(new FocusMonitorDesktopTransform(targetMonitorHandle));
		}

		// Layout the new workspace.
		workspace.DoLayout();
		workspace.FocusLastFocusedWindow();

		mapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = targetMonitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace
			}
		);

		return Unit.Result;
	}
}
