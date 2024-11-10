using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Whim.Bar;

/// <summary>
/// View model containing the workspaces for a given monitor.
/// </summary>
internal class WorkspaceWidgetViewModel : IDisposable
{
	private readonly IContext _context;
	private bool _disposedValue;

	/// <summary>
	/// The monitor which contains the workspaces.
	/// </summary>
	public IMonitor Monitor { get; }

	/// <summary>
	/// The workspaces for the monitor.
	/// </summary>
	public VeryObservableCollection<WorkspaceModel> Workspaces { get; } = [];

	/// <summary>
	/// Creates a new instance of <see cref="WorkspaceWidgetViewModel"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="monitor"></param>
	public WorkspaceWidgetViewModel(IContext context, IMonitor monitor)
	{
		_context = context;
		Monitor = monitor;

		_context.Store.WorkspaceEvents.WorkspaceAdded += WorkspaceEvents_WorkspaceAdded;
		_context.Store.WorkspaceEvents.WorkspaceRemoved += WorkspaceEvents_WorkspaceRemoved;
		_context.Store.MapEvents.MonitorWorkspaceChanged += MapEvents_MonitorWorkspaceChanged;
		_context.Store.WorkspaceEvents.WorkspaceRenamed += WorkspaceEvents_WorkspaceRenamed;

		UpdateWorkspacesCollection();
	}

	private void UpdateWorkspacesCollection()
	{
		Workspaces.Clear();

		IReadOnlyList<IWorkspace> workspaces =
			_context.Store.Pick(Pickers.PickStickyWorkspacesByMonitor(Monitor.Handle)).ValueOrDefault ?? [];

		foreach (IWorkspace workspace in workspaces)
		{
			IMonitor? monitorForWorkspace = _context
				.Store.Pick(Pickers.PickMonitorByWorkspace(workspace.Id))
				.ValueOrDefault;

			Workspaces.Add(
				new WorkspaceModel(_context, this, workspace, Monitor.Handle == monitorForWorkspace?.Handle)
			);
		}
	}

	private void WorkspaceEvents_WorkspaceAdded(object? sender, WorkspaceEventArgs args) =>
		UpdateWorkspacesCollection();

	private void WorkspaceEvents_WorkspaceRemoved(object? sender, WorkspaceEventArgs args) =>
		UpdateWorkspacesCollection();

	private void MapEvents_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs args)
	{
		if (args.Monitor.Handle != Monitor.Handle)
		{
			return;
		}

		foreach (WorkspaceModel workspaceModel in Workspaces)
		{
			workspaceModel.ActiveOnMonitor = workspaceModel.Workspace.Id == args.CurrentWorkspace.Id;
		}
	}

	private void WorkspaceEvents_WorkspaceRenamed(object? sender, WorkspaceRenamedEventArgs e)
	{
		WorkspaceModel? workspace = Workspaces.FirstOrDefault(m => m.Workspace.Id == e.Workspace.Id);
		if (workspace == null)
		{
			return;
		}

		workspace.Workspace_Renamed(sender, e);
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_context.Store.WorkspaceEvents.WorkspaceAdded -= WorkspaceEvents_WorkspaceAdded;
				_context.Store.WorkspaceEvents.WorkspaceRemoved -= WorkspaceEvents_WorkspaceRemoved;
				_context.Store.MapEvents.MonitorWorkspaceChanged -= MapEvents_MonitorWorkspaceChanged;
				_context.Store.WorkspaceEvents.WorkspaceRenamed -= WorkspaceEvents_WorkspaceRenamed;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
