namespace Whim.TreeLayout;

internal sealed partial class PhantomWindow : Microsoft.UI.Xaml.Window
{
	public PhantomWindow(IContext context)
	{
		Title = "Whim TreeLayout Phantom Window";
		this.InitializeBorderlessWindow(context, "Whim.TreeLayout", "PhantomWindow");
		this.SetIsShownInSwitchers(false);
		this.SetSystemBackdrop();
	}

	private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		this.Close();
	}
}
