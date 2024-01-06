namespace Whim;

internal class ButlerChores : IButlerChores
{
	private readonly IContext _context;
	private readonly ButlerTriggers _triggers;
	private readonly IButlerPantry _pantry;

	public ButlerChores(IContext context, ButlerTriggers triggers, IButlerPantry pantry)
	{
		_context = context;
		_triggers = triggers;
		_pantry = pantry;
	}

	public void Activate(IWorkspace workspace, IMonitor? monitor = null)
	{
		Logger.Debug($"Activating workspace {workspace}");

		if (!_context.WorkspaceManager2.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} is not tracked in Whim.");
			return;
		}

		monitor ??= _context.MonitorManager.ActiveMonitor;

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = _pantry.GetWorkspaceForMonitor(monitor);

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = _pantry.GetMonitorForWorkspace(workspace);

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		_pantry.SetMonitorWorkspace(monitor, workspace);

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && !loserMonitor.Equals(monitor))
		{
			_pantry.SetMonitorWorkspace(loserMonitor, oldWorkspace);
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			_triggers.MonitorWorkspaceChanged(
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
			// Hide all the windows from the old workspace.
			oldWorkspace?.Deactivate();
		}

		// Layout the new workspace.
		workspace.DoLayout();
		workspace.FocusLastFocusedWindow();
		_triggers.MonitorWorkspaceChanged(
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

		monitor ??= _context.MonitorManager.ActiveMonitor;
		IWorkspace? currentWorkspace = _pantry.GetWorkspaceForMonitor(monitor);
		if (currentWorkspace == null)
		{
			Logger.Debug($"No workspace found for monitor {monitor}");
			return;
		}

		IWorkspace? nextWorkspace = _pantry.GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
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
		foreach (IWorkspace workspace in _pantry.GetAllActiveWorkspaces())
		{
			workspace.DoLayout();
		}
	}

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			Logger.Error("No window was found");
			return false;
		}

		Logger.Debug("Moving window {window} in direction {edges} by {pixelsDeltas}");

		// Get the containing workspace.
		IWorkspace? workspace = _pantry.GetWorkspaceForWindow(window);
		if (workspace == null)
		{
			Logger.Error($"Could not find workspace for window {window}");
			return false;
		}

		// Get the containing monitor.
		IMonitor? monitor = _pantry.GetMonitorForWorkspace(workspace);
		if (monitor == null)
		{
			Logger.Error($"Could not find monitor for workspace {workspace}");
			return false;
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Normalize `pixelsDeltas` into the unit square.
		IPoint<double> normalized = monitor.WorkingArea.ToUnitSquare(pixelsDeltas, respectSign: true);

		Logger.Debug($"Normalized point: {normalized}");
		workspace.MoveWindowEdgesInDirection(edges, normalized, window);
		return true;
	}

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to next workspace");

		if (window == null)
		{
			Logger.Error("No window was found");

			return;
		}

		// Find the current workspace for the window.
		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace currentWorkspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		IWorkspace? nextWorkspace = _pantry.GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
		if (nextWorkspace == null)
		{
			Logger.Debug($"No next workspace found");
			return;
		}

		_pantry.SetWindowWorkspace(window, nextWorkspace);

		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);
	}

	// TODO: #734
	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to monitor {monitor}");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		Logger.Debug($"Moving window {window} to monitor {monitor}");
		IMonitor? oldMonitor = _pantry.GetMonitorForWindow(window);
		if (oldMonitor == null)
		{
			Logger.Error($"Window {window} was not found in any monitor");
			return;
		}

		if (oldMonitor.Equals(monitor))
		{
			Logger.Error($"Window {window} is already on monitor {monitor}");
			return;
		}

		IWorkspace? workspace = _pantry.GetWorkspaceForMonitor(monitor);
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
		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		IMonitor previousMonitor = _context.MonitorManager.GetPreviousMonitor(monitor);

		MoveWindowToMonitor(previousMonitor, window);
	}

	public void MoveWindowToNextMonitor(IWindow? window = null)
	{
		Logger.Debug($"Moving window {window} to next monitor");

		// Get the next monitor.
		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		IMonitor nextMonitor = _context.MonitorManager.GetNextMonitor(monitor);

		MoveWindowToMonitor(nextMonitor, window);
	}

	public void MoveWindowToPoint(IWindow window, IPoint<int> point)
	{
		Logger.Debug($"Moving window {window} to point {point}");

		// Get the monitor.
		IMonitor targetMonitor = _context.MonitorManager.GetMonitorAtPoint(point);

		// Get the target workspace.
		IWorkspace? targetWorkspace = _pantry.GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");

			return;
		}

		// Get the old workspace.
		IWorkspace? oldWorkspace = _pantry.GetWorkspaceForWindow(window);
		if (oldWorkspace == null)
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		bool isSameWorkspace = targetWorkspace.Equals(oldWorkspace);
		if (!isSameWorkspace)
		{
			_pantry.SetWindowWorkspace(window, targetWorkspace);
		}

		// Normalize `point` into the unit square.
		IPoint<int> pointInMonitor = targetMonitor.WorkingArea.ToMonitorCoordinates(point);
		IPoint<double> normalized = targetMonitor.WorkingArea.ToUnitSquare(pointInMonitor);

		Logger.Debug(
			$"Moving window {window} to workspace {targetWorkspace} in monitor {targetMonitor} at normalized point {normalized}"
		);

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (!isSameWorkspace)
		{
			oldWorkspace.RemoveWindow(window);
		}
		targetWorkspace.MoveWindowToPoint(window, normalized);

		// Trigger layouts.
		window.Focus();
	}

	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to workspace {workspace}");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Find the current workspace for the window.
		if (_pantry.GetWorkspaceForWindow(window) is not IWorkspace currentWorkspace)
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		_pantry.SetWindowWorkspace(window, workspace);

		currentWorkspace.RemoveWindow(window);
		workspace.AddWindow(window);
	}

	public void MergeWorkspaceWindows(IWorkspace source, IWorkspace target)
	{
		_pantry.MergeWorkspaceWindows(source, target);

		foreach (IWindow window in source.Windows)
		{
			target.AddWindow(window);
		}
	}

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false)
	{
		// Get the current monitor.
		workspace ??= _context.WorkspaceManager2.ActiveWorkspace;
		IMonitor? currentMonitor = _pantry.GetMonitorForWorkspace(workspace);

		if (currentMonitor == null)
		{
			Logger.Error($"Workspace {workspace} was not found in any monitor");
			return;
		}

		// Get the next monitor.
		IMonitor nextMonitor = reverse
			? _context.MonitorManager.GetPreviousMonitor(currentMonitor)
			: _context.MonitorManager.GetNextMonitor(currentMonitor);

		if (currentMonitor.Equals(nextMonitor))
		{
			Logger.Error($"Monitor {currentMonitor} is already the {(!reverse ? "next" : "previous")} monitor");
			return;
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = _pantry.GetWorkspaceForMonitor(nextMonitor);
		if (nextWorkspace == null)
		{
			Logger.Error($"Monitor {nextMonitor} was not found to correspond to any workspace");
			return;
		}

		Activate(nextWorkspace, currentMonitor);
	}
}
