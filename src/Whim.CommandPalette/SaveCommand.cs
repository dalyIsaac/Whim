using System;

namespace Whim.CommandPalette;

/// <summary>
/// Command to save the changes made in the palette.
/// </summary>
internal class SaveCommand : System.Windows.Input.ICommand
{
	private readonly CommandPaletteWindowViewModel _viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	/// <summary>
	/// Creates a new instance of <see cref="SaveCommand"/>.
	/// </summary>
	/// <param name="viewModel"></param>
	public SaveCommand(CommandPaletteWindowViewModel viewModel)
	{
		_viewModel = viewModel;
	}

	public bool CanExecute(object? parameter) => _viewModel.ActiveVariant != null;

	public void Execute(object? parameter) => _viewModel.ActiveVariant?.ViewModel.Save();
}
