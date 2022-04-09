using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

/// <summary>
/// Implementation of <see cref="IWorkspaceManager"/>.
/// </summary>
public class WorkspaceManager : IWorkspaceManager
{
	private readonly IConfigContext _configContext;
	public Commander Commander { get; } = new();

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	private readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// Maps windows to their workspace.
	/// </summary>
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();

	/// <summary>
	/// All the phantom windows that are currently registered.
	/// </summary>
	private readonly HashSet<IWindow> _phantomWindows = new();

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

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace ActiveWorkspace { get => _monitorWorkspaceMap[_configContext.MonitorManager.FocusedMonitor]; }

	private readonly List<ProxyLayoutEngine> _proxyLayoutEngines = new();
	public IEnumerable<ProxyLayoutEngine> ProxyLayoutEngines { get => _proxyLayoutEngines; }

	public WorkspaceManager(IConfigContext configContext)
	{
		_configContext = configContext;
	}

	public void Initialize()
	{
		Logger.Debug("Initializing workspace manager...");

		// Ensure there's at least n workspaces, for n monitors.
		if (_configContext.MonitorManager.Length > _workspaces.Count)
		{
			throw new InvalidOperationException("There must be at least as many workspaces as monitors.");
		}

		// Assign workspaces to monitors.
		int idx = 0;
		foreach (IMonitor monitor in _configContext.MonitorManager)
		{
			Activate(_workspaces[idx], monitor);
			idx++;
		}

		// Subscribe to <see cref="IWindowManager"/> events.
		_configContext.WindowManager.WindowRegistered += WindowManager_WindowRegistered;
		_configContext.WindowManager.WindowUnregistered += WindowManager_WindowUnregistered;

		// Initialize each of the workspaces.
		foreach (IWorkspace workspace in _workspaces)
		{
			workspace.Initialize();
		}
	}

