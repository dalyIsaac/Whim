using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class WorkspaceManager2 : IWorkspaceManager2
{
	private bool _initialized;

	private readonly IContext _context;

	private readonly IInternalContext _internalContext;

	/// <summary>
	/// The <see cref="IWorkspace"/>s stored by this manager.
	/// </summary>
	protected readonly List<IWorkspace> _workspaces = new();

	/// <summary>
	/// Stores the workspaces to create, when <see cref="Initialize"/> is called.
	/// The workspaces will have been created prior to <see cref="Initialize"/>.
	/// </summary>
	private readonly List<WorkspaceToCreate> _workspacesToCreate = new();

	private readonly List<CreateProxyLayoutEngine> _proxyLayoutEngines = new();

	public WorkspaceManager2(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public IWorkspace ActiveWorkspace
	{
		get
		{
			IMonitor activeMonitor = _context.MonitorManager.ActiveMonitor;
			Logger.Debug($"Getting active workspace for monitor {activeMonitor}");
			return _context.Butler.GetWorkspaceForMonitor(activeMonitor) ?? _workspaces[0];
		}
	}

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

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

	public bool Contains(IWorkspace workspace) => _workspaces.Contains(workspace);

	public IEnumerator<IWorkspace> GetEnumerator() => _workspaces.GetEnumerator();

	public void Initialize()
	{
		Logger.Debug("Initializing workspace manager");

		_initialized = true;

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

			_context.Butler.Activate(workspace, monitor);
			idx++;
		}
	}

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
			new(_context, _internalContext, name ?? $"Workspace {_workspaces.Count + 1}", layoutEngines);
		_workspaces.Add(workspace);
		WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
		return workspace;
	}

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

		_context.Butler.MergeWorkspaceWindows(workspace, _workspaces[^1]);
		_context.Butler.Activate(_workspaces[^1]);
		WorkspaceRemoved?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });

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
