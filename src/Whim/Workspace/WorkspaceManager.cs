using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Whim;

internal class WorkspaceManager : IWorkspaceManager
{
	private readonly IContext _context;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceAdded;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceRemoved;

	public event EventHandler<ActiveLayoutEngineChangedEventArgs>? ActiveLayoutEngineChanged;

	public event EventHandler<WorkspaceRenamedEventArgs>? WorkspaceRenamed;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutStarted;

	public event EventHandler<WorkspaceEventArgs>? WorkspaceLayoutCompleted;

	public Func<CreateLeafLayoutEngine[]> CreateLayoutEngines { get; set; } =
		() => new CreateLeafLayoutEngine[] { (id) => new ColumnLayoutEngine(id) };

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
				: _context.Store.Pick(Pickers.PickAllWorkspaces()).First();
		}
	}

	public WorkspaceManager(IContext context)
	{
		_context = context;
	}

	public IWorkspace? Add(string? name = null, IEnumerable<CreateLeafLayoutEngine>? createLayoutEngines = null) =>
		_context.Store.Dispatch(new AddWorkspaceTransform(name, createLayoutEngines)).OrDefault();

	public void AddProxyLayoutEngine(ProxyLayoutEngineCreator proxyLayoutEngineCreator) =>
		_context.Store.Dispatch(new AddProxyLayoutEngineTransform(proxyLayoutEngineCreator));

	public bool Contains(IWorkspace workspace) =>
		_context.Store.Pick(Pickers.PickAllWorkspaces()).Any(w => w.Id == workspace.Id);

	public IEnumerator<IWorkspace> GetEnumerator() => _context.Store.Pick(Pickers.PickAllWorkspaces()).GetEnumerator();

	public void Initialize()
	{
		_context.Store.WorkspaceEvents.WorkspaceAdded += WorkspaceSector_WorkspaceAdded;
		_context.Store.WorkspaceEvents.WorkspaceRemoved += WorkspaceSector_WorkspaceRemoved;
		_context.Store.WorkspaceEvents.ActiveLayoutEngineChanged += WorkspaceSector_ActiveLayoutEngineChanged;
		_context.Store.WorkspaceEvents.WorkspaceRenamed += WorkspaceSector_WorkspaceRenamed;
		_context.Store.WorkspaceEvents.WorkspaceLayoutStarted += WorkspaceSector_WorkspaceLayoutStarted;
		_context.Store.WorkspaceEvents.WorkspaceLayoutCompleted += WorkspaceSector_WorkspaceLayoutCompleted;
	}

	private void WorkspaceSector_WorkspaceAdded(object? sender, WorkspaceEventArgs args) =>
		WorkspaceAdded?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceRemoved(object? sender, WorkspaceEventArgs args) =>
		WorkspaceRemoved?.Invoke(sender, args);

	private void WorkspaceSector_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs args) =>
		ActiveLayoutEngineChanged?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceRenamed(object? sender, WorkspaceRenamedEventArgs args) =>
		WorkspaceRenamed?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceLayoutStarted(object? sender, WorkspaceEventArgs args) =>
		WorkspaceLayoutStarted?.Invoke(sender, args);

	private void WorkspaceSector_WorkspaceLayoutCompleted(object? sender, WorkspaceEventArgs args) =>
		WorkspaceLayoutCompleted?.Invoke(sender, args);

	public bool Remove(IWorkspace workspace) =>
		_context.Store.Dispatch(new RemoveWorkspaceByIdTransform(workspace.Id)).IsSuccessful;

	public bool Remove(string workspaceName) =>
		_context.Store.Dispatch(new RemoveWorkspaceByNameTransform(workspaceName)).IsSuccessful;

	public IWorkspace? TryGet(string workspaceName) =>
		_context.Store.Pick(Pickers.PickWorkspaceByName(workspaceName)).TryGet(out IWorkspace workspace)
			? workspace
			: null;

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
