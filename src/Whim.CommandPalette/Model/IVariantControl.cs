using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

public interface IVariantControl<T> where T : BaseVariantConfig
{
	UIElement Control { get; }

	IVariantViewModel<T> ViewModel { get; }

	double GetViewHeight();
}
