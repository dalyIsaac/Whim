using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class RadioButtonRowView : UserControl, IVariantRowView<SelectOption, SelectVariantRowViewModel>
{
	private readonly SelectVariantViewModel _selectVariantViewModel;

	public SelectVariantRowViewModel ViewModel { get; }

	public RadioButtonRowView(SelectVariantViewModel selectVariantViewModel, MatcherResult<SelectOption> item)
	{
		_selectVariantViewModel = selectVariantViewModel;
		ViewModel = new SelectVariantRowViewModel(item);
		UIElementExtensions.InitializeComponent(
			component: this,
			"Whim.CommandPalette",
			"Variants/Select/RadioButtonRowView"
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

	private void RadioButton_Click(object _, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		_selectVariantViewModel.VariantRow_OnClick(this);
	}
}
