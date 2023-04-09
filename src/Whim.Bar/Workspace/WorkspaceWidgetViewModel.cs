using System;
using System.ComponentModel;
using System.Linq;

namespace Whim.Bar;

/// <summary>
/// View model containing the workspaces for a given monitor.
/// </summary>
public class WorkspaceWidgetViewModel : INotifyPropertyChanged, IDisposable
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
		_context.WorkspaceManager.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;
		_context.WorkspaceManager.WorkspaceRenamed += WorkspaceManager_WorkspaceRenamed;

		// Populate the list of workspaces
		foreach (IWorkspace workspace in _context.WorkspaceManager)
		{
			IMonitor? monitorForWorkspace = _context.WorkspaceManager.GetMonitorForWorkspace(workspace);
			Workspaces.Add(new WorkspaceModel(context, this, workspace, Monitor == monitorForWorkspace));
		}
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void WorkspaceManager_WorkspaceAdded(object? sender, WorkspaceEventArgs args)
	{
		if (Workspaces.Any(w => w.Name == args.Workspace.Name))
		{
			return;
		}

		IMonitor? monitorForWorkspace = _context.WorkspaceManager.GetMonitorForWorkspace(args.Workspace);
		Workspaces.Add(new WorkspaceModel(_context, this, args.Workspace, Monitor == monitorForWorkspace));
	}

	private void WorkspaceManager_WorkspaceRemoved(object? sender, WorkspaceEventArgs args)
	{
		WorkspaceModel? workspace = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);
		if (workspace == null)
		{
			return;
		}

		Workspaces.Remove(workspace);
	}

	private void WorkspaceManager_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs args)
	{
		if (args.Monitor != Monitor)
		{
			return;
		}

		if (args.OldWorkspace != null)
		{
			WorkspaceModel? oldWorkspace = Workspaces.FirstOrDefault(w => w.Name == args.OldWorkspace.Name);
			if (oldWorkspace != null)
			{
				oldWorkspace.ActiveOnMonitor = false;
			}
		}

		WorkspaceModel? newWorkspace = Workspaces.FirstOrDefault(w => w.Name == args.NewWorkspace.Name);
		if (newWorkspace != null)
		{
			newWorkspace.ActiveOnMonitor = true;
		}
	}

	private void WorkspaceManager_WorkspaceRenamed(object? sender, WorkspaceRenamedEventArgs e)
	{
		WorkspaceModel? workspace = Workspaces.FirstOrDefault(m => m.Workspace == e.Workspace);
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
				_context.WorkspaceManager.MonitorWorkspaceChanged -= WorkspaceManager_MonitorWorkspaceChanged;
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
