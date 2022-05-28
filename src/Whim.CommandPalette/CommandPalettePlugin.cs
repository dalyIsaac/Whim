using System.Collections.Generic;

namespace Whim.CommandPalette;

public class CommandPalettePlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private CommandPaletteWindow? _commandPaletteWindow;
	public CommandPaletteConfig Config { get; }

	public CommandPalettePlugin(IConfigContext configContext, CommandPaletteConfig commandPaletteConfig)
	{
		_configContext = configContext;
		Config = commandPaletteConfig;
	}

	public void PreInitialize()
	{
		_configContext.FilterManager.IgnoreTitleMatch(CommandPaletteConfig.Title);
	}

	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_commandPaletteWindow = new CommandPaletteWindow(_configContext, this);
	}

	/// <summary>
	/// Activate the command palette window.
	/// </summary>
	/// <param name="items"></param>
	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null)
	{
		_commandPaletteWindow?.Activate(
			items,
			_configContext.MonitorManager.FocusedMonitor
		);
	}

	/// <summary>
	/// Hide the command palette.
	/// </summary>
	public void Hide()
	{
		_commandPaletteWindow?.Hide();
	}

	/// <summary>
	/// Toggle the visibility of the command palette.
	/// </summary>
	public void Toggle()
	{
		_commandPaletteWindow?.Toggle();
	}
}
