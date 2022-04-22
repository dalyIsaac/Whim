using System;

namespace Whim.Bar;

public class NextLayoutEngineCommand : System.Windows.Input.ICommand
{
	private readonly IConfigContext _configContext;
	private readonly ActiveLayoutWidgetViewModel _viewModel;

#pragma warning disable CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used
	public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used

	public NextLayoutEngineCommand(IConfigContext configContext, ActiveLayoutWidgetViewModel viewModel)
	{
		_configContext = configContext;
		_viewModel = viewModel;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter)
	{
		Logger.Debug("Switching to next layout engine");
		_configContext.WorkspaceManager.GetWorkspaceForMonitor(_viewModel.Monitor)?.NextLayoutEngine();
	}
}
