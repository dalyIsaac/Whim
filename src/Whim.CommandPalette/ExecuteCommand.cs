using System;

namespace Whim.CommandPalette;

/// <summary>
/// Command to save the changes made in the palette.
/// </summary>
internal class ConfirmCommand : System.Windows.Input.ICommand
{
	private readonly ICommandPaletteWindowViewModel _viewModel;

#pragma warning disable CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used
	/// <inheritdoc/>
	public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067 // The event 'NextLayoutEngineCommand.CanExecuteChanged' is never used

	/// <summary>
	/// Creates a new instance of <see cref="ConfirmCommand"/>.
	/// </summary>
	/// <param name="viewModel"></param>
	public ConfirmCommand(ICommandPaletteWindowViewModel viewModel)
	{
		_viewModel = viewModel;
	}

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
