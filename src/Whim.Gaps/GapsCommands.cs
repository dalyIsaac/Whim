using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Gaps;

/// <summary>
/// The commands for the floating layout plugin.
/// </summary>
public class GapsCommands : PluginCommands
{
	private readonly IGapsPlugin _gapsPlugin;

	/// <summary>
	/// Creates a new instance of the floating layout commands.
	/// </summary>
	public GapsCommands(IGapsPlugin gapsPlugin)
		: base(gapsPlugin.Name)
	{
		_gapsPlugin = gapsPlugin;

		Add(
				identifier: "outer.increase",
				title: "Increase outer gap",
				() => _gapsPlugin.UpdateOuterGap(_gapsPlugin.GapsConfig.DefaultOuterDelta),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_L)
			)
			.Add(
				identifier: "outer.decrease",
				title: "Decrease outer gap",
				() => _gapsPlugin.UpdateOuterGap(-_gapsPlugin.GapsConfig.DefaultOuterDelta),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_H)
			)
			.Add(
				identifier: "inner.increase",
				title: "Increase inner gap",
				() => _gapsPlugin.UpdateInnerGap(_gapsPlugin.GapsConfig.DefaultInnerDelta),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_K)
			)
			.Add(
				identifier: "inner.decrease",
				title: "Decrease inner gap",
				() => _gapsPlugin.UpdateInnerGap(-_gapsPlugin.GapsConfig.DefaultInnerDelta),
				keybind: new Keybind(IKeybind.WinCtrlShift, VIRTUAL_KEY.VK_J)
			);
	}
}
