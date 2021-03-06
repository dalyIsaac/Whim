using System;
using System.Collections.Generic;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.CommandPalette;

/// <summary>
/// CommandPalettePlugin displays a rudimentary command palette window. It allows the user to
/// interact with the loaded commands of Whim.
/// </summary>
public class CommandPalettePlugin : IPlugin, IDisposable
{
	private readonly IConfigContext _configContext;
	private CommandPaletteWindow? _commandPaletteWindow;
	private bool _disposedValue;

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
	public (ICommand, IKeybind?)[] GetCommands() => new (ICommand, IKeybind?)[]
	{
		// Toggle command palette
		(
			new Command(
				identifier: "command_palette.toggle",
				title: "Toggle command palette",
				callback: () => Activate()
			),
			new Keybind(DefaultCommands.WinShift, VIRTUAL_KEY.VK_K)
		)
	};
}
