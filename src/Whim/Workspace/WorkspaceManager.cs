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

internal record WorkspaceToCreate(string Name, IEnumerable<CreateLeafLayoutEngine>? LayoutEngines);

internal class WorkspaceManager : IWorkspaceManager
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
	/// Stores the workspaces to create, when <see cref="Initialize"/> is called.
	/// The workspaces will have been created prior to <see cref="Initialize"/>.
	/// </summary>
	private readonly List<WorkspaceToCreate> _workspacesToCreate = new();

	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

	private readonly List<CreateProxyLayoutEngine> _proxyLayoutEngines = new();

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public IWorkspace ActiveWorkspace
	{
		get
		{
			IMonitor activeMonitor = _context.MonitorManager.ActiveMonitor;
			Logger.Debug($"Getting active workspace for monitor {activeMonitor}");
			return _context.Butler.Pantry.GetWorkspaceForMonitor(activeMonitor) ?? _workspaces[0];
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

	public IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null)
	{
		if (_initialized)
		{
			return CreateWorkspace(name, createLayoutEngines);
		}

		_workspacesToCreate.Add(new(name ?? $"Workspace {_workspaces.Count + 1}", createLayoutEngines));
		return null;
	}

	public void AddProxyLayoutEngine(CreateProxyLayoutEngine proxyLayoutEngine)
	{
		Logger.Debug($"Adding proxy layout engine: {proxyLayoutEngine}");
		_proxyLayoutEngines.Add(proxyLayoutEngine);
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
			new(_context, _internalContext, _triggers, name ?? $"Workspace {_workspaces.Count + 1}", layoutEngines);
		_workspaces.Add(workspace);
		WorkspaceAdded?.Invoke(this, new WorkspaceEventArgs() { Workspace = workspace });
		return workspace;
	}

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
