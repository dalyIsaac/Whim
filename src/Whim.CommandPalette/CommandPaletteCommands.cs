using System.Collections.Generic;
using System.Linq;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Whim.CommandPalette;

/// <summary>
/// The commands for the command palette plugin.
/// </summary>
public class CommandPaletteCommands : PluginCommands
{
	private readonly IContext _ctx;
	private readonly ICommandPalettePlugin _commandPalettePlugin;

	/// <summary>
	/// Creates a new instance of the command palette commands.
	/// </summary>
	public CommandPaletteCommands(IContext context, ICommandPalettePlugin commandPalettePlugin)
		: base(commandPalettePlugin.Name)
	{
		_ctx = context;
		_commandPalettePlugin = commandPalettePlugin;

		_ = Add(
				identifier: "toggle",
				title: "Toggle command palette",
				_commandPalettePlugin.Toggle,
				keybind: new Keybind(IKeybind.WinShift, VIRTUAL_KEY.VK_K)
			)
			.Add(identifier: "activate_workspace", title: "Activate workspace", callback: ActivateWorkspaceCallback)
			.Add(
				identifier: "rename_workspace",
				title: "Rename workspace",
				callback: () =>
					_commandPalettePlugin.Activate(
						new FreeTextVariantConfig()
						{
							Callback = (text) => _ctx.WorkspaceManager.ActiveWorkspace.Name = text,
							Hint = "Enter new workspace name",
							InitialText = _ctx.WorkspaceManager.ActiveWorkspace.Name,
							Prompt = "Rename workspace",
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
							Callback = (name) => _ctx.WorkspaceManager.Add(name),
							Hint = "Enter new workspace name",
							Prompt = "Create workspace",
						}
					)
			)
			.Add(
				identifier: "move_window_to_workspace",
				title: "Move window to workspace",
				callback: () =>
				{
					IWorkspace activeWorkspace = _ctx.WorkspaceManager.ActiveWorkspace;
					List<ICommand> items = [];
					foreach (IWorkspace workspace in _ctx.WorkspaceManager)
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
					_commandPalettePlugin.Activate(
						new SelectVariantConfig()
						{
							Hint = "Select windows",
							Options = CreateMoveWindowsToWorkspaceOptions(),
							Callback = MoveMultipleWindowsToWorkspaceCallback,
						}
					)
			)
			.Add(
				identifier: "remove_window",
				title: "Select window to remove from Whim",
				callback: () =>
					_commandPalettePlugin.Activate(
						new MenuVariantConfig()
						{
							Hint = "Select window",
							Commands = _ctx.WorkspaceManager.ActiveWorkspace.Windows.Select(w =>
								RemoveWindowCommandCreator(w)
							),
							ConfirmButtonText = "Remove",
						}
					)
			)
			.Add(
				identifier: "find_focus_window",
				title: "Find and focus window",
				callback: () =>
					_commandPalettePlugin.Activate(
						new MenuVariantConfig()
						{
							Hint = "Find window",
							ConfirmButtonText = "Focus",
							Commands = _ctx
								.WorkspaceManager.SelectMany(w => w.Windows)
								.Select(w => FocusWindowCommandCreator(w)),
						}
					)
			);
	}

	private void ActivateWorkspaceCallback()
	{
		IWorkspace activeWorkspace = _ctx.Store.Pick(Pickers.PickActiveWorkspace());
		List<ICommand> items = [];
		foreach (IWorkspace workspace in _ctx.Store.Pick(Pickers.PickWorkspaces()))
		{
			if (workspace != activeWorkspace)
			{
				items.Add(
					new Command(
						identifier: $"{PluginName}.activate_workspace.{workspace.BackingName}",
						title: workspace.BackingName,
						callback: () => _ctx.Store.Dispatch(new ActivateWorkspaceTransform(workspace.Id))
					)
				);
			}
		}

		_commandPalettePlugin.Activate(
			new MenuVariantConfig()
			{
				Hint = "Select workspace",
				Commands = items,
				ConfirmButtonText = "Activate",
			}
		);
	}

	/// <summary>
	/// Creates the select options for moving multiple windows to a workspace.
	/// </summary>
	public SelectOption[] CreateMoveWindowsToWorkspaceOptions()
	{
		// All the windows in all the workspaces.
		IEnumerable<IWindow> windows = _ctx
			.WorkspaceManager.Select(w => w.Windows)
			.SelectMany(w => w)
			.OrderBy(w => w.Title);

		return windows
			.Select(w => new SelectOption()
			{
				Id = $"{PluginName}.move_multiple_windows_to_workspace.{w.Title}",
				Title = w.Title,
				IsEnabled = true,
				IsSelected = false,
			})
			.ToArray();
	}

	/// <summary>
	/// Move multiple windows to workspace command creator.
	/// </summary>
	/// <param name="windows"></param>
	/// <param name="workspace"></param>
	/// <returns>The move multiple windows to workspace command.</returns>
	internal ICommand MoveMultipleWindowsToWorkspaceCreator(IReadOnlyList<IWindow> windows, IWorkspace workspace) =>
		new Command(
			identifier: $"{PluginName}.move_multiple_windows_to_workspace.{workspace.Name}",
			title: workspace.Name,
			callback: () =>
			{
				foreach (IWindow window in windows)
				{
					_ctx.Butler.MoveWindowToWorkspace(workspace, window);
				}
			}
		);

	/// <summary>
	/// Callback to activate a menu variant to select the workspace to move the windows to.
	/// </summary>
	public void MoveMultipleWindowsToWorkspaceCallback(IEnumerable<SelectOption> options)
	{
		IEnumerable<string> selectedWindowNames = options.Where(o => o.IsSelected).Select(o => o.Title);

		IReadOnlyList<IWorkspace> workspaces = [.. _ctx.WorkspaceManager];
		IReadOnlyList<IWindow> windows = workspaces
			.SelectMany(w => w.Windows)
			.Where(w => selectedWindowNames.Contains(w.Title))
			.ToArray();

		IEnumerable<ICommand> commands = workspaces.Select(w => MoveMultipleWindowsToWorkspaceCreator(windows, w));

		_commandPalettePlugin.Activate(new MenuVariantConfig() { Hint = "Select workspace", Commands = commands });
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
			callback: () => _ctx.Butler.MoveWindowToWorkspace(workspace)
		);

	/// <summary>
	/// Untrack window command creator.
	/// </summary>
	/// <remarks>
	/// Creates a command to remove a window from the active workspace. This is useful as Whim can
	/// sometimes not realise that a window has been closed in niche cases.
	/// For example, when waking/closing from sleep.
	/// </remarks>
	/// <param name="window">The window to untrack.</param>
	/// <returns>The untrack window command.</returns>
	internal ICommand RemoveWindowCommandCreator(IWindow window) =>
		new Command(
			identifier: $"{PluginName}.remove_window.{window.Title}",
			title: window.Title,
			callback: () =>
			{
				IWorkspace workspace = _ctx.WorkspaceManager.ActiveWorkspace;
				workspace.RemoveWindow(window);
				workspace.DoLayout();
			}
		);

	/// <summary>
	/// Focus window command creator.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	internal ICommand FocusWindowCommandCreator(IWindow window) =>
		new Command(
			identifier: $"{PluginName}.focus_window.{window.Title}",
			title: window.Title,
			callback: () =>
			{
				IWorkspace? workspace = _ctx.Butler.Pantry.GetWorkspaceForWindow(window);
				if (workspace == null)
				{
					return;
				}

				if (_ctx.Butler.Pantry.GetMonitorForWorkspace(workspace) is null)
				{
					// The workspace is not active, and is not visible.
					_ctx.Butler.Activate(workspace);
				}

				FocusWindow(window);
			}
		);

	/// <summary>
	/// Focuses the given <paramref name="window"/>. If the window is minimized, it will be restored.
	/// </summary>
	/// <param name="window"></param>
	private static void FocusWindow(IWindow window)
	{
		window.Focus();
		if (window.IsMinimized)
		{
			window.Restore();
		}
	}
}
