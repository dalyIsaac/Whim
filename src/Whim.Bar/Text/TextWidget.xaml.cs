using Microsoft.UI.Xaml.Controls;

namespace Whim.Bar;

/// <summary>
/// Interaction logic for TextWidget.xaml
/// </summary>
public partial class TextWidget : UserControl
{
	/// <summary>
	/// The text view model. The text can be updated through this property.
	/// </summary>
	public TextWidgetViewModel ViewModel { get; private set; }

	/// <summary>
	/// Create the text widget with the given <paramref name="value"/>.
	/// </summary>
	/// <param name="value"></param>
	internal TextWidget(string? value = null)
	{
		ViewModel = new TextWidgetViewModel(value);
		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "Text/TextWidget");
	}

	/// <summary>
	/// Create the text widget bar component.
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static BarComponent CreateComponent(string? value = null)
	{
		return new BarComponent((context, monitor, window) => new TextWidget(value));
	}
}
