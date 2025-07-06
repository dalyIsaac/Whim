using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Whim.CommandPalette;

/// <inheritdoc />
public class CommandPalettePlugin : ICommandPalettePlugin
{
	private readonly IContext _context;
	private CommandPaletteWindow? _commandPaletteWindow;
	private bool _disposedValue;

	/// <summary>
	/// <c>whim.command_palette</c>
	/// </summary>
	public string Name => "whim.command_palette";

	/// <summary>
	/// The configuration for the command palette plugin.
	/// </summary>
	public CommandPaletteConfig Config { get; }

	/// <inheritdoc />
	public IPluginCommands PluginCommands { get; }

	/// <summary>
	/// Creates a new instance of the command palette plugin.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="commandPaletteConfig"></param>
	public CommandPalettePlugin(IContext context, CommandPaletteConfig commandPaletteConfig)
	{
		_context = context;
		Config = commandPaletteConfig;

		PluginCommands = new CommandPaletteCommands(_context, this);
	}

	/// <inheritdoc/>
	public void PreInitialize()
	{
		_context.FilterManager.AddTitleMatchFilter(CommandPaletteConfig.Title);
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
			monitor: _context.Store.Pick(Pickers.PickActiveMonitor())
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
		Logger.Debug("Toggling command palette");
		if (_commandPaletteWindow?.ViewModel.IsVisible == true)
		{
			_commandPaletteWindow.ViewModel.RequestHide();
		}
		else
		{
			Activate();
		}
	}

	/// <inheritdoc/>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				// dispose managed state (managed objects)
				_commandPaletteWindow?.Dispose();
				_commandPaletteWindow?.Close();
				_commandPaletteWindow = null;
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

	/// <inheritdoc/>
	public void LoadState(JsonElement state)
	{
		// Get the activation config from the state.
		if (state.TryGetProperty("activationConfig", out JsonElement activationConfig))
		{
			Config.ActivationConfig.Matcher.LoadState(activationConfig);
		}
	}

	/// <inheritdoc/>
	public JsonElement? SaveState()
	{
		Dictionary<string, JsonElement> state = [];

		if (Config.ActivationConfig.Matcher.SaveState() is JsonElement activationConfig)
		{
			state.Add("activationConfig", activationConfig);
		}

		return JsonSerializer.SerializeToElement(state);
	}
}
