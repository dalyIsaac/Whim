using System;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// Command for toggling <see cref="ITreeLayoutEngine.AddNodeDirection"/>.
/// </summary>
public class ToggleDirectionCommand : System.Windows.Input.ICommand
{
	private readonly TreeLayoutEngineWidgetViewModel _viewModel;

#pragma warning disable CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used
	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used

	/// <summary>
	/// Creates a new instance of <see cref="ToggleDirectionCommand"/>.
	/// </summary>
	/// <param name="viewModel"></param>
	public ToggleDirectionCommand(TreeLayoutEngineWidgetViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => true;

	/// <inheritdoc/>
	public void Execute(object? paramter)
	{
		Logger.Debug("Executing...");
		_viewModel.ToggleDirection();
	}
}
