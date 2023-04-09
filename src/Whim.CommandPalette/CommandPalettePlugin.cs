using System;
using System.Collections.Generic;

namespace Whim.CommandPalette;

/// <inheritdoc />
public class CommandPalettePlugin : ICommandPalettePlugin
{
	private readonly IContext _context;
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
	/// <param name="context"></param>
	/// <param name="commandPaletteConfig"></param>
	public CommandPalettePlugin(IContext context, CommandPaletteConfig commandPaletteConfig)
	{
		_context = context;
		Config = commandPaletteConfig;
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_context.FilterManager.IgnoreTitleMatch(CommandPaletteConfig.Title);
	}

	/// <inheritdoc/>
	public void PostInitialize()
	{
		// The window must be created on the UI thread (so don't do it in the constructor).
		_commandPaletteWindow = new CommandPaletteWindow(_context, this);
	}

	/// <summary>
	/// Activate the command palette.
	/// </summary>
	public void Activate(BaseVariantConfig? config = null)
	{
		_commandPaletteWindow?.Activate(
			config: config ?? Config.ActivationConfig,
			monitor: _context.MonitorManager.FocusedMonitor
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
	public IEnumerable<CommandItem> Commands => new CommandPaletteCommands(_context, this);
}
