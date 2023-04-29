using System.Collections.Generic;
using System.Linq;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.CommandPalette;

/// <summary>
/// The commands for the command palette plugin.
/// </summary>
public class CommandPaletteCommands : PluginCommands
{
	private readonly IContext _context;
	private readonly ICommandPalettePlugin _commandPalettePlugin;

	/// <summary>
	/// Creates a new instance of the command palette commands.
	/// </summary>
	public CommandPaletteCommands(IContext context, ICommandPalettePlugin commandPalettePlugin)
		: base(commandPalettePlugin.Name)
	{
		_context = context;
		_commandPalettePlugin = commandPalettePlugin;

		Add(
				identifier: "toggle",
				title: "Toggle command palette",
				() => _commandPalettePlugin.Activate(),
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_K)
			)
			.Add(
				identifier: "rename_workspace",
				title: "Rename workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new FreeTextVariantConfig()
						{
							Callback = (text) => _context.WorkspaceManager.ActiveWorkspace.Name = text,
							Hint = "Enter new workspace name",
							InitialText = _context.WorkspaceManager.ActiveWorkspace.Name,
							Prompt = "Rename workspace"
						}
					)
			)
			.Add(
				identifier: "create_workspace",
				title: "Create workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new FreeTextVariantConfig()
						{
							Callback = (text) =>
							{
								IWorkspace workspace = _context.WorkspaceManager.WorkspaceFactory(_context, text);
								_context.WorkspaceManager.Add(workspace);
							},
							Hint = "Enter new workspace name",
							Prompt = "Create workspace"
						}
					)
			)
			.Add(
				identifier: "move_window_to_workspace",
				title: "Move window to workspace",
				callback: () =>
				{
					IWorkspace activeWorkspace = _context.WorkspaceManager.ActiveWorkspace;
					List<ICommand> items = new();
					foreach (IWorkspace workspace in _context.WorkspaceManager)
					{
						if (workspace != activeWorkspace)
						{
							items.Add(MoveWindowToWorkspaceCommandCreator(workspace));
						}
					}

					_commandPalettePlugin.Activate(
						new MenuVariantConfig() { Hint = "Select workspace", Commands = items }
					);
				}
			)
			.Add(
				identifier: "move_multiple_windows_to_workspace",
				title: "Move multiple windows to workspace",
				callback: () =>
				{
					IWorkspace activeWorkspace = _context.WorkspaceManager.ActiveWorkspace;
					List<ICommand> items = new();
					foreach (IWorkspace workspace in _context.WorkspaceManager)
					{
						if (workspace != activeWorkspace)
						{
							items.Add(MoveMultipleWindowsToWorkspaceCreator(activeWorkspace.Windows, workspace));
						}
					}

					_commandPalettePlugin.Activate(
						new MenuVariantConfig() { Hint = "Select workspace", Commands = items }
					);
				}
			);
	}

	/// <summary>
	/// Move multiple windows to workspace command creator.
	/// </summary>
	/// <param name="windows"></param>
	/// <param name="workspace"></param>
	/// <returns>The move multiple windows to workspace command.</returns>
	internal ICommand MoveMultipleWindowsToWorkspaceCreator(IEnumerable<IWindow> windows, IWorkspace workspace) =>
		new Command(
			identifier: $"{PluginName}.move_multiple_windows_to_workspace.{workspace.Name}",
			title: workspace.Name,
			callback: () =>
				_commandPalettePlugin.Activate(
					new SelectVariantConfig()
					{
						Hint = "Select windows",
						Options = CreateMoveWindowsToWorkspaceOptions(),
						Callback = MoveMultipleWindowsToWorkspaceCallback
					}
				)
		);

	/// <summary>
	/// Creates the select options for moving multiple windows to a workspace.
	/// </summary>
	public SelectOption[] CreateMoveWindowsToWorkspaceOptions()
	{
		// All the windows in all the workspaces.
		IEnumerable<IWindow> windows = _context.WorkspaceManager
			.Select(w => w.Windows)
			.SelectMany(w => w)
			.OrderBy(w => w.Title);

		return windows
			.Select(
				w =>
					new SelectOption()
					{
						Id = $"{PluginName}.move_multiple_windows_to_workspace.{w.Title}",
						Title = w.Title,
						IsEnabled = true,
						IsSelected = false
					}
			)
			.ToArray();
	}

	/// <summary>
	/// Callback to activate a menu variant to select the workspace to move the windows to.
	/// </summary>
	public void MoveMultipleWindowsToWorkspaceCallback(IEnumerable<SelectOption> options)
	{
		IEnumerable<string> selectedWindowNames = options.Where(o => o.IsSelected).Select(o => o.Title);

		IEnumerable<IWindow> windows = _context.WorkspaceManager
			.SelectMany(w => w.Windows)
			.Where(w => selectedWindowNames.Contains(w.Title));

		IEnumerable<ICommand> items = _context.WorkspaceManager.Select(
			w => MoveMultipleWindowsToWorkspaceCreator(windows, w)
		);

		_commandPalettePlugin.Activate(new MenuVariantConfig() { Hint = "Select workspace", Commands = items });
	}

	/// <summary>
	/// Move window to workspace command creator.
	/// </summary>
	/// <param name="workspace">The workspace to move the window to.</param>
	/// <returns>The move window to workspace command.</returns>
	internal ICommand MoveWindowToWorkspaceCommandCreator(IWorkspace workspace) =>
		new Command(
			identifier: $"{PluginName}.move_window_to_workspace.{workspace.Name}",
			title: $"Move window to workspace \"{workspace.Name}\"",
			callback: () => _context.WorkspaceManager.MoveWindowToWorkspace(workspace)
		);
}
