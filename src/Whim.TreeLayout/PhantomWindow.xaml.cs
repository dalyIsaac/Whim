namespace Whim.TreeLayout;

public sealed partial class PhantomWindow : PluginWindow
{
	public PhantomWindow()
	{
		Title = "Whim TreeLayout Phantom Window";
		ExtendsContentIntoTitleBar = true;
		InitializeComponent("Whim.TreeLayout", "PhantomWindow");
	}
}
