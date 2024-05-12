using System.Linq;
using DotNext;
using Windows.Win32.Foundation;

namespace Whim;

internal class ButlerChores : IButlerChores
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public ButlerChores(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public void Activate(IWorkspace workspace, IMonitor? monitor = null)
	{
		Logger.Debug($"Activating workspace {workspace} in monitor {monitor}");

		if (!_context.WorkspaceManager.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} is not tracked");
			return;
		}

		if (monitor is null)
		{
			monitor = _context.Store.Pick(Pickers.GetActiveMonitor());
		}
		else if (!_context.Store.Pick(Pickers.GetMonitorByHandle(monitor.Handle)).IsSuccessful)
		{
			Logger.Error($"Workspace {monitor} is not tracked");
			return;
		}

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor);

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = _context.Butler.Pantry.GetMonitorForWorkspace(workspace);

		if (monitor.Handle == loserMonitor?.Handle)
		{
			Logger.Debug("Workspace is already activated");
			return;
		}

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		_context.Butler.Pantry.SetMonitorWorkspace(monitor, workspace);

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && loserMonitor.Handle != monitor.Handle)
		{
			_context.Butler.Pantry.SetMonitorWorkspace(loserMonitor, oldWorkspace);
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			_internalContext.Butler.TriggerMonitorWorkspaceChanged(
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
			FocusMonitorDesktop(monitor);
		}

		// Layout the new workspace.
		workspace.DoLayout();
		workspace.FocusLastFocusedWindow();
		_internalContext.Butler.TriggerMonitorWorkspaceChanged(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace
			}
		);
	}

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false)
	{
		Logger.Debug("Activating next workspace");

		monitor ??= _context.Store.Pick(Pickers.GetActiveMonitor());
		IWorkspace? currentWorkspace = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor);
		if (currentWorkspace == null)
		{
			Logger.Debug($"No workspace found for monitor {monitor}");
			return;
		}

		IWorkspace? nextWorkspace = _context.Butler.Pantry.GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
		if (nextWorkspace == null)
		{
			Logger.Debug($"No next workspace found for monitor {monitor}");
			return;
		}

		Activate(nextWorkspace, monitor);
	}

	public void LayoutAllActiveWorkspaces()
	{
		Logger.Debug("Layout all active workspaces");

		// For each workspace which is active in a monitor, do a layout.
		// Convert to an array to prevent enumeration modification during execution.
		IWorkspace[] workspaces = _context.Butler.Pantry.GetAllActiveWorkspaces().ToArray();
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.DoLayout();
		}
	}

	public void FocusMonitorDesktop(IMonitor monitor)
	{
		HWND desktop = _internalContext.CoreNativeManager.GetDesktopWindow();
		_internalContext.CoreNativeManager.SetForegroundWindow(desktop);
		_internalContext.WindowManager.OnWindowFocused(null);
		_context.Store.Dispatch(new ActivateEmptyMonitorTransform(monitor.Handle));
	}

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			Logger.Error("No window was found");
			return false;
		}

		Logger.Debug("Moving window {window} in direction {edges} by {pixelsDeltas}");

		// Get the containing workspace.
		IWorkspace? workspace = _context.Butler.Pantry.GetWorkspaceForWindow(window);
		if (workspace == null)
		{
			Logger.Error($"Could not find workspace for window {window}");
			return false;
		}

		// Get the containing monitor.
		IMonitor? monitor = _context.Butler.Pantry.GetMonitorForWorkspace(workspace);
		if (monitor == null)
		{
			Logger.Error($"Could not find monitor for workspace {workspace}");
			return false;
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Normalize `pixelsDeltas` into the unit square.
		IPoint<double> normalized = monitor.WorkingArea.NormalizeDeltaPoint(pixelsDeltas);

		Logger.Debug($"Normalized point: {normalized}");
		workspace.MoveWindowEdgesInDirection(edges, normalized, window, deferLayout: false);
		return true;
	}

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to next workspace");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		// Find the current workspace for the window.
		if (_context.Butler.Pantry.GetWorkspaceForWindow(window) is not IWorkspace currentWorkspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		// Get the adjacent workspace for the current workspace.
		IWorkspace? nextWorkspace = _context.Butler.Pantry.GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
		if (nextWorkspace == null)
		{
			Logger.Debug($"No next workspace found");
			return;
		}

		_context.Butler.Pantry.SetWindowWorkspace(window, nextWorkspace);

		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);

		currentWorkspace.DoLayout();
		nextWorkspace.DoLayout();

		window.Focus();
	}

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to monitor {monitor}");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		Logger.Debug($"Moving window {window} to monitor {monitor}");
		IMonitor? oldMonitor = _context.Butler.Pantry.GetMonitorForWindow(window);
		if (oldMonitor == null)
		{
			Logger.Error($"Window {window} was not found in any monitor");
			return;
		}

		if (oldMonitor.Handle == monitor.Handle)
		{
			Logger.Error($"Window {window} is already on monitor {monitor}");
			return;
		}

		IWorkspace? workspace = _context.Butler.Pantry.GetWorkspaceForMonitor(monitor);
		if (workspace == null)
		{
			Logger.Error($"Monitor {monitor} was not found in any workspace");
			return;
		}

		MoveWindowToWorkspace(workspace, window);
	}

	public void MoveWindowToPreviousMonitor(IWindow? window = null)
	{
		Logger.Debug($"Moving window {window} to previous monitor");

		// Get the previous monitor.
		IMonitor monitor = _context.Store.Pick(Pickers.GetActiveMonitor());
		IMonitor previousMonitor = _context
			.Store.Pick(Pickers.GetAdjacentMonitor(monitor.Handle, reverse: true, getFirst: true))
			.Value;

		MoveWindowToMonitor(previousMonitor, window);
	}

	public void MoveWindowToNextMonitor(IWindow? window = null)
	{
		Logger.Debug($"Moving window {window} to next monitor");

		// Get the next monitor.
		IMonitor monitor = _context.Store.Pick(Pickers.GetActiveMonitor());
		IMonitor nextMonitor = _context
			.Store.Pick(Pickers.GetAdjacentMonitor(monitor.Handle, reverse: false, getFirst: true))
			.Value;

		MoveWindowToMonitor(nextMonitor, window);
	}

	public void MoveWindowToPoint(IWindow window, IPoint<int> point)
	{
		Logger.Debug($"Moving window {window} to point {point}");

		// Get the monitor.
		Result<IMonitor> monitorAtPoint = _context.Store.Pick(Pickers.GetMonitorAtPoint(point));
		if (monitorAtPoint.TryGet(out IMonitor targetMonitor) == false)
		{
			return;
		}

		// Get the target workspace.
		IWorkspace? targetWorkspace = _context.Butler.Pantry.GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");
			return;
		}

		// Get the old workspace.
		IWorkspace? oldWorkspace = _context.Butler.Pantry.GetWorkspaceForWindow(window);
		if (oldWorkspace == null)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		// Normalize `point` into the unit square.
		IPoint<double> normalized = targetMonitor.WorkingArea.NormalizeAbsolutePoint(point);

		Logger.Debug(
			$"Moving window {window} to workspace {targetWorkspace} in monitor {targetMonitor} at normalized point {normalized}"
		);

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (!targetWorkspace.Equals(oldWorkspace))
		{
			_context.Butler.Pantry.SetWindowWorkspace(window, targetWorkspace);
			oldWorkspace.RemoveWindow(window);
			oldWorkspace.DoLayout();
		}

		targetWorkspace.MoveWindowToPoint(window, normalized, deferLayout: false);

		// Trigger layouts.
		window.Focus();
	}

	public void MoveWindowToWorkspace(IWorkspace targetWorkspace, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to workspace {targetWorkspace}");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		Logger.Debug($"Moving window {window} to workspace {targetWorkspace}");

		// Find the current workspace for the window.
		if (_context.Butler.Pantry.GetWorkspaceForWindow(window) is not IWorkspace oldWorkspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		if (oldWorkspace.Equals(targetWorkspace))
		{
			Logger.Error($"Window {window} is already on workspace {targetWorkspace}");
			return;
		}

		_context.Butler.Pantry.SetWindowWorkspace(window, targetWorkspace);

		oldWorkspace.RemoveWindow(window);
		targetWorkspace.AddWindow(window);

		// If both workspaces are visible, activate both
		// Otherwise, only layout the new workspace.
		if (
			_context.Butler.Pantry.GetMonitorForWorkspace(oldWorkspace) is not null
			&& _context.Butler.Pantry.GetMonitorForWorkspace(targetWorkspace) is not null
		)
		{
			targetWorkspace.DoLayout();
			oldWorkspace.DoLayout();
		}
		else
		{
			Activate(targetWorkspace);
		}

		window.Focus();
	}

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target)
	{
		_context.Butler.Pantry.MergeWorkspaceWindows(source, target);

		foreach (IWindow window in source.Windows)
		{
			target.AddWindow(window);
		}
	}

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false)
	{
		// Get the current monitor.
		workspace ??= _context.WorkspaceManager.ActiveWorkspace;
		IMonitor? currentMonitor = _context.Butler.Pantry.GetMonitorForWorkspace(workspace);

		if (currentMonitor == null)
		{
			Logger.Error($"Workspace {workspace} was not found in any monitor");
			return;
		}

		// Get the next monitor.
		Result<IMonitor> nextMonitorResult = _context.Store.Pick(
			Pickers.GetAdjacentMonitor(currentMonitor.Handle, reverse)
		);
		if (nextMonitorResult.TryGet(out IMonitor nextMonitor) == false)
		{
			return;
		}

		if (currentMonitor.Handle == nextMonitor.Handle)
		{
			Logger.Error($"Monitor {currentMonitor} is already the {(!reverse ? "next" : "previous")} monitor");
			return;
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = _context.Butler.Pantry.GetWorkspaceForMonitor(nextMonitor);
		if (nextWorkspace == null)
		{
			Logger.Error($"Monitor {nextMonitor} was not found to correspond to any workspace");
			return;
		}

		Activate(nextWorkspace, currentMonitor);
	}
}
