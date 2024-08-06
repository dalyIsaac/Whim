using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class FreeTextVariantViewModel(ICommandPaletteWindowViewModel windowViewModel) : IVariantViewModel
{
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel = windowViewModel;
	private FreeTextVariantConfig? _activationConfig;

	public string Prompt => _activationConfig?.Prompt ?? "";

	public string? ConfirmButtonText => _activationConfig?.ConfirmButtonText;

	public event PropertyChangedEventHandler? PropertyChanged;

	public void Activate(BaseVariantConfig activationConfig)
	{
		if (activationConfig is FreeTextVariantConfig config)
		{
			_activationConfig = config;
			OnPropertyChanged(nameof(Prompt));
		}
		else
		{
			_activationConfig = null;
		}
	}

	public void OnKeyDown(VirtualKey key)
	{
		if (key != VirtualKey.Enter)
		{
			return;
		}

		Execute();
	}

	public void Update() { }

	public void Confirm() => Execute();

	private void Execute()
	{
		if (_activationConfig == null)
		{
			return;
		}

		_activationConfig.Callback(_commandPaletteWindowViewModel.Text);
		_commandPaletteWindowViewModel.RequestHide();
	}

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
