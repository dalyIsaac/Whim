using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Whim.Core;

namespace Whim.Dashboard.Controls.ViewModel;

/// <summary>
/// View model used for for <see cref="WorkspaceDashboard"/>'s <c>DataContext</c>. This wraps the
/// <see cref="Workspaces"/> <see cref="ObservableCollection{T}"/> and <see cref="Count"/> properties,
/// exposing them for data binding.
/// </summary>
internal class WorkspaceDashboardViewModel : INotifyPropertyChanged
{
	public ObservableCollection<Model.Workspace> Workspaces { get; } = new();
	public int Count { get => Workspaces.Count; }

	public WorkspaceDashboardViewModel(IConfigContext configContext)
	{
		configContext.WorkspaceManager.WorkspaceAdded += WorkspaceManager_WorkspaceAdded;
		configContext.WorkspaceManager.WorkspaceRemoved += WorkspaceManager_WorkspaceRemoved;
		configContext.WorkspaceManager.WorkspaceMonitorChanged += WorkspaceManager_WorkspaceMonitorChanged;

		// Add the workspaces the WorkspaceManager knows about.
		foreach (IWorkspace workspace in configContext.WorkspaceManager)
		{
			IMonitor? monitor = configContext.WorkspaceManager.GetMonitorForWorkspace(workspace);
			Model.Workspace modelWorkspace = new(workspace, monitor);
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
		Model.Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);
		if (model != null)
		{
			return;
		}

		model = new(args.Workspace);
		Workspaces.Add(model);
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void WorkspaceManager_WorkspaceRemoved(object? sender, WorkspaceEventArgs args)
	{
		Model.Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.Workspace.Name);

		if (model == null) { return; }

		Workspaces.Remove(model);
		model.Dispose();
		OnPropertyChanged(nameof(Count)); // Count is a derived property.
	}

	private void WorkspaceManager_WorkspaceMonitorChanged(object? sender, WorkspaceMonitorChangedEventArgs args)
	{
		// Update the old workspace
		if (args.OldWorkspace != null)
		{
			Model.Workspace? model = Workspaces.FirstOrDefault(w => w.Name == args.OldWorkspace.Name);
			if (model != null)
			{
				model.Monitor = null;
			}
		}

		// Update the new workspace
		Model.Workspace? newWorkspace = Workspaces.FirstOrDefault(w => w.Name == args.NewWorkspace.Name);
		if (newWorkspace != null)
		{
			newWorkspace.Monitor = args.Monitor;
		}
	}
}
