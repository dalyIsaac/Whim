using Microsoft.UI.Xaml;

namespace Whim.CommandPalette;

/// <summary>
/// Wrapper for a palette variant control and view model which can be rendered in the palette window.
/// </summary>
/// <typeparam name="T">Activation config</typeparam>
public interface IVariantControl<T> where T : BaseVariantConfig
{
	/// <summary>
	/// The control to render.
	/// </summary>
	UIElement Control { get; }

	/// <summary>
	/// The view model for the control.
	/// </summary>
	IVariantViewModel<T> ViewModel { get; }

	/// <summary>
	/// Gets the height of <see cref="Control"/> in pixels. This is used for manually setting the
	/// command palette window height.
	/// </summary>
	/// <returns>The height of <see cref="Control"/> in pixels.</returns>
	double GetViewHeight();
}
