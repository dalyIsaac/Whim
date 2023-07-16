using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

/// <summary>
/// Implementation of <see cref="IWorkspaceManager"/>.
/// </summary>
internal class WorkspaceManager : IInternalWorkspaceManager, IWorkspaceManager
{
	private bool _initialized;
	private readonly IContext _context;
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
	/// All the phantom windows that are currently added.
	/// </summary>
	private readonly HashSet<IWindow> _phantomWindows = new();

	public IEnumerable<IWindow> PhantomWindows => _phantomWindows;

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

	public Func<IList<IImmutableLayoutEngine>> CreateLayoutEngines { get; set; } =
		() => new IImmutableLayoutEngine[] { new ImmutableColumnLayoutEngine() };

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace ActiveWorkspace
	{
		get
		{
			if (_monitorWorkspaceMap.ContainsKey(_context.MonitorManager.ActiveMonitor))
			{
				return _monitorWorkspaceMap[_context.MonitorManager.ActiveMonitor];
			}
			else
			{
				return _workspaces[0];
			}
		}
	}

	private readonly List<ImmutableProxyLayoutEngine> _proxyLayoutEngines = new();

	private bool _disposedValue;

	public IEnumerable<ImmutableProxyLayoutEngine> ProxyLayoutEngines => _proxyLayoutEngines;

	public WorkspaceManager(IContext context)
	{
		_context = context;
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

		_initialized = true;

		_context.MonitorManager.MonitorsChanged += MonitorManager_MonitorsChanged;

		// Ensure there's at least n workspaces, for n monitors.
		if (_context.MonitorManager.Length > _workspaces.Count)
		{
			throw new InvalidOperationException("There must be at least as many workspaces as monitors.");
		}

		// Assign workspaces to monitors.
		int idx = 0;
		foreach (IMonitor monitor in _context.MonitorManager)
		{
			// Get the workspace for this monitor. If the user hasn't provided enough workspaces, create a new one.
			IWorkspace workspace =
				idx < _workspaces.Count
					? _workspaces[idx]
					: new Workspace(_context, _triggers, $"Workspace {idx + 1}", CreateLayoutEngines());

			Activate(workspace, monitor);
			idx++;
		}

		// Initialize each of the workspaces.
		foreach (IWorkspace workspace in _workspaces)
		{
			workspace.Initialize();
		}
	}

