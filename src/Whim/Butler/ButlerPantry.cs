using System;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class ButlerPantry : IButlerPantry
{
	private readonly IContext _context;
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();
	private readonly Dictionary<IMonitor, IWorkspace> _monitorWorkspaceMap = new();

	public ButlerPantry(IContext context)
	{
		_context = context;
	}

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse = false, bool skipActive = false)
	{
		IWorkspace[] workspaces = _context.WorkspaceManager.ToArray();

		int idx = Array.IndexOf(workspaces, workspace);
		int delta = reverse ? -1 : 1;
		int nextIdx = (idx + delta).Mod(workspaces.Length);

		while (idx != nextIdx)
		{
			IWorkspace nextWorkspace = workspaces[nextIdx];
			IMonitor? monitor = GetMonitorForWorkspace(nextWorkspace);

			if (monitor == null || !skipActive)
			{
				return nextWorkspace;
			}

			nextIdx = (nextIdx + delta).Mod(workspaces.Length);
		}

		return null;
	}

	public IEnumerable<IWorkspace> GetAllActiveWorkspaces()
	{
		Logger.Debug("Getting all active workspaces");
		return _monitorWorkspaceMap.Values;
	}

	public IMonitor? GetMonitorForWindow(IWindow window)
	{
		Logger.Debug($"Getting monitor for window: {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace)
			? GetMonitorForWorkspace(workspace)
			: null;
	}

	public IMonitor? GetMonitorForWorkspace(IWorkspace workspace)
	{
		Logger.Debug($"Getting monitor for workspace {workspace}");

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

	public IWorkspace? GetWorkspaceForMonitor(IMonitor monitor)
	{
		Logger.Debug($"Getting workspace for monitor {monitor}");
		return _monitorWorkspaceMap.TryGetValue(monitor, out IWorkspace? workspace) ? workspace : null;
	}

	public IWorkspace? GetWorkspaceForWindow(IWindow window)
	{
		Logger.Debug($"Getting workspace for window {window}");
		return _windowWorkspaceMap.TryGetValue(window, out IWorkspace? workspace) ? workspace : null;
	}

	public bool RemoveMonitor(IMonitor monitor)
	{
		Logger.Debug($"Removing monitor {monitor}");
		return _monitorWorkspaceMap.Remove(monitor);
	}

	public bool RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window}");
		return _windowWorkspaceMap.Remove(window);
	}

	public void MergeWorkspaceWindows(IWorkspace workspaceToDelete, IWorkspace workspaceMergeTarget)
	{
		Logger.Debug($"Removing workspace {workspaceToDelete} and moving windows to {workspaceMergeTarget}");

		// Remove the workspace from the monitor map.
		IMonitor? monitor = GetMonitorForWorkspace(workspaceToDelete);
		if (monitor != null)
		{
			_monitorWorkspaceMap.Remove(monitor);
		}

		// Remap windows to the first workspace which isn't active.
		foreach (IWindow window in workspaceToDelete.Windows)
		{
			_windowWorkspaceMap[window] = workspaceMergeTarget;
		}
	}

	public void SetWindowWorkspace(IWindow window, IWorkspace workspace)
	{
		Logger.Debug($"Setting window {window} to workspace {workspace}");
		_windowWorkspaceMap[window] = workspace;
	}

	public void SetMonitorWorkspace(IMonitor monitor, IWorkspace workspace)
	{
		Logger.Debug($"Setting workspace {workspace} to monitor {monitor}");
		_monitorWorkspaceMap[monitor] = workspace;
	}
}
