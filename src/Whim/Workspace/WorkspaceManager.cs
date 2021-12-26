using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

using ProxyLayoutEngine = Func<ILayoutEngine, ILayoutEngine>;

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
	/// Maps monitors to their workspace.
	/// </summary>
	private readonly Dictionary<IMonitor, IWorkspace> _monitorWorkspaceMap = new();

	public event EventHandler<WorkspaceMonitorChangedEventArgs>? WorkspaceMonitorChanged;

	public event EventHandler<RouteEventArgs>? WorkspaceRouted;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	/// <summary>
	/// The active workspace.
	/// </summary>
	public IWorkspace? ActiveWorkspace { get; private set; }

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

		// Set the active workspace.
		ActiveWorkspace = _workspaces[0];

		// Subscribe to WindowRegistered event.
		_configContext.WindowManager.WindowRegistered += WindowRegisteredEventHandler;
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

		if (_workspaces.Count <= _configContext.MonitorManager.Length)
		{
			throw new InvalidOperationException($"There must be at least {_configContext.MonitorManager.Length} workspaces.");
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

	public void Activate(IWorkspace workspace, IMonitor? monitor = null)
	{
		Logger.Debug($"Activating workspace {workspace.Name}");

		if (monitor == null)
		{
			monitor = _configContext.MonitorManager.FocusedMonitor;
		}

		// Get the old workspace for the event.
		_monitorWorkspaceMap.TryGetValue(monitor, out IWorkspace? oldWorkspace);

		// Change the workspace.
		_monitorWorkspaceMap[monitor] = workspace;

		// Update the layout.
		workspace.DoLayout();

		// Fire the event.
		WorkspaceMonitorChanged?.Invoke(this, new WorkspaceMonitorChangedEventArgs(monitor, oldWorkspace, workspace));
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for active workspace {workspace.Name}");

		// Linear search for the monitor that contains the workspace.
		foreach (IMonitor monitor in _configContext.MonitorManager)
		{
			if (_monitorWorkspaceMap[monitor] == workspace)
			{
				return monitor;
			}
		}

		return null;
	}
	#endregion

	#region Windows
	internal void WindowRegisteredEventHandler(object sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		window.WindowUnregistered += WindowUnregisteredEventHandler;

		if (ActiveWorkspace == null || !window.IsFocused)
		{
			return;
		}

		_windowWorkspaceMap[window] = ActiveWorkspace;
		ActiveWorkspace?.AddWindow(window);
		WorkspaceRouted?.Invoke(this, RouteEventArgs.WindowAdded(window, ActiveWorkspace!));
	}

	internal void WindowUnregisteredEventHandler(object sender, WindowEventArgs args)
	{
		IWindow window = args.Window;
		window.WindowUnregistered -= WindowUnregisteredEventHandler;

		if (!_windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace))
		{
			Logger.Error($"Window {window} was not found in any workspace");
			return;
		}

		workspace.RemoveWindow(window);
		_windowWorkspaceMap.Remove(window);
		WorkspaceRouted?.Invoke(this, RouteEventArgs.WindowRemoved(window, workspace));
	}
	#endregion

	public void AddProxyLayoutEngine(ProxyLayoutEngine proxyLayoutEngine)
	{
		_proxyLayoutEngines.Add(proxyLayoutEngine);
	}
}
