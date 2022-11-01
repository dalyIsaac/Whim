using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <summary>
/// CommandPalettePlugin displays a rudimentary command palette window. It allows the user to
/// interact with the loaded commands of Whim.
/// </summary>
public interface ICommandPalettePlugin : IPlugin, IDisposable
{
	/// <summary>
	/// The configuration for the command palette plugin.
	/// </summary>
	public CommandPaletteConfig Config { get; }

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	public void Activate();

	/// <summary>
	/// Activate the command palette with the given config.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="items"></param>
	public void ActivateWithConfig(BaseCommandPaletteActivationConfig config, IEnumerable<CommandItem>? items = null);

	/// <summary>
	/// Hide the command palette.
	/// </summary>
	public void Hide();

	/// <summary>
	/// Toggle the visibility of the command palette.
	/// </summary>
	public void Toggle();
}
