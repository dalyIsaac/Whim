namespace Whim.TreeLayout;

internal sealed partial class PhantomWindow : Microsoft.UI.Xaml.Window
{
	public PhantomWindow(IConfigContext configContext)
	{
		Title = "Whim TreeLayout Phantom Window";
		this.InitializeBorderlessWindow(configContext, "Whim.TreeLayout", "PhantomWindow");
		this.SetIsShownInSwitchers(false);
	}
}
