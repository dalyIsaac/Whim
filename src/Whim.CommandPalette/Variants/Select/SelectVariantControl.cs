using Microsoft.UI.Xaml;
using System;

namespace Whim.CommandPalette;

internal class SelectVariantControl : IVariantControl
{
	// private readonly SelectVariantView _control;
#pragma warning disable CS8603 // Possible null reference return.
	public UIElement Control => null;
#pragma warning restore CS8603 // Possible null reference return.

	public IVariantViewModel ViewModel { get; }

	public SelectVariantControl(CommandPaletteWindowViewModel windowViewModel)
	{
		SelectVariantViewModel viewModel = new(windowViewModel);
		ViewModel = viewModel;
		// _control = new SelectVariantView(viewModel);
	}

	public double GetViewMaxHeight() => throw new NotImplementedException();
}
