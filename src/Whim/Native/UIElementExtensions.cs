using Microsoft.UI.Xaml;

namespace Whim;

/// <summary>
/// Extension methods for <see cref="UIElement"/>.
/// </summary>
public static class UIElementExtensions
{
	/// <summary>
	/// Initializes the <paramref name="component"/> with the specified namespace and path.
	/// This is necessary, as the build process will copy the plugins from
	/// the relevant project to the <c>/plugins</c> folder in <c>Whim.Runner</c>.
	/// As a result, the the <c>Uri</c> of <see cref="Application.LoadComponent(object, System.Uri, Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation)"/>
	/// will be relative to the originating project, not the <c>Whim.Runner</c> project.
	/// </summary>
	/// <param name="component">The component to initialize.</param>
	/// <param name="componentNamespace">The namespace of the component.</param>
	/// <param name="componentPath">
	/// The path to the XAML component. Do not include the <c>.xaml</c> extension.
	/// </param>
	public static void InitializeComponent(object component, string componentNamespace, string componentPath)
	{
		System.Uri resourceLocator = new($"ms-appx:///plugins/{componentNamespace}/{componentPath}.xaml");
		Application.LoadComponent(
			component,
			resourceLocator,
			Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Nested
		);
	}
}
