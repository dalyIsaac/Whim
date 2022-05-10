using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly CommandPaletteConfig _commandPaletteConfig;
	private readonly IWindow _window;
	public bool IsVisible { get; private set; }

	public CommandPaletteWindow(IConfigContext configContext, CommandPaletteConfig commandPaletteConfig)
	{
		_configContext = configContext;
		_commandPaletteConfig = commandPaletteConfig;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
	}

	public void Activate(IMonitor monitor)
	{
		int width = 800;
		int height = 800;

		ILocation<int> windowLocation = new Location(
			x: monitor.X + (monitor.Width / 2) - (width / 2),
			y: monitor.Y + (height / 4),
			width: width,
			height: height
		);

		Activate();
		Win32Helper.SetWindowPos(
			new WindowLocation(_window, windowLocation, WindowState.Normal),
			_window.Handle
		);
		IsVisible = true;
	}

	public void Hide()
	{
		_window.Hide();
		IsVisible = false;
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			Hide();
		}
		else
		{
			Activate(_configContext.MonitorManager.FocusedMonitor);
		}
	}
}
