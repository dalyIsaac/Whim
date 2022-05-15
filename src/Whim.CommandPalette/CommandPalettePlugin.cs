using System.Collections.Generic;

namespace Whim.CommandPalette;

public class CommandPalettePlugin : IPlugin
{
	private readonly IConfigContext _configContext;
	private readonly CommandPaletteConfig _commandPaletteConfig;
	private readonly Dictionary<string, uint> _commandCounts = new();
	private CommandPaletteWindow? _commandPaletteWindow;

	public CommandPalettePlugin(IConfigContext configContext, CommandPaletteConfig commandPaletteConfig)
	{
		_configContext = configContext;
		_commandPaletteConfig = commandPaletteConfig;
	}

	public void PreInitialize()
	{
		_configContext.FilterManager.IgnoreTitleMatch(CommandPaletteConfig.Title);
	}

	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_commandPaletteWindow = new CommandPaletteWindow(_configContext, _commandPaletteConfig);
	}

	public void Activate(IEnumerable<(ICommand, IKeybind?)>? items = null)
	{
		_commandPaletteWindow?.Activate(
			items,
			_configContext.MonitorManager.FocusedMonitor
		);
	}

	public void Hide()
	{
		_commandPaletteWindow?.Hide();
	}

	public void Toggle()
	{
		_commandPaletteWindow?.Toggle();
	}
}
