namespace Whim;

// TODO: Order
internal partial class WorkspaceManager2 : IWorkspaceManager2, IInternalWorkspaceManager
{
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

	public IWorkspace ActiveWorkspace { get; }

	public IWorkspaceContainer WorkspaceContainer { get; }

	public void Activate(IWorkspace workspace, IMonitor? monitor = null)
	{
		if (!WorkspaceContainer.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} is not in the workspace container.");
			return;
		}

		monitor ??= _context.MonitorManager.PrimaryMonitor;

		Logger.Debug($"Activating workspace {workspace} on monitor {monitor}");

		// Get the old workspace for the monitor.
		IWorkspace? oldWorkspace = WorkspaceContainer.GetWorkspaceForMonitor(monitor);

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = WorkspaceContainer.GetMonitorForWorkspace(workspace);

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		WorkspaceContainer.SetWorkspaceMonitor(workspace, monitor);

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && !loserMonitor.Equals(monitor))
		{
			WorkspaceContainer.SetWorkspaceMonitor(oldWorkspace, loserMonitor);
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			// TODO
			// MonitorWorkspaceChanged?.Invoke(
			// 	this,
			// 	new MonitorWorkspaceChangedEventArgs()
			// 	{
			// 		Monitor = oldWorkspaceValue.monitor,
			// 		PreviousWorkspace = workspace,
			// 		CurrentWorkspace = oldWorkspaceValue.workspace
			// 	}
			// );
		}
		else
		{
			// Hide all the windows from the old workspace.
			oldWorkspace?.Deactivate();
		}

		// Layout the new workspace.
		workspace.DoLayout();
		workspace.FocusLastFocusedWindow();
		// TODO
		// MonitorWorkspaceChanged?.Invoke(
		// 	this,
		// 	new MonitorWorkspaceChangedEventArgs()
		// 	{
		// 		Monitor = monitor,
		// 		PreviousWorkspace = oldWorkspace,
		// 		CurrentWorkspace = workspace
		// 	}
		// );
	}

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false)
	{
		monitor ??= _context.MonitorManager.PrimaryMonitor;

		IWorkspace? workspace = WorkspaceContainer.GetWorkspaceForMonitor(monitor);
		if (workspace == null)
		{
			Logger.Error($"No workspace found for monitor {monitor}");
			return;
		}

		IWorkspace? adjacentWorkspace = WorkspaceContainer.GetAdjacentWorkspace(workspace, reverse, skipActive);
		if (adjacentWorkspace == null)
		{
			Logger.Error($"No adjacent workspace found for workspace {workspace}");
			return;
		}

		Activate(adjacentWorkspace, monitor);
	}

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("No window to move");
			return;
		}

		// Find the current workspace for the window.
		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace currentWorkspace)
		{
			Logger.Error($"No workspace found for window {window}");
			return;
		}

		if (
			WorkspaceContainer.GetAdjacentWorkspace(currentWorkspace, reverse, skipActive)
			is not IWorkspace nextWorkspace
		)
		{
			Logger.Error($"No adjacent workspace found for workspace {currentWorkspace}");
			return;
		}

		// Move the window to the next workspace.
		WorkspaceContainer.SetWindowWorkspace(window, nextWorkspace);
		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);
	}

	public void SwapWorkspaceWithAdjacentMonitor(IWorkspace? workspace = null, bool reverse = false)
	{
		workspace ??= _context.WorkspaceManager2.ActiveWorkspace;

		IMonitor? monitor = WorkspaceContainer.GetMonitorForWorkspace(workspace);
		if (monitor == null)
		{
			Logger.Error($"No monitor found for workspace {workspace}");
			return;
		}

		IMonitor adjacentMonitor = reverse
			? _context.MonitorManager.GetPreviousMonitor(monitor)
			: _context.MonitorManager.GetNextMonitor(monitor);
		if (monitor.Equals(adjacentMonitor))
		{
			Logger.Error($"Monitor {monitor} is already the {(!reverse ? "next" : "previous")} monitor");
			return;
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = WorkspaceContainer.GetWorkspaceForMonitor(adjacentMonitor);
		if (nextWorkspace == null)
		{
			Logger.Error($"Monitor {adjacentMonitor} was not found to correspond to any workspace");
			return;
		}

		Activate(nextWorkspace, monitor);
	}

	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("No window to move");
			return;
		}

		// Find the current workspace for the window.
		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace currentWorkspace)
		{
			Logger.Error($"No workspace found for window {window}");
			return;
		}

		// Move the window to the next workspace.
		WorkspaceContainer.SetWindowWorkspace(window, workspace);
		currentWorkspace.RemoveWindow(window);
		workspace.AddWindow(window);
	}

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null)
	{
		window ??= _context.WorkspaceManager2.ActiveWorkspace.LastFocusedWindow;
		if (window == null)
		{
			Logger.Error("No window to move");
			return;
		}

		// Find the current workspace for the window.
		if (WorkspaceContainer.GetWorkspaceForWindow(window) is not IWorkspace oldWorkspace)
		{
			Logger.Error($"No workspace found for window {window}");
			return;
		}

		// Get the workspace for the monitor.
		IWorkspace? targetWorkspace = WorkspaceContainer.GetWorkspaceForMonitor(monitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"No workspace found for monitor {monitor}");
			return;
		}

		// Move the window to the next workspace.
		WorkspaceContainer.SetWindowWorkspace(window, targetWorkspace);
		oldWorkspace.RemoveWindow(window);
		targetWorkspace.AddWindow(window);
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
		IWorkspace? targetWorkspace = WorkspaceContainer.GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");

			return;
		}

		// Get the old workspace.
		IWorkspace? oldWorkspace = WorkspaceContainer.GetWorkspaceForWindow(window);
		if (oldWorkspace == null)
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		bool isSameWorkspace = targetWorkspace.Equals(oldWorkspace);
		if (!isSameWorkspace)
		{
			WorkspaceContainer.SetWindowWorkspace(window, targetWorkspace);
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

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;

		if (window == null)
		{
			Logger.Error("No window was found");
			return false;
		}

		Logger.Debug("Moving window {window} in direction {edges} by {pixelsDeltas}");

		// Get the containing workspace.
		IWorkspace? workspace = WorkspaceContainer.GetWorkspaceForWindow(window);
		if (workspace == null)
		{
			Logger.Error($"Could not find workspace for window {window}");
			return false;
		}

		// Get the containing monitor.
		IMonitor? monitor = WorkspaceContainer.GetMonitorForWorkspace(workspace);
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
}
