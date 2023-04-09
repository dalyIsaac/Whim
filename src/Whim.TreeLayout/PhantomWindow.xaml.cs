namespace Whim.TreeLayout;

internal sealed partial class PhantomWindow : Microsoft.UI.Xaml.Window
{
	public PhantomWindow(IConfigContext configContext)
	{
		Title = "Whim TreeLayout Phantom Window";
		this.InitializeBorderlessWindow(configContext, "Whim.TreeLayout", "PhantomWindow");
		this.SetIsShownInSwitchers(false);
	}

	private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		this.Close();
	}
}
