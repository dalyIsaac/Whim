using System.ComponentModel;

namespace Whim.Bar;

public class WorkspaceModel : INotifyPropertyChanged
{
	public IWorkspace Workspace { get; }
	public string Name => Workspace.Name;

	private bool _activeonMonitor;
	public bool ActiveOnMonitor
	{
		get => _activeonMonitor;
		set
		{
			_activeonMonitor = value;
			OnPropertyChanged(nameof(ActiveOnMonitor));
		}
	}

	public System.Windows.Input.ICommand SwitchWorkspaceCommand { get; }

	public WorkspaceModel(IConfigContext configContext, WorkspaceWidgetViewModel viewModel, IWorkspace workspace, bool activeOnMonitor)
	{
		Workspace = workspace;
		ActiveOnMonitor = activeOnMonitor;
		SwitchWorkspaceCommand = new SwitchWorkspaceCommand(configContext, viewModel, this);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
