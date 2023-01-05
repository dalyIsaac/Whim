using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class FreeTextVariantViewModel : IVariantViewModel<FreeTextVariantConfig>
{
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel;
	private FreeTextVariantConfig? _activationConfig;

	public string _prompt = "";
	public string Prompt
	{
		get => _prompt;
		set
		{
			if (Prompt != value)
			{
				_prompt = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Prompt)));
			}
		}
	}

	public bool ShowSaveButton => true;

	public event PropertyChangedEventHandler? PropertyChanged;

	public FreeTextVariantViewModel(ICommandPaletteWindowViewModel windowViewModel)
	{
		_commandPaletteWindowViewModel = windowViewModel;
	}

	public void Activate(FreeTextVariantConfig activationConfig)
	{
		_activationConfig = activationConfig;
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
}
