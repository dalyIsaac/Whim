using Microsoft.UI.Xaml.Controls;

namespace Whim.CommandPalette;

/// <summary>
/// A palette row is a single command title, and an optional associated keybind.
/// </summary>
internal sealed partial class RadioButtonRowControl : UserControl, IVariantRowControl<SelectOption>
{
	private readonly SelectVariantViewModel _selectVariantViewModel;

	public IVariantRowViewModel<SelectOption> ViewModel { get; }

	public RadioButtonRowControl(SelectVariantViewModel selectVariantViewModel, MatcherResult<SelectOption> item)
	{
		_selectVariantViewModel = selectVariantViewModel;
		ViewModel = new VariantRowViewModel<SelectOption>(item);
		UIElementExtensions.InitializeComponent(
			component: this,
			"Whim.CommandPalette",
			"Variants/Select/RadioButtonRowControl"
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
