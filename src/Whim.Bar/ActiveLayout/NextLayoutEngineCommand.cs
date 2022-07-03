using System;

namespace Whim.Bar;

/// <summary>
/// Command to switch to the next layout engine.
/// </summary>
public class NextLayoutEngineCommand : System.Windows.Input.ICommand
{
	private readonly IConfigContext _configContext;
	private readonly ActiveLayoutWidgetViewModel _viewModel;

#pragma warning disable CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used
	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used

	/// <summary>
	/// Creates a new instance of <see cref="NextLayoutEngineCommand"/>.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="viewModel"></param>
	public NextLayoutEngineCommand(IConfigContext configContext, ActiveLayoutWidgetViewModel viewModel)
	{
		_configContext = configContext;
		_viewModel = viewModel;
	}

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => true;

	/// <inheritdoc/>
	public void Execute(object? parameter)
	{
		Logger.Debug("Switching to next layout engine");
		_configContext.WorkspaceManager.GetWorkspaceForMonitor(_viewModel.Monitor)?.NextLayoutEngine();
	}
}
