using System.Collections.Generic;

namespace Whim.CommandPalette;

public sealed partial class CommandPaletteWindow : Microsoft.UI.Xaml.Window
{
	private readonly IConfigContext _configContext;
	private readonly IWindow _window;
	private IMonitor? _monitor;
	public bool IsVisible => _monitor != null;
	public CommandPaletteViewModel ViewModel { get; }

	public CommandPaletteWindow(IConfigContext configContext, CommandPalettePlugin plugin)
	{
		_configContext = configContext;
		_window = this.InitializeBorderlessWindow("Whim.CommandPalette", "CommandPaletteWindow", _configContext);

		Title = CommandPaletteConfig.Title;
		ViewModel = new CommandPaletteViewModel(configContext, plugin);
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null, IMonitor? monitor = null)
	{
		ViewModel.Activate(items);
		monitor ??= _configContext.MonitorManager.FocusedMonitor;

		if (monitor != _monitor)
		{
			return;
		}

		_monitor = monitor;

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
	}

	public void Hide()
	{
		_window.Hide();
		_monitor = null;
	}

	public void Toggle()
	{
		if (IsVisible)
		{
			Hide();
		}
		else
		{
			Activate();
		}
	}
}
