using System;
using System.Linq;
using DotNext;

namespace Whim;

/// <summary>
/// Activates the given workspace in the active monitor, or the given monitor (if provided).
/// </summary>
/// <param name="Workspace">
/// The workspace to activate.
/// </param>
/// <param name="Monitor">
/// The monitor to activate the workspace in. If <see langword="null"/>, this will default to
/// the active monitor.
/// </param>
public record ActivateWorkspaceTransform(IWorkspace Workspace, IMonitor? Monitor = null) : Transform
{
	internal override Result<Empty> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		if (!ctx.WorkspaceManager.Contains(Workspace))
		{
			return Result.FromException<Empty>(new InvalidOperationException($"Workspace {Workspace} is not tracked"));
		}

		IMonitor? targetMonitor = Monitor;
		if (targetMonitor is null)
		{
			targetMonitor = ctx.MonitorManager.ActiveMonitor;
		}
		else if (!ctx.Store.Pick(new GetAllMonitorsPicker()).Contains(targetMonitor))
		{
			return Result.FromException<Empty>(
				new InvalidOperationException($"Monitor {targetMonitor} is not tracked")
			);
		}

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = ctx.Store.Pick(MapPickers.GetWorkspaceForMonitor(targetMonitor)).OrDefault();

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = ctx.Store.Pick(MapPickers.GetMonitorForWorkspace(Workspace)).OrDefault();

		if (targetMonitor.Equals(loserMonitor))
		{
			Logger.Debug("Workspace is already activated");
			return Empty.Result;
		}

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		ctx.Butler.Pantry.SetMonitorWorkspace(targetMonitor, Workspace);

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && !loserMonitor.Equals(targetMonitor))
		{
			ctx.Butler.Pantry.SetMonitorWorkspace(loserMonitor, oldWorkspace);
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			rootSector.Maps.QueueEvent(
				new MonitorWorkspaceChangedEventArgs()
				{
					Monitor = oldWorkspaceValue.monitor,
					PreviousWorkspace = Workspace,
					CurrentWorkspace = oldWorkspaceValue.workspace
				}
			);
		}
		else
		{
			oldWorkspace?.Deactivate();

			// Temporarily focus the monitor's desktop HWND, to prevent another window from being focused.
			ctx.Store.Dispatch(new FocusMonitorDesktopTransform(targetMonitor));
		}

		// Layout the new workspace.
		Workspace.DoLayout();
		Workspace.FocusLastFocusedWindow();

		rootSector.Maps.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = targetMonitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = Workspace
			}
		);

		return Empty.Result;
	}
}