	#region Workspaces
	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public void Add(string? name = null, IEnumerable<IImmutableLayoutEngine>? layoutEngines = null)
	{
		Workspace workspace =
			new(
				_context,
				_triggers,
				name ?? $"Workspace {_workspaces.Count + 1}",
				layoutEngines ?? CreateLayoutEngines()
			);

		_workspaces.Add(workspace);

		if (_initialized)
		{
			workspace.Initialize();
		}

		WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
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

		WorkspaceRemoved?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });

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
			workspaceToActivate.AddWindow(window);
			_windowWorkspaceMap[window] = workspaceToActivate;
		}

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

		activeMonitor ??= _context.MonitorManager.ActiveMonitor;

		// Get the old workspace for the event.
		_monitorWorkspaceMap.TryGetValue(activeMonitor, out IWorkspace? oldWorkspace);

		// Find the monitor which just lost `workspace`.
		IMonitor? loserMonitor = _monitorWorkspaceMap.FirstOrDefault(m => m.Value == workspace).Key;

		// Update the active monitor. Having this line before the old workspace is deactivated
		// is important, as WindowManager.OnWindowHidden() checks to see if a window is in a
		// visible workspace when it receives the EVENT_OBJECT_HIDE event.
		_monitorWorkspaceMap[activeMonitor] = workspace;

		// Send out an event about the losing monitor.
		if (loserMonitor != null && oldWorkspace != null)
		{
			_monitorWorkspaceMap[loserMonitor] = oldWorkspace;
			oldWorkspace.DoLayout();
			MonitorWorkspaceChanged?.Invoke(
				this,
				new MonitorWorkspaceChangedEventArgs()
				{
					Monitor = loserMonitor,
					PreviousWorkspace = workspace,
					CurrentWorkspace = oldWorkspace
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
		workspace.FocusFirstWindow();
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

	public void ActivatePrevious(IMonitor? monitor = null)
	{
		Logger.Debug("Activating previous workspace");

		monitor ??= _context.MonitorManager.ActiveMonitor;
		IWorkspace? currentWorkspace = GetWorkspaceForMonitor(monitor);
		if (currentWorkspace == null)
		{
			Logger.Debug($"No workspace found for monitor {monitor}");
			return;
		}

		int idx = _workspaces.IndexOf(currentWorkspace);
		int prevIdx = (idx - 1).Mod(_workspaces.Count);

		IWorkspace prevWorkspace = _workspaces[prevIdx];

		Activate(prevWorkspace, monitor);
	}

	public void ActivateNext(IMonitor? monitor = null)
	{
		Logger.Debug("Activating next workspace");

		monitor ??= _context.MonitorManager.ActiveMonitor;
		IWorkspace? currentWorkspace = GetWorkspaceForMonitor(monitor);
		if (currentWorkspace == null)
		{
			Logger.Debug($"No workspace found for monitor {monitor}");
			return;
		}

		int idx = _workspaces.IndexOf(currentWorkspace);
		int nextIdx = (idx + 1).Mod(_workspaces.Count);

		IWorkspace nextWorkspace = _workspaces[nextIdx];

		Activate(nextWorkspace, monitor);
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for active workspace {workspace}");

		// Linear search for the monitor that contains the workspace.
		foreach ((IMonitor m, IWorkspace w) in _monitorWorkspaceMap)
		{
			if (w == workspace)
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
		foreach (IWorkspace workspace in _monitorWorkspaceMap.Values)
		{
			workspace.DoLayout();
		}
	}
	#endregion

	#region Windows
	public void WindowAdded(IWindow window)
	{
		Logger.Debug($"Adding window {window}");
		IWorkspace? workspace = _context.RouterManager.RouteWindow(window);

		if (!_context.RouterManager.RouteToActiveWorkspace && workspace == null)
		{
			IMonitor? monitor = _context.MonitorManager.GetMonitorAtPoint(window.Center);
			if (monitor is not null)
			{
				workspace = GetWorkspaceForMonitor(monitor);
			}
		}
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

		workspace.RemoveWindow(window);
		_windowWorkspaceMap.Remove(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
	}

	public void WindowFocused(IWindow window)
	{
		Logger.Debug($"Window focused: {window}");

		foreach (IWorkspace workspace in _workspaces)
		{
			if (workspace is IInternalWorkspace ws)
			{
				ws.WindowFocused(window);
			}
		}

		_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspaceFocused);
		if (workspaceFocused != null && workspaceFocused != ActiveWorkspace)
		{
			Activate(workspaceFocused);
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

		if (workspace is IInternalWorkspace ws)
		{
			ws.WindowMinimizeStart(window);
		}
	}

	public void WindowMinimizeEnd(IWindow window)
	{
		Logger.Debug($"Window minimize end: {window}");

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		if (workspace is IInternalWorkspace ws)
		{
			ws.WindowMinimizeEnd(window);
		}
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
				workspace = new Workspace(
					_context,
					_triggers,
					$"Workspace {_workspaces.Count + 1}",
					CreateLayoutEngines()
				);
				workspace.Initialize();
			}

			// Add the workspace to the map.
			Activate(workspace, monitor);
		}

		// For each workspace which is active in a monitor, do a layout.
		// This will handle cases when the monitor's properties have changed.
		LayoutAllActiveWorkspaces();
	}

	public void AddProxyLayoutEngine(ImmutableProxyLayoutEngine proxyLayoutEngine)
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

		if (_phantomWindows.Contains(window))
		{
			Logger.Error($"Window {window} is a phantom window and cannot be moved");
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

		if (GetMonitorForWorkspace(workspace) is null)
		{
			window.Hide();
		}
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

		if (oldMonitor == monitor)
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
		Logger.Debug($"Moving window {window} to location {point}");

		// Duck out if the window is a phantom window.
		bool isPhantom = _phantomWindows.Contains(window);
		if (isPhantom)
		{
			Logger.Error($"Window {window} is a phantom window and cannot be moved");
			return;
		}
		// Get the monitor.
		IMonitor targetMonitor = _context.MonitorManager.GetMonitorAtPoint(point);

		// Get the target workspace.
		IWorkspace? targetWorkspace = GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");
			return;
		}

		Logger.Debug($"Moving window {window} to workspace {targetWorkspace} in monitor {targetMonitor}");
		Logger.Debug($"Active workspace is {ActiveWorkspace}");

		// If the window is being moved to a different workspace, remove it from the current workspace.
		if (targetWorkspace != ActiveWorkspace && !ActiveWorkspace.RemoveWindow(window))
		{
			Logger.Error($"Could not remove window {window} from workspace {ActiveWorkspace}");
			return;
		}

		// Normalize `point` into the unit square.
		IPoint<int> pointInMonitor = targetMonitor.WorkingArea.ToMonitorCoordinates(point);
		IPoint<double> normalized = targetMonitor.WorkingArea.ToUnitSquare(pointInMonitor);

		Logger.Debug($"Normalized point: {normalized}");
		targetWorkspace.MoveWindowToPoint(window, normalized);

		// If the window is being moved to a different workspace, update the reference in the window map.
		if (targetWorkspace != ActiveWorkspace)
		{
			_windowWorkspaceMap[window] = targetWorkspace;
		}

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

	#region Phantom Windows
	public void AddPhantomWindow(IWorkspace workspace, IWindow window)
	{
		Logger.Debug($"Adding phantom window {window} to workspace {workspace}");
		_phantomWindows.Add(window);
		_windowWorkspaceMap[window] = workspace;
	}

	public void RemovePhantomWindow(IWindow window)
	{
		Logger.Debug($"Removing phantom window {window}");
		_phantomWindows.Remove(window);
		_windowWorkspaceMap.Remove(window);
	}
	#endregion

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
