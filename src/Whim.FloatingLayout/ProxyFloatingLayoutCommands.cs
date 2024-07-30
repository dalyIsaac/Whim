using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.FloatingLayout;

/// <summary>
/// The commands for the floating layout plugin.
/// </summary>
public class ProxyFloatingLayoutCommands : PluginCommands
{
	private readonly IProxyFloatingLayoutPlugin _proxyFloatingLayoutPlugin;

	/// <summary>
	/// Creates a new instance of the floating layout commands.
	/// </summary>
	public ProxyFloatingLayoutCommands(IProxyFloatingLayoutPlugin proxyFloatingLayoutPlugin)
		: base(proxyFloatingLayoutPlugin.Name)
	{
		_proxyFloatingLayoutPlugin = proxyFloatingLayoutPlugin;

		Add(
				identifier: "toggle_window_floating",
				title: "Toggle window floating",
				() => _proxyFloatingLayoutPlugin.ToggleWindowFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_F)
			)
			.Add(
				identifier: "mark_window_as_floating",
				title: "Mark window as floating",
				() => _proxyFloatingLayoutPlugin.MarkWindowAsFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_M)
			)
			.Add(
				identifier: "mark_window_as_docked",
				title: "Mark window as docked",
				() => _proxyFloatingLayoutPlugin.MarkWindowAsDocked(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_D)
			);
	}
}
