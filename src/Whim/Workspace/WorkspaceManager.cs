using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal record WorkspaceToCreate(string Name, IEnumerable<CreateLeafLayoutEngine>? LayoutEngines);

internal class WorkspaceManager : IWorkspaceManager
{
	private bool _initialized;
	private readonly IContext _context;
	private readonly IInternalContext _internalContext;

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
			return _context
				.Store.Pick(Pickers.PickWorkspaceByMonitor(activeMonitor.Handle))
				.TryGet(out IWorkspace workspace)
				? workspace
				: _context.Store.Pick(new GetAllMutableWorkspacesPicker())[0];
		}
	}

	private bool _disposedValue;

	public WorkspaceManager(IContext context, IInternalContext internalContext)
	{
		_context = context;
		_internalContext = internalContext;
	}

	public IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null)
	{
		_context.Store.Dispatch(new AddWorkspaceTransform(name, createLayoutEngines));
	}

	public void AddProxyLayoutEngine(CreateProxyLayoutEngine proxyLayoutEngine)
	{
		Logger.Debug($"Adding proxy layout engine: {proxyLayoutEngine}");
		_proxyLayoutEngines.Add(proxyLayoutEngine);
	}

	public bool Contains(IWorkspace workspace) =>
		_context.Store.Pick(Pickers.PickAllWorkspaces()).Any(w => w.Id == workspace.Id);

	public IEnumerator<IWorkspace> GetEnumerator() => _context.Store.Pick(Pickers.PickAllWorkspaces()).GetEnumerator();

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

			_context.Store.Dispatch(new ActivateWorkspaceTransform(workspace, monitor));
			idx++;
		}
	}

	public bool Remove(IWorkspace workspace) =>
		_context.Store.Dispatch(new RemoveWorkspaceByIdTransform(workspace.Id)).IsSuccessful;

	public bool Remove(string workspaceName) =>
		_context.Store.Dispatch(new RemoveWorkspaceByNameTransform(workspaceName)).IsSuccessful;

	public IWorkspace? TryGet(string workspaceName) =>
		_context.Store.Pick(Pickers.GetWorkspaceById(workspaceName)).TryGet(out Workspace workspace) ? workspace : null;

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
