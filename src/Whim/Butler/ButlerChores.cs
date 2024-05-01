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

	public void Activate(IWorkspace workspace, IMonitor? monitor = null) =>
		_context.Store.Dispatch(new ActivateWorkspaceTransform(workspace, monitor));

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new ActivateAdjacentTransform(monitor, reverse, skipActive));

	public void LayoutAllActiveWorkspaces() => _context.Store.Dispatch(new LayoutAllActiveWorkspacesTransform());

	public void FocusMonitorDesktop(IMonitor monitor) =>
		_context.Store.Dispatch(new FocusMonitorDesktopTransform(monitor));

	public bool MoveWindowEdgesInDirection(Direction edges, IPoint<int> pixelsDeltas, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowEdgesInDirectionTransform(edges, pixelsDeltas, window)).IsSuccessful;

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentWorkspaceTransform(window, reverse, skipActive));

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToMonitorTransform(monitor, window));

	public void MoveWindowToPreviousMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: true));

	public void MoveWindowToNextMonitor(IWindow? window = null) =>
		_context.Store.Dispatch(new MoveWindowToAdjacentMonitorTransform(window, Reverse: false));

	public void MoveWindowToPoint(IWindow window, IPoint<int> point)
	{
		Logger.Debug($"Moving window {window} to point {point}");

		// Get the monitor.
		IMonitor targetMonitor = _context.MonitorManager.GetMonitorAtPoint(point);

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
		IMonitor nextMonitor = reverse
			? _context.MonitorManager.GetPreviousMonitor(currentMonitor)
			: _context.MonitorManager.GetNextMonitor(currentMonitor);

		if (currentMonitor.Equals(nextMonitor))
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
