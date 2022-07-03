using System.Linq;

namespace Whim.Bar;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BarWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;
	private readonly IMonitor _monitor;
	private readonly IWindowState _windowLocation;

	/// <summary>
	/// Creates a new bar window.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="barConfig"></param>
	/// <param name="monitor"></param>
	/// <exception cref="BarException"></exception>
	public BarWindow(IConfigContext configContext, BarConfig barConfig, IMonitor monitor)
	{
		_configContext = configContext;
		_barConfig = barConfig;
		_monitor = monitor;

		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "BarWindow");

		IWindow? window = IWindow.CreateWindow(this.GetHandle(), _configContext);
		if (window == null)
		{
			throw new BarException("Window was unexpectedly null");
		}

		int leftMargin = (int)_barConfig.Margin.Left;
		int rightMargin = (int)_barConfig.Margin.Right;
		int topMargin = (int)_barConfig.Margin.Top;
		int bottomMargin = (int)_barConfig.Margin.Bottom;

		_windowLocation = new WindowState(window, new Location(
			x: _monitor.X + leftMargin,
			y: _monitor.Y + rightMargin,
			width: _monitor.Width - (leftMargin + rightMargin),
			height: _barConfig.Height), WindowSize.Normal);

		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/3689
		Title = "Whim Bar";
		Win32Helper.HideCaptionButtons(_windowLocation.Window.Handle);
		Win32Helper.SetWindowCorners(_windowLocation.Window.Handle);
		this.SetIsShownInSwitchers(false);

		// Set up the bar.
		LeftPanel.Children.AddRange(_barConfig.LeftComponents.Select(c => c(_configContext, _monitor, this)));
		CenterPanel.Children.AddRange(_barConfig.CenterComponents.Select(c => c(_configContext, _monitor, this)));
		RightPanel.Children.AddRange(_barConfig.RightComponents.Select(c => c(_configContext, _monitor, this)));
	}

	/// <summary>
	/// Renders the bar in the correct location. Use this instead of Show() to ensure
	/// the bar is rendered in the correct location.
	/// </summary>
	public void Render()
	{
		Win32Helper.SetWindowPos(_windowLocation);
	}
}
