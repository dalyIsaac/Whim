using System;
using System.ComponentModel;

namespace Whim.Bar;

/// <summary>
/// Command for switching workspace.
/// </summary>
public class SwitchWorkspaceCommand : System.Windows.Input.ICommand
{
	private readonly IConfigContext _configContext;
	private readonly WorkspaceWidgetViewModel _viewModel;
	private readonly WorkspaceModel _workspace;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Creates a new instance of <see cref="SwitchWorkspaceCommand"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="viewModel"></param>
	/// <param name="workspace"></param>
	public SwitchWorkspaceCommand(IConfigContext configContext, WorkspaceWidgetViewModel viewModel, WorkspaceModel workspace)
	{
		_configContext = configContext;
		_viewModel = viewModel;
		_workspace = workspace;
		_workspace.PropertyChanged += Workspace_PropertyChanged;
	}

	private void Workspace_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(_workspace.ActiveOnMonitor))
		{
			CanExecuteChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => !_workspace.ActiveOnMonitor;

	/// <inheritdoc/>
	public void Execute(object? parameter)
	{
		Logger.Debug("Executing...");
		if (parameter is WorkspaceModel)
		{
			Logger.Debug($"Activating workspace {_workspace.Workspace} on monitor {_viewModel.Monitor}");
			_configContext.WorkspaceManager.Activate(_workspace.Workspace, _viewModel.Monitor);
		}
	}
}
