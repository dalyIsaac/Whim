using System;
using System.Linq;

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
	public VeryObservableCollection<WorkspaceModel> Workspaces { get; } = new();

	/// <summary>
	/// Creates a new instance of <see cref="WorkspaceWidgetViewModel"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="monitor"></param>
	public WorkspaceWidgetViewModel(IContext context, IMonitor monitor)
	{
		_context = context;
		Monitor = monitor;

		_context.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		_context.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		_context.Butler.MonitorWorkspaceChanged += Butler_MonitorWorkspaceChanged;
		_context.WorkspaceManager.WorkspaceRenamed += WorkspaceManager_WorkspaceRenamed;

		// Populate the list of workspaces
		foreach (IWorkspace workspace in _context.WorkspaceManager)
		{
			IMonitor? monitorForWorkspace = _context.Butler.Pantry.GetMonitorForWorkspace(workspace);
			Workspaces.Add(new WorkspaceModel(context, this, workspace, Monitor.Equals(monitorForWorkspace)));
		}
	}

	private void WorkspaceManager_WorkspaceAdded(object? sender, WorkspaceEventArgs args)
	{
		if (Workspaces.Any(model => model.Workspace.Equals(args.Workspace)))
		{
			return;
		}

		IMonitor? monitorForWorkspace = _context.Butler.Pantry.GetMonitorForWorkspace(args.Workspace);
		Workspaces.Add(new WorkspaceModel(_context, this, args.Workspace, Monitor.Equals(monitorForWorkspace)));
	}

	private void WorkspaceManager_WorkspaceRemoved(object? sender, WorkspaceEventArgs args)
	{
		WorkspaceModel? workspaceModel = Workspaces.FirstOrDefault(model => model.Workspace.Equals(args.Workspace));
		if (workspaceModel == null)
		{
			return;
		}

		Workspaces.Remove(workspaceModel);
	}

	private void Butler_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs args)
	{
		if (!args.Monitor.Equals(Monitor))
		{
			return;
		}

		foreach (WorkspaceModel workspaceModel in Workspaces)
		{
			workspaceModel.ActiveOnMonitor = workspaceModel.Workspace.Equals(args.CurrentWorkspace);
		}
	}

	private void WorkspaceManager_WorkspaceRenamed(object? sender, WorkspaceRenamedEventArgs e)
	{
		WorkspaceModel? workspace = Workspaces.FirstOrDefault(m => m.Workspace.Equals(e.Workspace));
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
				_context.WorkspaceManager.WorkspaceAdded -= WorkspaceManager_WorkspaceAdded;
				_context.WorkspaceManager.WorkspaceRemoved -= WorkspaceManager_WorkspaceRemoved;
				_context.Butler.MonitorWorkspaceChanged -= Butler_MonitorWorkspaceChanged;
				_context.WorkspaceManager.WorkspaceRenamed -= WorkspaceManager_WorkspaceRenamed;
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
