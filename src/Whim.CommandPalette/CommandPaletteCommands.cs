using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.CommandPalette;

/// <summary>
/// The commands for the command palette plugin.
/// </summary>
public class CommandPaletteCommands : IEnumerable<CommandItem>
{
	private readonly IConfigContext _configContext;
	private readonly CommandPalettePlugin _commandPalettePlugin;
	private string Name => _commandPalettePlugin.Name;

	/// <summary>
	/// Creates a new instance of the command palette commands.
	/// </summary>
	public CommandPaletteCommands(IConfigContext configContext, CommandPalettePlugin commandPalettePlugin)
	{
		_configContext = configContext;
		_commandPalettePlugin = commandPalettePlugin;
	}

	/// <summary>
	/// Toggle command palette command.
	/// </summary>
	public CommandItem ToggleCommand => new(
		new Command(
			identifier: $"{Name}.toggle",
			title: "Toggle command palette",
			callback: () => _commandPalettePlugin.Activate()
		),
		new Keybind(CoreCommands.WinShift, VIRTUAL_KEY.VK_K)
	);

	/// <summary>
	/// Rename workspace command.
	/// </summary>
	public CommandItem RenameWorkspaceCommand => new(
		new Command(
			identifier: $"{Name}.rename_workspace",
			title: "Rename workspace",
			callback: () => _commandPalettePlugin.ActivateWithConfig(
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
			callback: () => _commandPalettePlugin.ActivateWithConfig(
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

				_commandPalettePlugin.ActivateWithConfig(
					config: new CommandPaletteMenuActivationConfig(
						hint: "Select workspace"
					),
					items
				);
			}
		)
	);

	/// <inheritdoc />
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return ToggleCommand;
		yield return RenameWorkspaceCommand;
		yield return CreateWorkspaceCommand;
		yield return MoveWindowToWorkspaceCommand;
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
