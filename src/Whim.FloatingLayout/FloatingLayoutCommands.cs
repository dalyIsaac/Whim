using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.FloatingLayout;

/// <summary>
/// The commands for the floating layout plugin.
/// </summary>
public class FloatingLayoutCommands : PluginCommands
{
	private readonly IFloatingLayoutPlugin _floatingLayoutPlugin;

	/// <summary>
	/// Creates a new instance of the floating layout commands.
	/// </summary>
	public FloatingLayoutCommands(IFloatingLayoutPlugin floatingLayoutPlugin)
		: base(floatingLayoutPlugin.Name)
	{
		_floatingLayoutPlugin = floatingLayoutPlugin;

		Add(
				identifier: "toggle_window_floating",
				title: "Toggle window floating",
				() => _floatingLayoutPlugin.ToggleWindowFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_F)
			)
			.Add(
				identifier: "mark_window_as_floating",
				title: "Mark window as floating",
				() => _floatingLayoutPlugin.MarkWindowAsFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_M)
			)
			.Add(
				identifier: "mark_window_as_docked",
				title: "Mark window as docked",
				() => _floatingLayoutPlugin.MarkWindowAsDocked(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_D)
			);
	}
}
