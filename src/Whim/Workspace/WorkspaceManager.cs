using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.Foundation;

namespace Whim;

/// <summary>
/// Functions to trigger <see cref="WorkspaceManager"/> events, for within <see cref="Workspace"/>.
/// </summary>
internal record WorkspaceManagerTriggers
{
	public required Action<ActiveLayoutEngineChangedEventArgs> ActiveLayoutEngineChanged { get; init; }
	public required Action<WorkspaceRenamedEventArgs> WorkspaceRenamed { get; init; }
	public required Action<WorkspaceEventArgs> WorkspaceLayoutStarted { get; init; }
	public required Action<WorkspaceEventArgs> WorkspaceLayoutCompleted { get; init; }
}

internal record WorkspaceToCreate(string Name, IEnumerable<CreateLeafLayoutEngine>? LayoutEngines);

/// <summary>
/// Implementation of <see cref="IWorkspaceManager"/>.
/// </summary>
internal class WorkspaceManager : IInternalWorkspaceManager, IWorkspaceManager
{
	private bool _initialized;

	private readonly IContext _context;
	private readonly IInternalContext _internalContext;
	protected readonly WorkspaceManagerTriggers _triggers;

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	protected readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// Maps windows to their workspace.
	/// </summary>
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();

	/// <summary>
	/// Stores the workspaces to create, when <see cref="Initialize"/> is called.
	/// The workspaces will have been created prior to <see cref="Initialize"/>.
	/// </summary>
	private readonly List<WorkspaceToCreate> _workspacesToCreate = new();

	/// <summary>
	/// Maps monitors to their active workspace.
	/// </summary>
	private readonly Dictionary<IMonitor, IWorkspace> _monitorWorkspaceMap = new();

	public event EventHandler<MonitorWorkspaceChangedEventArgs>? MonitorWorkspaceChanged;

	public event EventHandler<RouteEventArgs>? WindowRouted;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	private readonly List<CreateProxyLayoutEngine> _proxyLayoutEngines = new();

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace ActiveWorkspace
	{
		get
		{
			IMonitor activeMonitor = _context.MonitorManager.ActiveMonitor;
			Logger.Debug($"Getting active workspace for monitor {activeMonitor}");

			return _monitorWorkspaceMap.TryGetValue(activeMonitor, out IWorkspace? workspace)
				? workspace
				: _workspaces[0];
		}
	}

	private bool _disposedValue;

