using Windows.Win32.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Whim.FocusIndicator;
/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class FocusIndicatorWindow : Microsoft.UI.Xaml.Window
{
	public readonly FocusIndicatorConfig FocusIndicatorConfig;

	public FocusIndicatorWindow(FocusIndicatorConfig focusIndicatorConfig)
	{
		FocusIndicatorConfig = focusIndicatorConfig;
		InitializeComponent();

		HWND hwnd = new(WinRT.Interop.WindowNative.GetWindowHandle(this));
		Win32Helper.HideCaptionButtons(hwnd);
		Win32Helper.SetWindowCorners(hwnd);
	}

	/// <summary>
	/// Activates the window at the given coordinates.
	/// </summary>
	/// <param name="windowLocation">The location of the window.</param>
	public void Activate(IWindowLocation windowLocation)
	{
		Activate();
		Win32Helper.SetWindowPos(windowLocation);
	}
}
