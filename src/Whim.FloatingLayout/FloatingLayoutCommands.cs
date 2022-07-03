using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.FloatingLayout;

/// <summary>
/// Default commands for the floating layout plugin.
/// </summary>
public static class FloatingLayoutCommands
{
	/// <summary>
	/// Gets the default floating layout plugin commands.
	/// </summary>
	public static (ICommand, IKeybind?)[] GetCommands(FloatingLayoutPlugin floatingLayoutPlugin) => new (ICommand, IKeybind?)[]
	{
		(
			new Command(
				identifier: "floating_layout.toggle_window_floating",
				title: "Toggle window floating",
				callback: () => floatingLayoutPlugin.ToggleWindowFloating()
			),
			new Keybind(DefaultCommands.WinShift, VIRTUAL_KEY.VK_F)
		)
	};
}
