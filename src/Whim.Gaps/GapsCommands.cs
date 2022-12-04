using System.Collections;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.Gaps;

/// <summary>
/// The commands for the floating layout plugin.
/// </summary>
public class GapsCommands : IEnumerable<CommandItem>
{
	private readonly IGapsPlugin _gapsPlugin;
	private string Name => _gapsPlugin.Name;

	/// <summary>
	/// Creates a new instance of the floating layout commands.
	/// </summary>
	public GapsCommands(IGapsPlugin gapsPlugin)
	{
		_gapsPlugin = gapsPlugin;
	}

	/// <summary>
	/// Increase outer gap command.
	/// </summary>
	public CommandItem IncreaseOuterGapCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.outer.increase",
				title: "Increase outer gap",
				callback: () => _gapsPlugin.UpdateOuterGap(_gapsPlugin.GapsConfig.DefaultOuterDelta)
			),
			Keybind = new Keybind(CoreCommands.WinCtrlShift, VIRTUAL_KEY.VK_L)
		};

	/// <summary>
	/// Decrease outer gap command.
	/// </summary>
	public CommandItem DecreaseOuterGapCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.outer.decrease",
				title: "Decrease outer gap",
				callback: () => _gapsPlugin.UpdateOuterGap(-_gapsPlugin.GapsConfig.DefaultOuterDelta)
			),
			Keybind = new Keybind(CoreCommands.WinCtrlShift, VIRTUAL_KEY.VK_H)
		};

	/// <summary>
	/// Increase inner gap command.
	/// </summary>
	public CommandItem IncreaseInnerGapCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.inner.increase",
				title: "Increase inner gap",
				callback: () => _gapsPlugin.UpdateInnerGap(_gapsPlugin.GapsConfig.DefaultInnerDelta)
			),
			Keybind = new Keybind(CoreCommands.WinCtrlShift, VIRTUAL_KEY.VK_K)
		};

	/// <summary>
	/// Decrease inner gap command.
	/// </summary>
	public CommandItem DecreaseInnerGapCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.inner.decrease",
				title: "Decrease inner gap",
				callback: () => _gapsPlugin.UpdateInnerGap(-_gapsPlugin.GapsConfig.DefaultInnerDelta)
			),
			Keybind = new Keybind(CoreCommands.WinCtrlShift, VIRTUAL_KEY.VK_J)
		};

	/// <inheritdoc/>
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return IncreaseOuterGapCommand;
		yield return DecreaseOuterGapCommand;
		yield return IncreaseInnerGapCommand;
		yield return DecreaseInnerGapCommand;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
