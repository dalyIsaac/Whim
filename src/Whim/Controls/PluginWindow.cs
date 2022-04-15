using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Whim;

[ContentProperty(Name = "WindowContent")]
public class PluginWindow : Microsoft.UI.Xaml.Window
{
	private bool _contentLoaded;
	private readonly ContentControl _windowContent = new()
	{
		HorizontalContentAlignment = HorizontalAlignment.Stretch,
		VerticalContentAlignment = VerticalAlignment.Stretch
	};

	public PluginWindow()
	{
		Content = _windowContent;
	}

	/// <summary>
	/// Initializes the window with the specified namespace and path.
	/// This is necessary, as the build process will copy the plugins from
	/// the relevant project to the <c>/plugins</c> folder in <c>Whim.App</c>.
	/// As a result, the the <c>Uri</c> of <see cref="Application.LoadComponent"/>
	/// will be relative to the originating project, not the <c>Whim.App</c> project.
	/// </summary>
	/// <param name="windowNamespace">The namespace of the window.</param>
	/// <param name="windowPath">
	/// The path to the XAML window. Do not include the <c>.xaml</c> extension.
	/// </param>
	public void InitializeComponent(string windowNamespace, string windowPath)
	{
		if (_contentLoaded)
		{
			return;
		}

		_contentLoaded = true;

		System.Uri resourceLocator = new($"ms-appx:///plugins/{windowNamespace}/{windowPath}.xaml");
		Application.LoadComponent(this, resourceLocator, Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Nested);
	}

	public new string Title
	{
		get => base.Title;
		set => base.Title = value;
	}

	public object? WindowContent
	{
		get { return _windowContent.Content; }
		set { _windowContent.Content = value; }
	}
}
