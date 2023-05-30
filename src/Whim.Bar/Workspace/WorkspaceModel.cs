using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// Workspace model for the bar.
/// </summary>
internal class WorkspaceModel : INotifyPropertyChanged
{
	/// <summary>
	/// The workspace.
	/// </summary>
	public IWorkspace Workspace { get; }

	/// <summary>
	/// The The name of the workspace.
	/// </summary>
	public string Name => Workspace.Name;

	private bool _activeonMonitor;

	/// <summary>
	/// Whether the workspace is active on the monitor.
	/// </summary>
	public bool ActiveOnMonitor
	{
		get => _activeonMonitor;
		set
		{
			_activeonMonitor = value;
			OnPropertyChanged(nameof(ActiveOnMonitor));
		}
	}

	/// <summary>
	/// Command to switch to the workspace.
	/// </summary>
	public System.Windows.Input.ICommand SwitchWorkspaceCommand { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceModel"/> class.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="viewModel"></param>
	/// <param name="workspace"></param>
	/// <param name="activeOnMonitor"></param>
	public WorkspaceModel(
		IContext context,
		WorkspaceWidgetViewModel viewModel,
		IWorkspace workspace,
		bool activeOnMonitor
	)
	{
		Workspace = workspace;
		ActiveOnMonitor = activeOnMonitor;
		SwitchWorkspaceCommand = new SwitchWorkspaceCommand(context, viewModel, this);
	}

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <inheritdoc/>
	protected virtual void OnPropertyChanged(string? propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Triggered when the workspace is renamed.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	internal void Workspace_Renamed(object? sender, WorkspaceRenamedEventArgs e)
	{
		OnPropertyChanged(nameof(Name));
	}
}
