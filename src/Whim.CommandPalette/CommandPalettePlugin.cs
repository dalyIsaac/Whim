using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <inheritdoc />
public class CommandPalettePlugin : ICommandPalettePlugin
{
	private readonly IConfigContext _configContext;
	private CommandPaletteWindow? _commandPaletteWindow;
	private bool _disposedValue;

	/// <inheritdoc />
	public string Name => "whim.commmand_palette";

	/// <summary>
	/// The configuration for the command palette plugin.
	/// </summary>
	public CommandPaletteConfig Config { get; }

	/// <summary>
	/// Creates a new instance of the command palette plugin.
	/// </summary>
	/// <param name="configContext"></param>
	/// <param name="commandPaletteConfig"></param>
	public CommandPalettePlugin(IConfigContext configContext, CommandPaletteConfig commandPaletteConfig)
	{
		_configContext = configContext;
		Config = commandPaletteConfig;
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_configContext.FilterManager.IgnoreTitleMatch(CommandPaletteConfig.Title);
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_commandPaletteWindow = new CommandPaletteWindow(_configContext, this);
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	public void Activate(BaseVariantConfig? config = null)
	{
		_commandPaletteWindow?.Activate(
			config: config
				?? Config.ActivationConfig
				?? new MenuVariantConfig() { Commands = _configContext.CommandManager },
			monitor: _configContext.MonitorManager.FocusedMonitor
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

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_commandPaletteWindow?.Close();
			}

			// free unmanaged resources (unmanaged objects) and override finalizer
			// set large fields to null
			_disposedValue = true;
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <inheritdoc />
	public IEnumerable<CommandItem> Commands => new CommandPaletteCommands(_configContext, this);
}
