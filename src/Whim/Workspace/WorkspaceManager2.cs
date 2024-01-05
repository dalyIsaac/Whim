using System;
using System.Collections;
using System.Collections.Generic;

namespace Whim;

internal class WorkspaceManager2 : IWorkspaceManager2
{
	private bool _initialized;

	private readonly IContext _context;

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	protected readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// Maps windows to their workspace.
	/// </summary>
	private readonly Dictionary<IWindow, IWorkspace> _windowWorkspaceMap = new();

	/// <summary>
	/// Maps monitors to their active workspace.
	/// </summary>
	private readonly Dictionary<IMonitor, IWorkspace> _monitorWorkspaceMap = new();

	/// <summary>
	/// Stores the workspaces to create, when <see cref="Initialize"/> is called.
	/// The workspaces will have been created prior to <see cref="Initialize"/>.
	/// </summary>
	private readonly List<WorkspaceToCreate> _workspacesToCreate = new();

	public WorkspaceManager2(IContext context)
	{
		_context = context;
	}

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

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

	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;
	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;
	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;
	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;
	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;
	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

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

	public IWorkspace? GetAdjacentWorkspace(IWorkspace workspace, bool reverse, bool skipActive)
	{
		int idx = _workspaces.IndexOf(workspace);
		int delta = reverse ? -1 : 1;
		int nextIdx = (idx + delta).Mod(_workspaces.Count);

		while (idx != nextIdx)
		{
			IWorkspace nextWorkspace = _workspaces[nextIdx];
			IMonitor? monitor = _context.Butler.GetMonitorForWorkspace(nextWorkspace);

			if (monitor == null || !skipActive)
			{
				return nextWorkspace;
			}

			nextIdx = (nextIdx + delta).Mod(_workspaces.Count);
		}

		return null;
	}

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();

	public void Initialize() => throw new NotImplementedException();

	public void OnWorkspaceAdded(WorkspaceEventArgs args) => WorkspaceAdded?.Invoke(this, args);

	public void OnWorkspaceRemoved(WorkspaceEventArgs args) => WorkspaceRemoved?.Invoke(this, args);

	public void OnWorkspaceRenamed(WorkspaceRenamedEventArgs args) => WorkspaceRenamed?.Invoke(this, args);

	public void OnWorkspaceLayoutStarted(WorkspaceEventArgs args) => WorkspaceLayoutStarted?.Invoke(this, args);

	public void OnWorkspaceLayoutCompleted(WorkspaceEventArgs args) => WorkspaceLayoutCompleted?.Invoke(this, args);

	public void OnActiveLayoutEngineChanged(ActiveLayoutEngineChangedEventArgs args) =>
		ActiveLayoutEngineChanged?.Invoke(this, args);

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

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
