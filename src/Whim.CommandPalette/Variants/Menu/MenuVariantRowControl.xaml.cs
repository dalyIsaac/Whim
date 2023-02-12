using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class MenuVariantRowControl
	: UserControl,
		IVariantRowControl<CommandItem, MenuVariantRowViewModel>
{
	public static double MenuRowHeight => 24;

	public MenuVariantRowViewModel ViewModel { get; }

	public MenuVariantRowControl(MatcherResult<CommandItem> matcherResult)
	{
		ViewModel = new MenuVariantRowViewModel(matcherResult);
		UIElementExtensions.InitializeComponent(this, "Whim.CommandPalette", "Variants/Menu/MenuVariantRowControl");
	}

	public void Initialize()
	{
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	public void Update(MatcherResult<CommandItem> matcherResult)
	{
		Logger.Debug("Updating with a new item");
		ViewModel.Update(matcherResult);
		this.SetTitle(CommandTitle.Inlines);
		SetKeybinds();
	}

	private void SetKeybinds()
	{
		Logger.Debug("Setting keybinds");

		if (ViewModel.Model.Data.Keybind is not null)
		{
			CommandKeybind.Text = ViewModel.Model.Data.Keybind.ToString();
			CommandKeybindBorder.Visibility = Visibility.Visible;
		}
		else
		{
			CommandKeybindBorder.Visibility = Visibility.Collapsed;
		}
	}
}
