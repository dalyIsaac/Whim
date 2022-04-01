namespace Whim.TreeLayout;

public sealed partial class PhantomWindow : Microsoft.UI.Xaml.Window
{
	public PhantomWindow()
	{
		this.InitializeComponent();
		Title = "Whim TreeLayout Phantom Window";
		ExtendsContentIntoTitleBar = true;
	}
}
