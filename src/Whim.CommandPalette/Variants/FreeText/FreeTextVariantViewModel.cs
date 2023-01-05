using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class FreeTextVariantViewModel : IVariantViewModel
{
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel;
	private FreeTextVariantConfig? _activationConfig;

	public string Prompt => _activationConfig?.Prompt ?? "";

	public bool ShowSaveButton => true;

	public event PropertyChangedEventHandler? PropertyChanged;

	public FreeTextVariantViewModel(ICommandPaletteWindowViewModel windowViewModel)
	{
		_commandPaletteWindowViewModel = windowViewModel;
	}

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

	public void Hide() { }

	public void OnKeyDown(VirtualKey key)
	{
		if (_activationConfig == null || key != VirtualKey.Enter)
		{
			return;
		}

		_activationConfig.Callback(_commandPaletteWindowViewModel.Text);
		_commandPaletteWindowViewModel.RequestHide();
	}

	public void Update() { }

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
