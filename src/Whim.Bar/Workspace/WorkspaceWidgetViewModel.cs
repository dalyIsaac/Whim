using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Whim.Bar;

/// <summary>
/// View model containing the workspaces for a given monitor.
/// </summary>
public class WorkspaceWidgetViewModel : INotifyPropertyChanged, IDisposable
{
	private readonly IConfigContext _configContext;
	private bool disposedValue;

	/// <summary>
	/// The monitor which contains the workspaces.
	/// </summary>
	public IMonitor Monitor { get; }

	/// <summary>
	/// The workspaces for the monitor.
	/// </summary>
	public ObservableCollection<WorkspaceModel> Workspaces { get; } = new();

	/// <summary>
	/// Creates a new instance of <see cref="WorkspaceWidgetViewModel"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="monitor"></param>
	public WorkspaceWidgetViewModel(IConfigContext configContext, IMonitor monitor)
	{
		_configContext = configContext;
		Monitor = monitor;

		_configContext.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		_configContext.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		_configContext.WorkspaceManager.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;

		// Populate the list of workspaces
		foreach (IWorkspace workspace in _configContext.WorkspaceManager)
		{
			IMonitor? monitorForWorkspace = _configContext.WorkspaceManager.GetMonitorForWorkspace(workspace);
			Workspaces.Add(new WorkspaceModel(configContext, this, workspace, Monitor == monitorForWorkspace));
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

		IMonitor? monitorForWorkspace = _configContext.WorkspaceManager.GetMonitorForWorkspace(args.Workspace);
		Workspaces.Add(new WorkspaceModel(_configContext, this, args.Workspace, Monitor == monitorForWorkspace));
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

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_configContext.WorkspaceManager.WorkspaceAdded -= WorkspaceManager_WorkspaceAdded;
				_configContext.WorkspaceManager.WorkspaceRemoved -= WorkspaceManager_WorkspaceRemoved;
				_configContext.WorkspaceManager.MonitorWorkspaceChanged -= WorkspaceManager_MonitorWorkspaceChanged;
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			disposedValue = true;
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