	#region Workspaces
	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public void Add(IWorkspace workspace)
	{
		Logger.Debug($"Adding workspace {workspace}");
		_workspaces.Add(workspace);
		WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs(workspace));
	}

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public bool Remove(IWorkspace workspace)
	{
		Logger.Debug($"Removing workspace {workspace}");

		if (_workspaces.Count - 1 <= _configContext.MonitorManager.Length)
		{
			Logger.Debug($"There must be at least {_configContext.MonitorManager.Length} workspaces.");
			return false;
		}

		bool wasFound = _workspaces.Remove(workspace);

		if (!wasFound)
		{
			Logger.Debug($"Workspace {workspace} was not found");
			return false;
		}

		WorkspaceRemoved?.Invoke(this, new WorkspaceEventArgs(workspace));

		// Remap windows to the last workspace
		IWorkspace lastWorkspace = _workspaces[^1];

		foreach (IWindow window in workspace.Windows)
		{
			lastWorkspace.AddWindow(window);
			_windowWorkspaceMap[window] = lastWorkspace;
		}

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

	public void Activate(IWorkspace workspace, IMonitor? focusedMonitor = null)
	{
		Logger.Debug($"Activating workspace {workspace}");

		focusedMonitor ??= _configContext.MonitorManager.FocusedMonitor;

		// Get the old workspace for the event.
		_monitorWorkspaceMap.TryGetValue(focusedMonitor, out IWorkspace? oldWorkspace);

		// Hide all the windows from the old workspace.
		oldWorkspace?.Deactivate();

		// Update the monitor which just lost `workspace`.
		IMonitor? loserMonitor = _monitorWorkspaceMap.Keys.FirstOrDefault(m => _monitorWorkspaceMap[m] == workspace);

		if (loserMonitor != null && oldWorkspace != null)
		{
			_monitorWorkspaceMap[loserMonitor] = oldWorkspace;
			oldWorkspace.DoLayout();
			MonitorWorkspaceChanged?.Invoke(this, new MonitorWorkspaceChangedEventArgs(loserMonitor,
																					  oldWorkspace: workspace,
																					  newWorkspace: oldWorkspace));
		}

		// Update the focused monitor.
		_monitorWorkspaceMap[focusedMonitor] = workspace;
		workspace.DoLayout();
		workspace.FocusFirstWindow();
		MonitorWorkspaceChanged?.Invoke(this, new MonitorWorkspaceChangedEventArgs(focusedMonitor, oldWorkspace, workspace));
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for active workspace {workspace}");

		// Linear search for the monitor that contains the workspace.
		foreach (IMonitor monitor in _configContext.MonitorManager)
		{
			if (_monitorWorkspaceMap[monitor] == workspace)
			{
				Logger.Debug($"Found monitor {monitor} for workspace {workspace}");
				return monitor;
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
	private void WindowManager_WindowRegistered(object? sender, WindowEventArgs args)
	{
		Logger.Debug($"Registering window {args}");

		IWindow window = args.Window;

		if (ActiveWorkspace == null)
		{
			Logger.Error($"No active workspace found.");
			return;
		}

		_windowWorkspaceMap[window] = ActiveWorkspace;
		ActiveWorkspace?.AddWindow(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowAdded(window, ActiveWorkspace!));
		Logger.Debug($"Window {window} registered to workspace {ActiveWorkspace!.Name}");
	}

	private void WindowManager_WindowUnregistered(object? sender, WindowEventArgs args)
	{
		Logger.Debug($"Window unregistered: {args}");

		IWindow window = args.Window;

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.RemoveWindow(window);
		_windowWorkspaceMap.Remove(window);
		WindowRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
	}
	#endregion

	#region Monitors
	public void AddProxyLayoutEngine(ProxyLayoutEngine proxyLayoutEngine)
	{
		_proxyLayoutEngines.Add(proxyLayoutEngine);
	}

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor)
	{
		return _monitorWorkspaceMap[monitor];
	}

	public IMonitor? GetMonitorForWindow(IWindow window)
	{
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace) ? GetMonitorForWorkspace(workspace) : null;
	}
	#endregion

	public void TriggerActiveLayoutEngineChanged(ActiveLayoutEngineChangedEventArgs args)
	{
		ActiveLayoutEngineChanged?.Invoke(this, args);
	}

	public void TriggerWorkspaceRenamed(WorkspaceRenamedEventArgs args)
	{
		WorkspaceRenamed?.Invoke(this, args);
	}

	public void MoveWindowToWorkspace(IWorkspace workspace, IWindow? window = null)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;
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

		if (ActiveWorkspace == workspace)
		{
			return;
		}

		_windowWorkspaceMap[window] = workspace;
		ActiveWorkspace.RemoveWindow(window);
		workspace.AddWindow(window);
	}

	public void MoveWindowToMonitor(IMonitor monitor, IWindow? window = null)
	{
		window ??= ActiveWorkspace.LastFocusedWindow;
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
		IMonitor monitor = _configContext.MonitorManager.FocusedMonitor;
		IMonitor previousMonitor = _configContext.MonitorManager.GetPreviousMonitor(monitor);

		MoveWindowToMonitor(previousMonitor, window);
	}

	public void MoveWindowToNextMonitor(IWindow? window = null)
	{
		Logger.Debug($"Moving window {window} to next monitor");

		// Get the next monitor.
		IMonitor monitor = _configContext.MonitorManager.FocusedMonitor;
		IMonitor nextMonitor = _configContext.MonitorManager.GetNextMonitor(monitor);

		MoveWindowToMonitor(nextMonitor, window);
	}

	public void MoveWindowToPoint(IWindow window, IPoint<int> location)
	{
		Logger.Debug($"Moving window {window} to location {location}");

		// Get the monitor.
		IMonitor targetMonitor = _configContext.MonitorManager.GetMonitorAtPoint(location);

		// Get the target workspace.
		IWorkspace? targetWorkspace = GetWorkspaceForMonitor(targetMonitor);
		if (targetWorkspace == null)
		{
			Logger.Error($"Monitor {targetMonitor} was not found to correspond to any workspace");
			return;
		}

		Logger.Debug($"Moving window {window} to workspace {targetWorkspace} in monitor {targetMonitor}");
		Logger.Debug($"Active workspace is {ActiveWorkspace}");

		bool isPhantom = _phantomWindows.Contains(window);
		if (isPhantom && targetWorkspace != ActiveWorkspace)
		{
			Logger.Error($"Window {window} is a phantom window and cannot be moved");
			return;
		}

		if (!ActiveWorkspace.RemoveWindow(window))
		{
			Logger.Error($"Could not remove window {window} from workspace {ActiveWorkspace}");
			return;
		}

		IPoint<double> normalized = targetMonitor.ToUnitSquare(location);
		Logger.Verbose($"Normalized location: {normalized}");

		targetWorkspace.MoveWindowToPoint(window, normalized, isPhantom);
		_windowWorkspaceMap[window] = targetWorkspace;

		// Trigger layouts.
		ActiveWorkspace.DoLayout();
		targetWorkspace.DoLayout();
		window.Focus();
	}

	#region Phantom Windows
	public void RegisterPhantomWindow(IWorkspace workspace, IWindow window)
	{
		Logger.Debug($"Registering phantom window {window} to workspace {workspace}");
		_phantomWindows.Add(window);
		_windowWorkspaceMap[window] = workspace;
	}

	public void UnregisterPhantomWindow(IWindow window)
	{
		Logger.Debug($"Unregistering phantom window {window}");
		_phantomWindows.Remove(window);
		_windowWorkspaceMap.Remove(window);
	}
	#endregion
}
