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

	public IWorkspace? this[string workspaceName] => throw new NotImplementedException();

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

	// TODO: Move the mapping to the butler
	public bool Contains(IWorkspace workspace) => throw new NotImplementedException();

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

	public void OnWorkspaceAdded(WorkspaceEventArgs? args) => throw new NotImplementedException();

	public void OnWorkspaceRemoved(WorkspaceEventArgs? args) => throw new NotImplementedException();

	public void OnWorkspaceRenamed(WorkspaceRenamedEventArgs? args) => throw new NotImplementedException();

	public void OnWorkspaceLayoutStarted(WorkspaceEventArgs? args) => throw new NotImplementedException();

	public void OnWorkspaceLayoutCompleted(WorkspaceEventArgs? args) => throw new NotImplementedException();

	public void OnActiveLayoutEngineChanged(ActiveLayoutEngineChangedEventArgs? args) =>
		throw new NotImplementedException();

	public bool Remove(IWorkspace workspace) => throw new NotImplementedException();

	public bool Remove(string workspaceName) => throw new NotImplementedException();

	public IWorkspace? TryGet(string workspaceName) => throw new NotImplementedException();

	IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

	IWorkspace? IWorkspaceManager2.Add(string? name, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines) =>
		throw new NotImplementedException();
}
