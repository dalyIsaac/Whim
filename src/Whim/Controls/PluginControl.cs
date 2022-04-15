using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace Whim;

[ContentProperty(Name = "UserControlContent")]
public class PluginControl : UserControl
{
	private bool _contentLoaded;
	private readonly ContentControl _userControlContent = new()
	{
		HorizontalContentAlignment = HorizontalAlignment.Stretch,
		VerticalContentAlignment = VerticalAlignment.Stretch
	};

	public PluginControl()
	{
		Content = _userControlContent;
	}

	/// <summary>
	/// Initializes the control with the specified namespace and path.
	/// This is necessary, as the build process will copy the plugins from
	/// the relevant project to the <c>/plugins</c> folder in <c>Whim.App</c>.
	/// As a result, the the <c>Uri</c> of <see cref="Application.LoadComponent"/>
	/// will be relative to the originating project, not the <c>Whim.App</c> project.
	/// </summary>
	/// <param name="controlNamespace">The namespace of the control.</param>
	/// <param name="control">
	/// The path to the XAML control. Do not include the <c>.xaml</c> extension.
	/// </param>
	public void InitializeComponent(string controlNamespace, string controlPath)
	{
		if (_contentLoaded)
		{
			return;
		}

		_contentLoaded = true;

		System.Uri resourceLocator = new($"ms-appx:///plugins/{controlNamespace}/{controlPath}.xaml");
		Application.LoadComponent(this, resourceLocator, Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Nested);
	}

	public object? UserControlContent
	{
		get { return _userControlContent.Content; }
		set { _userControlContent.Content = value; }
	}
}
