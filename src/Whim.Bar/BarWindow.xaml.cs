using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim.Bar;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BarWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly BarConfig _barConfig;
	private readonly IMonitor _monitor;
	private readonly IWindowLocation _windowLocation;

	public BarWindow(IConfigContext configContext, BarConfig barConfig, IMonitor monitor)
	{
		_configContext = configContext;
		_barConfig = barConfig;
		_monitor = monitor;

		InitializeComponent();

		IWindow? window = Window.CreateWindow(this.GetHandle(), _configContext);
		if (window == null)
		{
			throw new Exception("Window was unexpectedly null");
		}

		_windowLocation = new WindowLocation(window, new Location(
			x: _monitor.X + _barConfig.Margin,
			y: _monitor.Y + _barConfig.Margin,
			width: _monitor.Width - (_barConfig.Margin * 2),
			height: _barConfig.Height), WindowState.Normal);

		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/3689
		Title = "Whim Bar";
		SetWindowStyle();
		Win32Helper.SetWindowCorners(_windowLocation.Window.Handle);

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

	/// <summary>
	///
	/// </summary>
	/// <returns></returns>
	private void SetWindowStyle()
	{
		int style = PInvoke.GetWindowLong(_windowLocation.Window.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);

		// Hide the title bar and caption buttons
		style &= ~(int)WINDOW_STYLE.WS_CAPTION & ~(int)WINDOW_STYLE.WS_THICKFRAME;

		PInvoke.SetWindowLong(_windowLocation.Window.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
	}
}
