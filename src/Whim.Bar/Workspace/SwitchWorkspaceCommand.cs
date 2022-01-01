using System;
using System.Windows.Input;

namespace Whim.Bar;

public class SwitchWorkspaceCommand : System.Windows.Input.ICommand
{
	private readonly IConfigContext _configContext;
	private readonly WorkspaceWidgetViewModel _viewModel;
	private readonly WorkspaceModel _workspace;

	public SwitchWorkspaceCommand(IConfigContext configContext, WorkspaceWidgetViewModel viewModel, WorkspaceModel workspace)
	{
		_configContext = configContext;
		_viewModel = viewModel;
		_workspace = workspace;
	}

	public event EventHandler? CanExecuteChanged
	{
		add => CommandManager.RequerySuggested += value;
		remove => CommandManager.RequerySuggested -= value;
	}

	public bool CanExecute(object? parameter) => !_workspace.ActiveOnMonitor;

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
