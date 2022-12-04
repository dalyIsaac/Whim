using System.Collections;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.FloatingLayout;

/// <summary>
/// The commands for the floating layout plugin.
/// </summary>
public class FloatingLayoutCommands : IEnumerable<CommandItem>
{
	private readonly IFloatingLayoutPlugin _floatingLayoutPlugin;
	private string Name => _floatingLayoutPlugin.Name;

	/// <summary>
	/// Creates a new instance of the floating layout commands.
	/// </summary>
	public FloatingLayoutCommands(IFloatingLayoutPlugin floatingLayoutPlugin)
	{
		_floatingLayoutPlugin = floatingLayoutPlugin;
	}

	/// <summary>
	/// Toggle window floating command.
	/// </summary>
	public CommandItem ToggleWindowFloatingCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.toggle_window_floating",
				title: "Toggle window floating",
				callback: () => _floatingLayoutPlugin.ToggleWindowFloating()
			),
			Keybind = new Keybind(CoreCommands.WinShift, VIRTUAL_KEY.VK_F)
		};

	/// <summary>
	/// Mark window as floating command.
	/// </summary>
	public CommandItem MarkWindowAsFloatingCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.mark_window_as_floating",
				title: "Mark window as floating",
				callback: () => _floatingLayoutPlugin.MarkWindowAsFloating()
			),
			Keybind = new Keybind(CoreCommands.WinShift, VIRTUAL_KEY.VK_M)
		};

	/// <summary>
	/// Mark window as docked command.
	/// </summary>
	public CommandItem MarkWindowAsDockedCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.mark_window_as_docked",
				title: "Mark window as docked",
				callback: () => _floatingLayoutPlugin.MarkWindowAsDocked()
			),
			Keybind = new Keybind(CoreCommands.WinShift, VIRTUAL_KEY.VK_D)
		};

	/// <inheritdoc/>
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return ToggleWindowFloatingCommand;
		yield return MarkWindowAsFloatingCommand;
		yield return MarkWindowAsDockedCommand;
	}

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
