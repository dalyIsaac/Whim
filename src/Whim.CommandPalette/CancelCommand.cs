using System;

namespace Whim.CommandPalette;

/// <summary>
/// Command to cancel the changes made in the palette.
/// </summary>
internal class CancelCommand : System.Windows.Input.ICommand
{
	private readonly CommandPaletteWindowViewModel _viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Creates a new instance of <see cref="CancelCommand"/>.
	/// </summary>
	/// <param name="viewModel"></param>
	public CancelCommand(CommandPaletteWindowViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public bool CanExecute(object? parameter) => true;

	public void Execute(object? parameter) => _viewModel.RequestHide();
}
