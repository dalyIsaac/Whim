using System.ComponentModel;
using Windows.System;

namespace Whim.CommandPalette;

internal class SelectVariantViewModel : IVariantViewModel
{
	private readonly ICommandPaletteWindowViewModel _commandPaletteWindowViewModel;
	private SelectVariantConfig? _activationConfig;

	public bool ShowSaveButton => true;

	public event PropertyChangedEventHandler? PropertyChanged;

	public SelectVariantViewModel(ICommandPaletteWindowViewModel windowViewModel)
	{
		_commandPaletteWindowViewModel = windowViewModel;
	}

	public void Activate(BaseVariantConfig activationConfig)
	{
		if (activationConfig is SelectVariantConfig config)
		{
			_activationConfig = config;
		}
		else
		{
			_activationConfig = null;
		}
	}

	public void Hide() { }

	public void OnKeyDown(VirtualKey key)
	{
		if (_activationConfig == null)
		{
			return;
		}

		_commandPaletteWindowViewModel.RequestHide();
		throw new System.NotImplementedException();
	}

	public void Update() { }

	protected virtual void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
