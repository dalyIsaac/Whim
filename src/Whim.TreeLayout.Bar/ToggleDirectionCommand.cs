using System;

namespace Whim.TreeLayout.Bar;

/// <summary>
/// Command for toggling the direction to add new windows in.
/// </summary>
public class ToggleDirectionCommand : System.Windows.Input.ICommand
{
	private readonly TreeLayoutEngineWidgetViewModel _viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

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
