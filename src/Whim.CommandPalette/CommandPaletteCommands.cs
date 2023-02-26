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
							InitialText = _configContext.WorkspaceManager.ActiveWorkspace.Name,
							Prompt = "Rename workspace"
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
							Hint = "Enter new workspace name",
							Prompt = "Create workspace"
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
					IWorkspace activeWorkspace = _configContext.WorkspaceManager.ActiveWorkspace;
					List<CommandItem> items = new();
					foreach (IWorkspace workspace in _configContext.WorkspaceManager)
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
		};

	private string MoveMultipleWindowsToWorkspaceCommandIdentifier => $"{Name}.move_multiple_windows_to_workspace";

	/// <summary>
	/// Creates the select options for moving multiple windows to a workspace.
	/// </summary>
	public SelectOption[] CreateMoveWindowsToWorkspaceOptions()
	{
		// All the windows in all the workspaces.
		IEnumerable<IWindow> windows = _configContext.WorkspaceManager
			.Select(w => w.Windows)
			.SelectMany(w => w)
			.OrderBy(w => w.Title);

		return windows
			.Select(
				w =>
					new SelectOption()
					{
						Id = $"{MoveMultipleWindowsToWorkspaceCommandIdentifier}.windows.{w.Title}",
						Title = w.Title,
						IsEnabled = true,
						IsSelected = false
					}
			)
			.ToArray();
	}

	/// <summary>
	/// Move multiple windows to workspace command creator.
	/// </summary>
	/// <param name="windows"></param>
	/// <param name="workspace"></param>
	/// <returns>The move multiple windows to workspace command.</returns>
	public CommandItem MoveMultipleWindowsToWorkspaceCreator(IEnumerable<IWindow> windows, IWorkspace workspace) =>
		new()
		{
			Command = new Command(
				identifier: $"{MoveMultipleWindowsToWorkspaceCommandIdentifier}.workspaces.{workspace.Name}",
				title: workspace.Name,
				callback: () =>
				{
					foreach (IWindow window in windows)
					{
						_configContext.WorkspaceManager.MoveWindowToWorkspace(workspace, window);
					}
				}
			)
		};

	/// <summary>
	/// Callback to activate a menu variant to select the workspace to move the windows to.
	/// </summary>
	public void MoveMultipleWindowsToWorkspaceCallback(IEnumerable<SelectOption> options)
	{
		IEnumerable<string> selectedWindowNames = options.Where(o => o.IsSelected).Select(o => o.Title);

		IEnumerable<IWindow> windows = _configContext.WorkspaceManager
			.SelectMany(w => w.Windows)
			.Where(w => selectedWindowNames.Contains(w.Title));

		IEnumerable<CommandItem> items = _configContext.WorkspaceManager.Select(
			w => MoveMultipleWindowsToWorkspaceCreator(windows, w)
		);

		_commandPalettePlugin.Activate(new MenuVariantConfig() { Hint = "Select workspace", Commands = items });
	}

	/// <summary>
	/// Moves the current window to a given workspace.
	/// </summary>
	public CommandItem MoveMultipleWindowsToWorkspace =>
		new()
		{
			Command = new Command(
				identifier: MoveMultipleWindowsToWorkspaceCommandIdentifier,
				title: "Move multiple windows to workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new SelectVariantConfig()
						{
							Hint = "Select windows",
							Options = CreateMoveWindowsToWorkspaceOptions(),
							AllowMultiSelect = true,
							Callback = MoveMultipleWindowsToWorkspaceCallback
						}
					)
			)
		};

	/// <inheritdoc />
	public IEnumerator<CommandItem> GetEnumerator()
	{
		yield return ToggleCommandPaletteCommand;
		yield return RenameWorkspaceCommand;
		yield return CreateWorkspaceCommand;
		yield return MoveWindowToWorkspaceCommand;
		yield return MoveMultipleWindowsToWorkspace;
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
