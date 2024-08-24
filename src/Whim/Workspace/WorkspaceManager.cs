using System.Collections;
using System.Linq;

namespace Whim;

internal class WorkspaceManager(IContext context) : IWorkspaceManager
{
	private readonly IContext _context = context;

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines
	{
		get => _context.Store.Pick(PickCreateLeafLayoutEngines());
		set => _context.Store.Dispatch(new SetCreateLayoutEnginesTransform(value));
	}

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public IWorkspace ActiveWorkspace
	{
		get
		{
			IMonitor activeMonitor = _context.MonitorManager.ActiveMonitor;
			Logger.Debug($"Getting active workspace for monitor {activeMonitor}");
			return _context.Store.Pick(PickActiveWorkspace());
		}
	}

	public WorkspaceId? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null) =>
		_context.Store.Dispatch(new AddWorkspaceTransform(name, createLayoutEngines)).TryGet(out WorkspaceId id)
			? id
			: null;

	public void AddProxyLayoutEngine(ProxyLayoutEngineCreator proxyLayoutEngineCreator) =>
		_context.Store.Dispatch(new AddProxyLayoutEngineTransform(proxyLayoutEngineCreator));

	public bool Contains(IWorkspace workspace) => _context.Store.Pick(PickWorkspaces()).Any(w => w.Id == workspace.Id);

	public IEnumerator<IWorkspace> GetEnumerator() => _context.Store.Pick(PickWorkspaces()).GetEnumerator();

	public void Initialize()
	{
		_context.Store.WorkspaceEvents.WorkspaceAdded += WorkspaceSector_WorkspaceAdded;
		_context.Store.WorkspaceEvents.WorkspaceRemoved += WorkspaceSector_WorkspaceRemoved;
		_context.Store.WorkspaceEvents.ActiveLayoutEngineChanged += WorkspaceSector_ActiveLayoutEngineChanged;
		_context.Store.WorkspaceEvents.WorkspaceRenamed += WorkspaceSector_WorkspaceRenamed;
		_context.Store.WorkspaceEvents.WorkspaceLayoutStarted += WorkspaceSector_WorkspaceLayoutStarted;
		_context.Store.WorkspaceEvents.WorkspaceLayoutCompleted += WorkspaceSector_WorkspaceLayoutCompleted;
	}

	private void WorkspaceSector_WorkspaceAdded(object? sender, WorkspaceAddedEventArgs args) =>
		WorkspaceAdded?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceRemoved(object? sender, WorkspaceRemovedEventArgs args) =>
		WorkspaceRemoved?.Invoke(sender, args);

	private void WorkspaceSector_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs args) =>
		ActiveLayoutEngineChanged?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceRenamed(object? sender, WorkspaceRenamedEventArgs args) =>
		WorkspaceRenamed?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceLayoutStarted(object? sender, WorkspaceLayoutStartedEventArgs args) =>
		WorkspaceLayoutStarted?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceLayoutCompleted(object? sender, WorkspaceLayoutCompletedEventArgs args) =>
		WorkspaceLayoutCompleted?.Invoke(sender, args);

	public bool Remove(IWorkspace workspace) =>
		_context.Store.Dispatch(new RemoveWorkspaceByIdTransform(workspace.Id)).IsSuccessful;

	public bool Remove(string workspaceName) =>
		_context.Store.Dispatch(new RemoveWorkspaceByNameTransform(workspaceName)).IsSuccessful;

	public IWorkspace? TryGet(string workspaceName) =>
		_context.Store.Pick(PickWorkspaceByName(workspaceName)).TryGet(out IWorkspace workspace) ? workspace : null;

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Dispose()
	{
		_context.Store.WorkspaceEvents.WorkspaceAdded -= WorkspaceSector_WorkspaceAdded;
		_context.Store.WorkspaceEvents.WorkspaceRemoved -= WorkspaceSector_WorkspaceRemoved;
		_context.Store.WorkspaceEvents.ActiveLayoutEngineChanged -= WorkspaceSector_ActiveLayoutEngineChanged;
		_context.Store.WorkspaceEvents.WorkspaceRenamed -= WorkspaceSector_WorkspaceRenamed;
		_context.Store.WorkspaceEvents.WorkspaceLayoutStarted -= WorkspaceSector_WorkspaceLayoutStarted;
		_context.Store.WorkspaceEvents.WorkspaceLayoutCompleted -= WorkspaceSector_WorkspaceLayoutCompleted;
	}
}
