using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class SelectVariantRowView
	: UserControl,
		IVariantRowView<SelectOption, SelectVariantRowViewModel>
{
	public SelectVariantRowViewModel ViewModel { get; }

	public SelectVariantRowView(MatcherResult<SelectOption> item)
	{
		ViewModel = new SelectVariantRowViewModel(item);
		UIElementExtensions.InitializeComponent(
			component: this,
			"Whim.CommandPalette",
			"Variants/Select/SelectVariantRowView"
		);
	}

	public void Initialize()
	{
		this.SetTitle(OptionTitle.Inlines);
	}

	public void Update(MatcherResult<SelectOption> matcherResult)
	{
		Logger.Debug("Updating with a new item");
		ViewModel.Update(matcherResult);
		this.SetTitle(OptionTitle.Inlines);
	}
}
