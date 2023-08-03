using FluentAssertions;
using Moq;
using Whim.TestUtils;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteCommandsTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<IWorkspace> Workspace { get; }
		public Mock<IWorkspace> OtherWorkspace { get; }
		public Mock<ICommandPalettePlugin> Plugin { get; }
		public Mock<IWindow>[] Windows { get; }
		public IWindow[] WindowsArray => Windows.Select(w => w.Object).ToArray();
		public CommandPaletteCommands Commands { get; }

		public Wrapper()
		{
			Context = new();
			WorkspaceManager = new();
			Workspace = new();
			Workspace.SetupGet(w => w.Name).Returns("Workspace");
			OtherWorkspace = new();
			OtherWorkspace.SetupGet(w => w.Name).Returns("Other workspace");

			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager.SetupGet(w => w.ActiveWorkspace).Returns(Workspace.Object);
			WorkspaceManager
				.Setup(w => w.GetEnumerator())
				.Returns(new List<IWorkspace>() { Workspace.Object, OtherWorkspace.Object }.GetEnumerator());

			Plugin = new();
			Plugin.SetupGet(p => p.Name).Returns("whim.command_palette");

			Windows = new Mock<IWindow>[3];
			Windows[0] = new();
			Windows[0].SetupGet(w => w.Title).Returns("Window 0");
			Windows[1] = new();
			Windows[1].SetupGet(w => w.Title).Returns("Window 1");
			Windows[2] = new();
			Windows[2].SetupGet(w => w.Title).Returns("Window 2");

			Workspace.SetupGet(w => w.Windows).Returns(new List<IWindow>() { Windows[0].Object, Windows[1].Object });
			OtherWorkspace.SetupGet(w => w.Windows).Returns(new List<IWindow>() { Windows[2].Object });

			Commands = new(Context.Object, Plugin.Object);
		}
	}

	private static List<FreeTextVariantConfig> VerifyFreeTextActivated(Mock<ICommandPalettePlugin> plugin)
	{
		List<FreeTextVariantConfig> configs = new();
		plugin.Setup(p => p.Activate(Capture.In(configs)));
		return configs;
	}

	private static List<MenuVariantConfig> VerifyMenuActivated(Mock<ICommandPalettePlugin> plugin)
	{
		List<MenuVariantConfig> configs = new();
		plugin.Setup(p => p.Activate(Capture.In(configs)));
		return configs;
	}

	[Fact]
	public void ActivateWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.activate_workspace"
		);

		List<MenuVariantConfig> configs = VerifyMenuActivated(wrapper.Plugin);
		command.TryExecute();

		// Verify that the plugin was activated.
		wrapper.Plugin.Verify(p => p.Activate(It.IsAny<MenuVariantConfig>()), Times.Once);

		// Call the callback.
		ICommand activeWorkspaceCommand = configs[0].Commands.First();
		activeWorkspaceCommand.TryExecute();

		// Verify that the workspace was activated.
		wrapper.WorkspaceManager.Verify(w => w.Activate(wrapper.OtherWorkspace.Object, null), Times.Once);
	}

	[Fact]
	public void ToggleCommandPalette()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand("whim.command_palette.toggle");

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.Toggle(), Times.Once);
	}

	[Fact]
	public void RenameWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.rename_workspace"
		);

		List<FreeTextVariantConfig> configs = VerifyFreeTextActivated(wrapper.Plugin);
		command.TryExecute();

		// Verify that the plugin was activated
		wrapper.Plugin.Verify(p => p.Activate(It.IsAny<FreeTextVariantConfig>()), Times.Once);

		// Call the callback
		configs[0].Callback("New workspace name");

		// Verify that the workspace was renamed
		wrapper.WorkspaceManager.VerifySet(w => w.ActiveWorkspace.Name = "New workspace name", Times.Once);
	}

	[Fact]
	public void CreateWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.create_workspace"
		);

		List<FreeTextVariantConfig> configs = VerifyFreeTextActivated(wrapper.Plugin);
		command.TryExecute();

		// Verify that the plugin was activated.
		wrapper.Plugin.Verify(x => x.Activate(It.IsAny<FreeTextVariantConfig>()), Times.Once);

		// Call the callback.
		configs[0].Callback("New workspace name");

		// Verify that the workspace was created.
		wrapper.Context.Verify(x => x.WorkspaceManager.Add("New workspace name", null), Times.Once);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommand()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.move_window_to_workspace"
		);
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);

		List<FreeTextVariantConfig> freeTextConfigs = VerifyFreeTextActivated(wrapper.Plugin);
		List<MenuVariantConfig> menuConfigs = VerifyMenuActivated(wrapper.Plugin);

		// When
		command.TryExecute();

		// Verify that the plugin was menu activated.
		wrapper.Plugin.Verify(x => x.Activate(It.IsAny<MenuVariantConfig>()), Times.Once);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommandCreator()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		ICommand command = commands.MoveWindowToWorkspaceCommandCreator(wrapper.Workspace.Object);
		command.TryExecute();

		// Verify that MoveWindowToWorkspace was called with the workspace.
		wrapper.WorkspaceManager.Verify(x => x.MoveWindowToWorkspace(wrapper.Workspace.Object, null), Times.Once);
	}

	[Fact]
	public void CreateMoveWindowsToWorkspaceOptions()
	{
		// Given
		Wrapper wrapper = new();

		// When
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);
		SelectOption[] options = commands.CreateMoveWindowsToWorkspaceOptions();

		// Then
		Assert.Equal(3, options.Length);
		Assert.Equal("Window 0", options[0].Title);
		Assert.Equal("Window 1", options[1].Title);
		Assert.Equal("Window 2", options[2].Title);
		options.Should().OnlyContain(x => x.IsEnabled);
		options.Should().OnlyContain(x => !x.IsSelected);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspaceCreator()
	{
		// Given
		Wrapper wrapper = new();

		// When
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);
		ICommand command = commands.MoveMultipleWindowsToWorkspaceCreator(
			wrapper.WindowsArray,
			wrapper.Workspace.Object
		);

		// Then
		command.TryExecute();
		wrapper.WorkspaceManager.Verify(
			x => x.MoveWindowToWorkspace(wrapper.Workspace.Object, wrapper.Windows[0].Object),
			Times.Once
		);
		wrapper.WorkspaceManager.Verify(
			x => x.MoveWindowToWorkspace(wrapper.Workspace.Object, wrapper.Windows[1].Object),
			Times.Once
		);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspaceCallback()
	{
		// Given
		Wrapper wrapper = new();
		List<SelectOption> options =
			new()
			{
				new SelectOption()
				{
					Id = "0",
					Title = "Window 0",
					IsSelected = true
				},
				new SelectOption()
				{
					Id = "1",
					Title = "Window 1",
					IsSelected = false
				},
				new SelectOption()
				{
					Id = "2",
					Title = "Window 2",
					IsSelected = true
				},
			};

		// When
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);
		commands.MoveMultipleWindowsToWorkspaceCallback(options);

		// Then
		string[] expectedWorkspaces = new string[] { "Workspace", "Other workspace" };
		wrapper.Plugin.Verify(
			x =>
				x.Activate(
					It.Is<MenuVariantConfig>(
						c =>
							c.Hint == "Select workspace"
							&& c.Commands.Select(c => c.Title).SequenceEqual(expectedWorkspaces)
					)
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.move_multiple_windows_to_workspace"
		);

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Verify(
			x =>
				x.Activate(
					It.Is<SelectVariantConfig>(
						c =>
							c.Hint == "Select windows"
							&& c.Options
								.Select(y => y.Title)
								.SequenceEqual(new string[] { "Window 0", "Window 1", "Window 2" })
					)
				),
			Times.Once
		);
	}

	[Fact]
	public void RemoveWindow()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.remove_window"
		);

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Verify(
			x =>
				x.Activate(
					It.Is<MenuVariantConfig>(
						c =>
							c.Hint == "Select window"
							&& c.ConfirmButtonText == "Remove"
							&& c.Commands
								.Select(c => c.Title)
								.SequenceEqual(new string[] { "Remove \"Window 0\"", "Remove \"Window 1\"" })
					)
				),
			Times.Once
		);
	}

	[Fact]
	public void RemoveWindowCommandCreator()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteCommands commands = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		ICommand command = commands.RemoveWindowCommandCreator(wrapper.Windows[0].Object);
		command.TryExecute();

		// Then
		wrapper.Workspace.Verify(x => x.RemoveWindow(wrapper.Windows[0].Object), Times.Once);
	}
}
