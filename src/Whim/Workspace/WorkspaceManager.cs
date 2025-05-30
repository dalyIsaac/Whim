using System.Collections;
using System.Linq;

namespace Whim;

internal class WorkspaceManager(IContext context) : IWorkspaceManager
{
	private readonly IContext _ctx = context;

	public event EventHandler<WorkspaceAddedEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceRemovedEventArgs>? WorkspaceRemoved;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<WorkspaceLayoutStartedEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceLayoutCompletedEventArgs>? WorkspaceLayoutCompleted;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines
	{
		get => _ctx.Store.Pick(PickCreateLeafLayoutEngines());
		set => _ctx.Store.WhimDispatch(new SetCreateLayoutEnginesTransform(value));
	}

	public IWorkspace? this[string workspaceName] => TryGet(workspaceName);

	public IWorkspace ActiveWorkspace
	{
		get
		{
			IMonitor activeMonitor = _ctx.MonitorManager.ActiveMonitor;
			Logger.Debug($"Getting active workspace for monitor {activeMonitor}");
			return _ctx.Store.Pick(PickActiveWorkspace());
		}
	}

	public WorkspaceId? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null) =>
		_ctx.Store.WhimDispatch(new AddWorkspaceTransform(name, createLayoutEngines)).TryGet(out WorkspaceId id)
			? id
			: null;

	public void AddProxyLayoutEngine(ProxyLayoutEngineCreator proxyLayoutEngineCreator) =>
		_ctx.Store.WhimDispatch(new AddProxyLayoutEngineTransform(proxyLayoutEngineCreator));

	public bool Contains(IWorkspace workspace) => _ctx.Store.Pick(PickWorkspaces()).Any(w => w.Id == workspace.Id);

	public IEnumerator<IWorkspace> GetEnumerator() => _ctx.Store.Pick(PickWorkspaces()).GetEnumerator();

	public void Initialize()
	{
		_ctx.Store.WorkspaceEvents.WorkspaceAdded += WorkspaceSector_WorkspaceAdded;
		_ctx.Store.WorkspaceEvents.WorkspaceRemoved += WorkspaceSector_WorkspaceRemoved;
		_ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged += WorkspaceSector_ActiveLayoutEngineChanged;
		_ctx.Store.WorkspaceEvents.WorkspaceRenamed += WorkspaceSector_WorkspaceRenamed;
		_ctx.Store.WorkspaceEvents.WorkspaceLayoutStarted += WorkspaceSector_WorkspaceLayoutStarted;
		_ctx.Store.WorkspaceEvents.WorkspaceLayoutCompleted += WorkspaceSector_WorkspaceLayoutCompleted;
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
		_ctx.Store.WhimDispatch(new RemoveWorkspaceByIdTransform(workspace.Id)).IsSuccessful;

	public bool Remove(string workspaceName) =>
		_ctx.Store.WhimDispatch(new RemoveWorkspaceByNameTransform(workspaceName)).IsSuccessful;

	public IWorkspace? TryGet(string workspaceName) =>
		_ctx.Store.Pick(PickWorkspaceByName(workspaceName)).TryGet(out IWorkspace workspace) ? workspace : null;

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Dispose()
	{
		_ctx.Store.WorkspaceEvents.WorkspaceAdded -= WorkspaceSector_WorkspaceAdded;
		_ctx.Store.WorkspaceEvents.WorkspaceRemoved -= WorkspaceSector_WorkspaceRemoved;
		_ctx.Store.WorkspaceEvents.ActiveLayoutEngineChanged -= WorkspaceSector_ActiveLayoutEngineChanged;
		_ctx.Store.WorkspaceEvents.WorkspaceRenamed -= WorkspaceSector_WorkspaceRenamed;
		_ctx.Store.WorkspaceEvents.WorkspaceLayoutStarted -= WorkspaceSector_WorkspaceLayoutStarted;
		_ctx.Store.WorkspaceEvents.WorkspaceLayoutCompleted -= WorkspaceSector_WorkspaceLayoutCompleted;
	}
}
