using System.Linq;

namespace Whim.Bar;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BarWindow : Microsoft.UI.Xaml.Window
{
	private readonly IContext _context;
	private readonly BarConfig _barConfig;
	private readonly IMonitor _monitor;

	/// <summary>
	/// The current window state.
	/// </summary>
	public IWindowState WindowState { get; }

	/// <summary>
	/// Creates a new bar window.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="barConfig"></param>
	/// <param name="monitor"></param>
	/// <exception cref="BarException"></exception>
	public BarWindow(IContext context, BarConfig barConfig, IMonitor monitor)
	{
		_context = context;
		_barConfig = barConfig;
		_monitor = monitor;

		UIElementExtensions.InitializeComponent(this, "Whim.Bar", "BarWindow");

		IWindow window =
			_context.WindowManager.CreateWindow(this.GetHandle())
			?? throw new BarException("Window was unexpectedly null");

		double scaleFactor = _monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;

		WindowState = new WindowState()
		{
			Window = window,
			Location = new Location<int>()
			{
				X = _monitor.WorkingArea.X,
				Y = _monitor.WorkingArea.Y,
				Width = (int)(_monitor.WorkingArea.Width / scale),
				Height = _barConfig.Height
			},
			WindowSize = WindowSize.Normal
		};

		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/3689
		Title = "Whim Bar";
		_context.NativeManager.HideCaptionButtons(WindowState.Window.Handle);
		this.SetIsShownInSwitchers(false);

		// Set up the bar.
		LeftPanel.Children.AddRange(_barConfig.LeftComponents.Select(c => c(_context, _monitor, this)));
		CenterPanel.Children.AddRange(_barConfig.CenterComponents.Select(c => c(_context, _monitor, this)));
		RightPanel.Children.AddRange(_barConfig.RightComponents.Select(c => c(_context, _monitor, this)));
	}

	internal void UpdateLocation()
	{
		double scaleFactor = _monitor.ScaleFactor;
		double scale = scaleFactor / 100.0;

		WindowState.Location = new Location<int>()
		{
			X = _monitor.WorkingArea.X,
			Y = _monitor.WorkingArea.Y,
			Width = (int)(_monitor.WorkingArea.Width / scale),
			Height = _barConfig.Height
		};
	}
}
