using System;

namespace Whim.CommandPalette;

/// <summary>
/// Command to save the changes made in the palette.
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="ConfirmCommand"/>.
/// </remarks>
/// <param name="viewModel"></param>
internal class ConfirmCommand(ICommandPaletteWindowViewModel viewModel) : System.Windows.Input.ICommand
{
	private readonly ICommandPaletteWindowViewModel _viewModel = viewModel;

	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;

	public bool CanExecute(object? parameter) => _viewModel.ActiveVariant != null;

	public void Execute(object? parameter)
	{
		if (_viewModel.ActiveVariant == null)
		{
			return;
		}

		BaseVariantConfig config = _viewModel.ActivationConfig;
		_viewModel.ActiveVariant.ViewModel.Confirm();

		// Only hide the palette if the active config is the same as the one that was used to activate it.
		if (_viewModel.IsConfigActive(config))
		{
			_viewModel.RequestHide();
		}
	}
}
