using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.FloatingWindow;

/// <summary>
/// The commands for the floating window plugin.
/// </summary>
public class FloatingWindowCommands : PluginCommands
{
	private readonly IFloatingWindowPlugin _floatingWindowPlugin;

	/// <summary>
	/// Creates a new instance of the floating window commands.
	/// </summary>
	public FloatingWindowCommands(IFloatingWindowPlugin floatingWindowPlugin)
		: base(floatingWindowPlugin.Name)
	{
		_floatingWindowPlugin = floatingWindowPlugin;

		Add(
				identifier: "toggle_window_floating",
				title: "Toggle window floating",
				() => _floatingWindowPlugin.ToggleWindowFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_F)
			)
			.Add(
				identifier: "mark_window_as_floating",
				title: "Mark window as floating",
				() => _floatingWindowPlugin.MarkWindowAsFloating(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_M)
			)
			.Add(
				identifier: "mark_window_as_docked",
				title: "Mark window as docked",
				() => _floatingWindowPlugin.MarkWindowAsDocked(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_D)
			);
	}
}
