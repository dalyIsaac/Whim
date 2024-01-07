using System;

namespace Whim.Bar;

/// <summary>
/// Command to switch to the next layout engine.
/// </summary>
internal class NextLayoutEngineCommand : System.Windows.Input.ICommand
{
	private readonly IContext _context;
	private readonly ActiveLayoutWidgetViewModel _viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Creates a new instance of <see cref="NextLayoutEngineCommand"/>.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="viewModel"></param>
	public NextLayoutEngineCommand(IContext context, ActiveLayoutWidgetViewModel viewModel)
	{
		_context = context;
		_viewModel = viewModel;
	}

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => true;

	/// <inheritdoc/>
	public void Execute(object? parameter)
	{
		Logger.Debug("Switching to next layout engine");
		_context.Butler.GetWorkspaceForMonitor(_viewModel.Monitor)?.NextLayoutEngine();
	}
}