	public WorkspaceManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
		_triggers = new WorkspaceManagerTriggers()
		{
			ActiveLayoutEngineChanged = (ActiveLayoutEngineChangedEventArgs e) =>
				ActiveLayoutEngineChanged?.Invoke(this, e),
			WorkspaceRenamed = (WorkspaceRenamedEventArgs e) => WorkspaceRenamed?.Invoke(this, e),
			WorkspaceLayoutStarted = (WorkspaceEventArgs e) => WorkspaceLayoutStarted?.Invoke(this, e),
			WorkspaceLayoutCompleted = (WorkspaceEventArgs e) => WorkspaceLayoutCompleted?.Invoke(this, e)
		};
	}

	public void Initialize()
	{
		Logger.Debug("Initializing workspace manager...");

		InitializeWorkspaces();

		_initialized = true;
		_context.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;

		InitializeWindows();

		Logger.Debug("Workspace manager initialized");
	}

	/// <summary>
	/// Load the saved workspaces and merge them with the user-specified workspaces.
	/// </summary>
	/// <exception cref="InvalidOperationException"></exception>
	private void InitializeWorkspaces()
	{
		// Create the workspaces.
		foreach (WorkspaceToCreate workspaceToCreate in _workspacesToCreate)
		{
			CreateWorkspace(workspaceToCreate.Name, workspaceToCreate.LayoutEngines);
		}

		// Assign workspaces to monitors.
		int idx = 0;
		foreach (IMonitor monitor in _context.MonitorManager)
		{
			// Get the workspace for this monitor. If the user hasn't provided enough workspaces, try create a new one.
			IWorkspace? workspace =
				(idx < _workspaces.Count ? _workspaces[idx] : CreateWorkspace($"Workspace {idx + 1}"))
				?? throw new InvalidOperationException($"Could not create workspace");

			Activate(workspace, monitor);
			idx++;
		}
	}

	/// <summary>
	/// Add the saved windows at their saved locations inside their saved workspaces.
	/// Other windows are routed to the monitor they're on.
	/// </summary>
	private void InitializeWindows()
	{
		List<HWND> processedWindows = new();

		// Route windows to their saved workspaces.
		foreach (
			SavedWorkspace savedWorkspace in _internalContext.CoreSavedStateManager.SavedState?.Workspaces ?? new()
		)
		{
			IWorkspace? workspace = TryGet(savedWorkspace.Name);
			if (workspace == null)
			{
				Logger.Debug($"Could not find workspace {savedWorkspace.Name}");
				continue;
			}

			foreach (SavedWindow savedWindow in savedWorkspace.Windows)
			{
				HWND hwnd = (HWND)savedWindow.Handle;
				IWindow? window = _context.WindowManager.CreateWindow(hwnd);
				if (window == null)
				{
					Logger.Debug($"Could not find window with handle {savedWindow.Handle}");
					continue;
				}

				_windowWorkspaceMap[window] = workspace;
				workspace.MoveWindowToPoint(window, savedWindow.Rectangle.Center);
				processedWindows.Add(hwnd);

				// Fire the window added event.
				_internalContext.WindowManager.OnWindowAdded(window);
			}
		}

		// Route the rest of the windows to the monitor they're on.
		// Don't route to the active workspace while we're adding all the windows.
		RouterOptions routerOptions = _context.RouterManager.RouterOptions;
		_context.RouterManager.RouterOptions = RouterOptions.RouteToLaunchedWorkspace;

		// Add all existing windows.
		foreach (HWND hwnd in _internalContext.CoreNativeManager.GetAllWindows())
		{
			if (processedWindows.Contains(hwnd))
			{
				continue;
			}

			_internalContext.WindowManager.AddWindow(hwnd);
		}

		// Restore the route to active workspace setting.
		_context.RouterManager.RouterOptions = routerOptions;
	}

	#region Workspaces
	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	private Workspace? CreateWorkspace(
		string? name = null,
		IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null
	)
	{
		CreateLeafLayoutEngine[] engineCreators = createLayoutEngines?.ToArray() ?? CreateLayoutEngines();

		if (engineCreators.Length == 0)
		{
			Logger.Error("No layout engines were provided");
			return null;
		}

		// Create the layout engines.
		ILayoutEngine[] layoutEngines = new ILayoutEngine[engineCreators.Length];
		for (int i = 0; i < engineCreators.Length; i++)
		{
			layoutEngines[i] = engineCreators[i](new LayoutEngineIdentity());
		}

		// Set up the proxies.
		for (int engineIdx = 0; engineIdx < engineCreators.Length; engineIdx++)
		{
			ILayoutEngine currentEngine = layoutEngines[engineIdx];
			foreach (CreateProxyLayoutEngine createProxyLayoutEngineFn in _proxyLayoutEngines)
			{
				ILayoutEngine proxy = createProxyLayoutEngineFn(currentEngine);
				layoutEngines[engineIdx] = proxy;
				currentEngine = proxy;
			}
		}

		// Create the workspace.
		Workspace workspace =
			new(_context, _internalContext, _triggers, name ?? $"Workspace {_workspaces.Count + 1}", layoutEngines);
		_workspaces.Add(workspace);
		WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
		return workspace;
	}

	public void Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null)
	{
		if (_initialized)
		{
			CreateWorkspace(name, createLayoutEngines);
		}
		else
		{
			_workspacesToCreate.Add(new(name ?? $"Workspace {_workspaces.Count + 1}", createLayoutEngines));
		}
	}

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool Remove(IWorkspace workspace)
	{
		Logger.Debug($"Removing workspace {workspace}");

		if (_workspaces.Count - 1 < _context.MonitorManager.Length)
		{
			Logger.Debug($"There must be at least {_context.MonitorManager.Length} workspaces.");
			return false;
		}

		bool wasFound = _workspaces.Remove(workspace);

		if (!wasFound)
		{
			Logger.Debug($"Workspace {workspace} was not found");
			return false;
		}

		// Remap windows to the first workspace which isn't active.
		IWorkspace workspaceToActivate = _workspaces[^1];
		foreach (IWorkspace w in _workspaces)
		{
			if (!_monitorWorkspaceMap.ContainsValue(w))
			{
				workspaceToActivate = w;
				break;
			}
		}

		foreach (IWindow window in workspace.Windows)
		{
			_windowWorkspaceMap[window] = workspaceToActivate;
		}

		foreach (IWindow window in workspace.Windows)
		{
			workspaceToActivate.AddWindow(window);
		}

		WorkspaceRemoved?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });

		// Activate the last workspace
		Activate(workspaceToActivate);

		return wasFound;
	}

	public bool Remove(string workspaceName)
	{
		Logger.Debug($"Trying to remove workspace {workspaceName}");

		IWorkspace? workspace = _workspaces.Find(w => w.Name == workspaceName);
		if (workspace == null)
		{
			Logger.Debug($"Workspace {workspaceName} not found");
			return false;
		}

		return Remove(workspace);
	}

	public IWorkspace? TryGet(string workspaceName)
	{
		Logger.Debug($"Trying to get workspace {workspaceName}");
		return _workspaces.Find(w => w.Name == workspaceName);
	}

	public void Activate(IWorkspace workspace, IMonitor? activeMonitor = null)
	{
		Logger.Debug($"Activating workspace {workspace}");

		if (!_workspaces.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} is not tracked in Whim.");
			return;
		}

		activeMonitor ??= _context.MonitorManager.ActiveMonitor;

		// Get the old workspace for the event.
		IWorkspace? oldWorkspace = GetWorkspaceForMonitor(activeMonitor);

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = GetMonitorForWorkspace(workspace);

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		_monitorWorkspaceMap[activeMonitor] = workspace;

		(IWorkspace workspace, IMonitor monitor)? layoutOldWorkspace = null;
		if (loserMonitor != null && oldWorkspace != null && !loserMonitor.Equals(activeMonitor))
		{
			_monitorWorkspaceMap[loserMonitor] = oldWorkspace;
			layoutOldWorkspace = (oldWorkspace, loserMonitor);
		}

		if (layoutOldWorkspace is (IWorkspace, IMonitor) oldWorkspaceValue)
		{
			Logger.Debug($"Layouting workspace {oldWorkspace} in loser monitor {loserMonitor}");
			oldWorkspace?.DoLayout();
			MonitorWorkspaceChanged?.Invoke(
				this,
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
		MonitorWorkspaceChanged?.Invoke(
			this,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = activeMonitor,
				PreviousWorkspace = oldWorkspace,
				CurrentWorkspace = workspace
			}
		);
	}

	public void ActivatePrevious(IMonitor? monitor = null) => ActivateAdjacent(monitor, true);

	public void ActivateNext(IMonitor? monitor = null) => ActivateAdjacent(monitor, false);

	public void ActivateAdjacent(IMonitor? monitor = null, bool reverse = false, bool skipActive = false)
	{
		Logger.Debug("Activating next workspace");

		monitor ??= _context.MonitorManager.ActiveMonitor;
		IWorkspace? currentWorkspace = GetWorkspaceForMonitor(monitor);
		if (currentWorkspace == null)
		{
			Logger.Debug($"No workspace found for monitor {monitor}");
			return;
		}

		IWorkspace? nextWorkspace = GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
		if (nextWorkspace == null)
		{
			Logger.Debug($"No next workspace found for monitor {monitor}");
			return;
		}

		Activate(nextWorkspace, monitor);
	}

	public void MoveWindowToAdjacentWorkspace(IWindow? window = null, bool reverse = false, bool skipActive = false)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to next workspace");

		if (window == null)
		{
			Logger.Error("No window was found");

			return;
		}

		// Find the current workspace for the window.
		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? currentWorkspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		IWorkspace? nextWorkspace = GetAdjacentWorkspace(currentWorkspace, reverse, skipActive);
		if (nextWorkspace == null)
		{
			Logger.Debug($"No next workspace found");
			return;
		}

		_windowWorkspaceMap[window] = nextWorkspace;

		currentWorkspace.RemoveWindow(window);
		nextWorkspace.AddWindow(window);
	}

	private IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse, bool skipActive)
	{
		int idx = _workspaces.IndexOf(workspace);
		int delta = reverse ? -1 : 1;
		int nextIdx = (idx + delta).Mod(_workspaces.Count);

		while (idx != nextIdx)
		{
			IWorkspace nextWorkspace = _workspaces[nextIdx];
			IMonitor? monitor = GetMonitorForWorkspace(nextWorkspace);

			if (monitor == null || !skipActive)
			{
				return nextWorkspace;
			}

			nextIdx = (nextIdx + delta).Mod(_workspaces.Count);
		}

		return null;
	}

	public void SwapActiveWorkspaceWithAdjacentMonitor(bool reverse = false)
	{
		// Get the next monitor.
		IMonitor monitor = _context.MonitorManager.ActiveMonitor;
		IMonitor nextMonitor = reverse
			? _context.MonitorManager.GetPreviousMonitor(monitor)
			: _context.MonitorManager.GetNextMonitor(monitor);

		if (monitor.Equals(nextMonitor))
		{
			Logger.Error($"Monitor {monitor} is already the {(!reverse ? "next" : "previous")} monitor");
			return;
		}

		// Get workspace on next monitor.
		IWorkspace? nextWorkspace = GetWorkspaceForMonitor(nextMonitor);
		if (nextWorkspace == null)
		{
			Logger.Error($"Monitor {nextMonitor} was not found to correspond to any workspace");
			return;
		}

		Activate(nextWorkspace, monitor);
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for active workspace {workspace}");

		// Linear search for the monitor that contains the workspace.
		foreach ((IMonitor m, IWorkspace w) in _monitorWorkspaceMap)
		{
			if (w.Equals(workspace))
			{
				Logger.Debug($"Found monitor {m} for workspace {workspace}");
				return m;
			}
		}

		Logger.Debug($"Could not find monitor for workspace {workspace}");
		return null;
	}

	public void LayoutAllActiveWorkspaces()
	{
		Logger.Debug("Layout all active workspaces");

		// For each workspace which is active in a monitor, do a layout.
		foreach (IWorkspace workspace in _monitorWorkspaceMap.Values.ToArray())
		{
			workspace.DoLayout();
		}
	}
	#endregion

	#region Windows
	public void WindowAdded(IWindow window)
	{
		Logger.Debug($"Adding window {window}");

		IWorkspace? workspace = null;
		if (_context.RouterManager.RouterOptions == RouterOptions.RouteToActiveWorkspace)
		{
			workspace = ActiveWorkspace;
		}
		else if (_context.RouterManager.RouterOptions == RouterOptions.RouteToLastTrackedActiveWorkspace)
		{
			workspace = _internalContext.MonitorManager.LastWhimActiveMonitor is IMonitor lastWhimActiveMonitor
				? GetWorkspaceForMonitor(lastWhimActiveMonitor)
				: ActiveWorkspace;
		}

		// RouteWindow takes precedence over RouterOptions.
		if (_context.RouterManager.RouteWindow(window) is IWorkspace routedWorkspace)
		{
			workspace = routedWorkspace;
		}

		// Check the workspace exists. If it doesn't, clear the workspace.
		if (workspace != null && !_workspaces.Contains(workspace))
		{
			Logger.Error($"Workspace {workspace} was not found");
			workspace = null;
		}

		// If no workspace has been selected, route the window to the monitor it's on.
		if (workspace == null)
		{
			IMonitor? monitor = _context.MonitorManager.GetMonitorAtPoint(window.Rectangle.Center);
			if (monitor is not null)
			{
				workspace = GetWorkspaceForMonitor(monitor);
			}
		}

		// If that fails too, route the window to the active workspace.
		workspace ??= ActiveWorkspace;

		_windowWorkspaceMap[window] = workspace;

		workspace.AddWindow(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowAdded(window, workspace));
		Logger.Debug($"Window {window} added to workspace {workspace.Name}");
	}

	public void WindowRemoved(IWindow window)
	{
		Logger.Debug($"Window removed: {window}");

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		_windowWorkspaceMap.Remove(window);

		workspace.RemoveWindow(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
	}

	public void WindowFocused(IWindow? window)
	{
		Logger.Debug($"Window focused: {window}");

		foreach (IWorkspace workspace in _workspaces)
		{
			((IInternalWorkspace)workspace).WindowFocused(window);
		}

		if (window is null)
		{
			return;
		}

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspaceForWindow))
		{
			Logger.Debug($"Window {window} was not found in any workspace");
			return;
		}

		if (!_monitorWorkspaceMap.ContainsValue(workspaceForWindow))
		{
			Logger.Debug($"Window {window} is not in an active workspace");
			Activate(workspaceForWindow);
			return;
		}
	}

	public void WindowMinimizeStart(IWindow window)
	{
		Logger.Debug($"Window minimize start: {window}");

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		((IInternalWorkspace)workspace).WindowMinimizeStart(window);
	}

	public void WindowMinimizeEnd(IWindow window)
	{
		Logger.Debug($"Window minimize end: {window}");

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		((IInternalWorkspace)workspace).WindowMinimizeEnd(window);
	}
	#endregion

	#region Monitors
	public void MonitorManager_MonitorsChanged(object? sender, MonitorsChangedEventArgs e)
	{
		Logger.Debug($"MonitorManager_MonitorsChanged: {e}");

		// If a monitor was removed, remove the workspace from the map.
		foreach (IMonitor monitor in e.RemovedMonitors)
		{
			IWorkspace? workspace = GetWorkspaceForMonitor(monitor);
			_monitorWorkspaceMap.Remove(monitor);

			if (workspace is null)
			{
				Logger.Error($"Could not find workspace for monitor {monitor}");
				continue;
			}

			workspace.Deactivate();
		}

		// If a monitor was added, set it to an inactive workspace.
		foreach (IMonitor monitor in e.AddedMonitors)
		{
			// Try find a workspace which doesn't have a monitor.
			IWorkspace? workspace = _workspaces.Find(w => GetMonitorForWorkspace(w) == null);

			// If there's no workspace, create one.
			if (workspace is null)
			{
				if (CreateWorkspace() is IWorkspace newWorkspace)
				{
					workspace = newWorkspace;
					_workspaces.Add(workspace);
					WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
				}
				else
				{
					return;
				}
			}

			// Add the workspace to the map.
			Activate(workspace, monitor);
		}

		// For each workspace which is active in a monitor, do a layout.
		// This will handle cases when the monitor's properties have changed.
		LayoutAllActiveWorkspaces();
	}

	public void AddProxyLayoutEngine(CreateProxyLayoutEngine proxyLayoutEngine)
	{
		Logger.Debug($"Adding proxy layout engine: {proxyLayoutEngine}");
		_proxyLayoutEngines.Add(proxyLayoutEngine);
	}

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting workspace for monitor: {monitor}");
		return _monitorWorkspaceMap.TryGetValue(monitor, out IWorkspace? workspace) ? workspace : null;
	}

	public IMonitor? GetMonitorForWindow(IWindow window)
	{
		Logger.Debug($"Getting monitor for window: {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace)
			? GetMonitorForWorkspace(workspace)
			: null;
	}
	#endregion

	public IWorkspace? GetWorkspaceForWindow(IWindow window)
	{
		Logger.Debug($"Getting workspace for window: {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace) ? workspace : null;
	}

	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to workspace {workspace}");

		if (window == null)
		{
			Logger.Error("No window was found");

			return;
		}

		Logger.Debug($"Moving window {window} to workspace {workspace}");

		// Find the current workspace for the window.
		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? currentWorkspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		_windowWorkspaceMap[window] = workspace;

		currentWorkspace.RemoveWindow(window);
		workspace.AddWindow(window);
	}

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;
		Logger.Debug($"Moving window {window} to monitor {monitor}");

		if (window == null)
		{
			Logger.Error("No window was found");
			return;
		}

		Logger.Debug($"Moving window {window} to monitor {monitor}");
		IMonitor? oldMonitor = GetMonitorForWindow(window);
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

		IWorkspace? workspace = GetWorkspaceForMonitor(monitor);
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
		IWorkspace? targetWorkspace = GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");

			return;
		}

		// Get the old workspace.
		IWorkspace? oldWorkspace = GetWorkspaceForWindow(window);
		if (oldWorkspace == null)
		{
			Logger.Error($"Window {window} was not found in any workspace");

			return;
		}

		bool isSameWorkspace = targetWorkspace.Equals(oldWorkspace);
		if (!isSameWorkspace)
		{
			_windowWorkspaceMap[window] = targetWorkspace;
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
		IWorkspace? workspace = GetWorkspaceForWindow(window);
		if (workspace == null)
		{
			Logger.Error($"Could not find workspace for window {window}");
			return false;
		}

		// Get the containing monitor.
		IMonitor? monitor = GetMonitorForWorkspace(workspace);
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

	protected void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				Logger.Debug("Disposing workspace manager");

				// dispose managed state (managed objects)
				foreach (IWorkspace workspace in _workspaces)
				{
					workspace.Dispose();
				}

				_internalContext.Dispose();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
