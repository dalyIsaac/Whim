namespace Whim.TreeLayout;

internal sealed partial class PhantomWindow : Microsoft.UI.Xaml.Window
{
	public PhantomWindow()
	{
		Title = "Whim TreeLayout Phantom Window";
		ExtendsContentIntoTitleBar = true;
		UIElementExtensions.InitializeComponent(this, "Whim.TreeLayout", "PhantomWindow");
	}
}
