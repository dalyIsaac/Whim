using System;
using System.Collections.Generic;
using System.Linq;
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
	public void Activate()
	{
		_commandPaletteWindow?.Activate(
			config: Config.ActivationConfig,
			items: _configContext.CommandManager,
			monitor: _configContext.MonitorManager.FocusedMonitor
		);
	}

	/// <summary>
	/// Activate the command palette with the given config.
	/// </summary>
	/// <param name="config"></param>
	/// <param name="items"></param>
	public void ActivateWithConfig(BaseCommandPaletteActivationConfig config, IEnumerable<CommandItem>? items = null)
	{
		_commandPaletteWindow?.Activate(
			config: config,
			items: items,
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
	public CommandItem[] GetCommands() => new CommandItem[]
	{
			ToggleCommand,
			RenameWorkspaceCommand,
			CreateWorkspaceCommand,
			MoveWindowToWorkspaceCommand
	};

	/// <summary>
	/// Toggle command palette command.
	/// </summary>
	public CommandItem ToggleCommand => new(
		new Command(
			identifier: $"{Name}.toggle",
			title: "Toggle command palette",
			callback: () => Activate()
		),
		new Keybind(DefaultCommands.WinShift, VIRTUAL_KEY.VK_K)
	);

	/// <summary>
	/// Rename workspace command.
	/// </summary>
	public CommandItem RenameWorkspaceCommand => new(
		new Command(
			identifier: $"{Name}.rename_workspace",
			title: "Rename workspace",
			callback: () => ActivateWithConfig(
				new CommandPaletteFreeTextActivationConfig(
					callback: (text) => _configContext.WorkspaceManager.ActiveWorkspace.Name = text,
					hint: "Enter new workspace name",
					initialText: _configContext.WorkspaceManager.ActiveWorkspace.Name
				)
			)
		)
	);

	/// <summary>
	/// Create workspace command.
	/// </summary>
	public CommandItem CreateWorkspaceCommand => new(
		new Command(
			identifier: $"{Name}.create_workspace",
			title: "Create workspace",
			callback: () => ActivateWithConfig(
				new CommandPaletteFreeTextActivationConfig(
					callback: (text) =>
					{
						IWorkspace workspace = _configContext.WorkspaceManager.WorkspaceFactory(_configContext, text);
						_configContext.WorkspaceManager.Add(workspace);
					},
					hint: "Enter new workspace name"
				)
			)
		)
	);

	/// <summary>
	/// Move window to workspace command.
	/// </summary>
	public CommandItem MoveWindowToWorkspaceCommand => new(
		new Command(
			identifier: $"{Name}.move_window_to_workspace",
			title: "Move window to workspace",
			callback: () =>
			{
				IEnumerable<CommandItem> items = _configContext.WorkspaceManager
					.Select(w => new CommandItem(
						new Command(
							identifier: $"{Name}.move_window_to_workspace.{w.Name}",
							title: w.Name,
							callback: () => _configContext.WorkspaceManager.MoveWindowToWorkspace(w)
						)
					));

				ActivateWithConfig(
					config: new CommandPaletteMenuActivationConfig(
						hint: "Select workspace"
					),
					items
				);
			}
		)
	);
}
