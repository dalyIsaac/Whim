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
	private readonly ICommandPalettePlugin _commandPalettePlugin;
	private string Name => _commandPalettePlugin.Name;

	/// <summary>
	/// Creates a new instance of the command palette commands.
	/// </summary>
	public CommandPaletteCommands(IConfigContext configContext, ICommandPalettePlugin commandPalettePlugin)
	{
		_configContext = configContext;
		_commandPalettePlugin = commandPalettePlugin;
	}

	/// <summary>
	/// Toggle command palette command.
	/// </summary>
	public CommandItem ToggleCommandPaletteCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.toggle",
				title: "Toggle command palette",
				() => _commandPalettePlugin.Activate()
			),
			Keybind = new Keybind(CoreCommands.WinShift, VIRTUAL_KEY.VK_K)
		};

	/// <summary>
	/// Rename workspace command.
	/// </summary>
	public CommandItem RenameWorkspaceCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.rename_workspace",
				title: "Rename workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new FreeTextVariantConfig()
						{
							Callback = (text) => _configContext.WorkspaceManager.ActiveWorkspace.Name = text,
							Hint = "Enter new workspace name",
							InitialText = _configContext.WorkspaceManager.ActiveWorkspace.Name
						}
					)
			)
		};

	/// <summary>
	/// Create workspace command.
	/// </summary>
	public CommandItem CreateWorkspaceCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.create_workspace",
				title: "Create workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new FreeTextVariantConfig()
						{
							Callback = (text) =>
							{
								IWorkspace workspace = _configContext.WorkspaceManager.WorkspaceFactory(
									_configContext,
									text
								);
								_configContext.WorkspaceManager.Add(workspace);
							},
							Hint = "Enter new workspace name"
						}
					)
			)
		};

	/// <summary>
	/// Move window to workspace command creator.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <returns>The move window to workspace command.</returns>
	public CommandItem MoveWindowToWorkspaceCommandCreator(IWorkspace workspace) =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_to_workspace",
				title: $"Move window to workspace \"{workspace.Name}\"",
				callback: () => _configContext.WorkspaceManager.MoveWindowToWorkspace(workspace)
			)
		};

	/// <summary>
	/// Move window to workspace command.
	/// </summary>
	public CommandItem MoveWindowToWorkspaceCommand =>
		new()
		{
			Command = new Command(
				identifier: $"{Name}.move_window_to_workspace",
				title: "Move window to workspace",
				callback: () =>
				{
					IEnumerable<CommandItem> items = _configContext.WorkspaceManager.Select(
						w => MoveWindowToWorkspaceCommandCreator(w)
					);

					_commandPalettePlugin.Activate(
						new MenuVariantConfig() { Hint = "Select workspace", Commands = items }
					);
				}
			)
		};

	/// <inheritdoc />
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return ToggleCommandPaletteCommand;
		yield return RenameWorkspaceCommand;
		yield return CreateWorkspaceCommand;
		yield return MoveWindowToWorkspaceCommand;
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
