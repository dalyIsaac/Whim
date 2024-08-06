using System;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// Command for toggling the direction to add new windows in.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="ToggleDirectionCommand"/>.
/// </remarks>
/// <param name="viewModel"></param>
public class ToggleDirectionCommand(TreeLayoutEngineWidgetViewModel viewModel) : System.Windows.Input.ICommand
{
	private readonly TreeLayoutEngineWidgetViewModel _viewModel = viewModel;

	/// <inheritdoc/>
#pragma warning disable CS0067 // The event 'ToggleDirectionCommand.CanExecuteChanged' is never used
	public event EventHandler? CanExecuteChanged;

#pragma warning restore CS0067 // The event 'ToggleDirectionCommand.CanExecuteChanged' is never used

	/// <inheritdoc/>
	public bool CanExecute(object? parameter) => true;

	/// <inheritdoc/>
	public void Execute(object? paramter)
	{
		Logger.Debug("Executing...");
		_viewModel.ToggleDirection();
	}
}
