using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Whim.Dashboard.Workspaces;

/// <summary>
/// View model used for for <see cref="WorkspaceDashboard"/>'s <c>DataContext</c>. This wraps the
/// <see cref="Workspaces"/> <see cref="ObservableCollection{T}"/> and <see cref="Count"/> properties,
/// exposing them for data binding.
/// </summary>
internal class WorkspaceDashboardViewModel : INotifyPropertyChanged, IDisposable
{
	private bool disposedValue;
	private readonly IConfigContext _configContext;
	public ObservableCollection<Workspace> Workspaces { get; } = new();
	public int Count { get => Workspaces.Count; }

	public WorkspaceDashboardViewModel(IConfigContext configContext)
	{
		_configContext = configContext;
		_configContext.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		_configContext.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		_configContext.WorkspaceManager.MonitorWorkspaceChanged += WorkspaceManager_MonitorWorkspaceChanged;
		_configContext.WorkspaceManager.ActiveLayoutEngineChanged += WorkspaceManager_ActiveLayoutEngineChanged;

		// Add the workspaces the WorkspaceManager knows about.
		foreach (IWorkspace workspace in configContext.WorkspaceManager)
		{
			IMonitor? monitor = configContext.WorkspaceManager.GetMonitorForWorkspace(workspace);
			Workspace modelWorkspace = new(workspace, monitor);
			Workspaces.Add(modelWorkspace);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void WorkspaceManager_WorkspaceAdded(object? sender, WorkspaceEventArgs args)
	{
		Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);
		if (model != null)
		{
			// Ignore the model if it already exists.
			return;
		}

		model = new(args.Workspace);
		Workspaces.Add(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void WorkspaceManager_WorkspaceRemoved(object? sender, WorkspaceEventArgs args)
	{
		Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);

		if (model == null) { return; }

		Workspaces.Remove(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void WorkspaceManager_MonitorWorkspaceChanged(object? sender, MonitorWorkspaceChangedEventArgs args)
	{
		// Update the old workspace
		if (args.OldWorkspace != null)
		{
			Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.OldWorkspace.Name);
			if (model != null)
			{
				model.Monitor = null;
			}
		}

		// Update the new workspace
		Workspace? newWorkspace = Workspaces.FirstOrDefault(w => w.Name == args.NewWorkspace.Name);
		if (newWorkspace != null)
		{
			newWorkspace.Monitor = args.Monitor;
		}
	}

	private void WorkspaceManager_ActiveLayoutEngineChanged(object? sender, ActiveLayoutEngineChangedEventArgs args)
	{
		Workspace? workspace = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);
		if (workspace == null)
		{
			return;
		}

		workspace.Workspace_ActiveLayoutEngineChanged();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_configContext.WorkspaceManager.WorkspaceAdded -= WorkspaceManager_WorkspaceAdded;
				_configContext.WorkspaceManager.WorkspaceRemoved -= WorkspaceManager_WorkspaceRemoved;
				_configContext.WorkspaceManager.MonitorWorkspaceChanged -= WorkspaceManager_MonitorWorkspaceChanged;
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
