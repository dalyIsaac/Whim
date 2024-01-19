using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteCommandsTests
{
	private class Wrapper
	{
		public IContext Context { get; }
		public IWorkspace Workspace { get; }
		public IWorkspace OtherWorkspace { get; }
		public ICommandPalettePlugin Plugin { get; }
		public IWindow[] Windows { get; }
		public CommandPaletteCommands Commands { get; }

		public Wrapper()
		{
			Context = Substitute.For<IContext>();

			Workspace = Substitute.For<IWorkspace>();
			Workspace.Name.Returns("Workspace");

			OtherWorkspace = Substitute.For<IWorkspace>();
			OtherWorkspace.Name.Returns("Other workspace");

			Context.WorkspaceManager.ActiveWorkspace.Returns(Workspace);
			Context
				.WorkspaceManager.GetEnumerator()
				.Returns((_) => new List<IWorkspace>() { Workspace, OtherWorkspace }.GetEnumerator());

			Plugin = Substitute.For<ICommandPalettePlugin>();
			Plugin.Name.Returns("whim.command_palette");

			Windows = new IWindow[3];
			Windows[0] = Substitute.For<IWindow>();
			Windows[0].Title.Returns("Window 0");
			Windows[1] = Substitute.For<IWindow>();
			Windows[1].Title.Returns("Window 1");
			Windows[2] = Substitute.For<IWindow>();
			Windows[2].Title.Returns("Window 2");

			Workspace.Windows.Returns((_) => new List<IWindow>() { Windows[0], Windows[1] });
			OtherWorkspace.Windows.Returns((_) => new List<IWindow>() { Windows[2] });

			Commands = new(Context, Plugin);
		}
	}

	[Fact]
	public void ActivateWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.activate_workspace"
		);

		MenuVariantConfig? config = null;
		wrapper.Plugin.Activate(Arg.Do<MenuVariantConfig>(c => config = c));

		command.TryExecute();

		// Verify that the plugin was activated.
		wrapper.Plugin.Received(1).Activate(Arg.Any<MenuVariantConfig>());

		// Call the callback.
		ICommand activeWorkspaceCommand = config!.Commands.First();
		activeWorkspaceCommand.TryExecute();

		// Verify that the workspace was activated.
		wrapper.Context.WorkspaceManager.Received(1).Activate(wrapper.OtherWorkspace, null);
	}

	[Fact]
	public void ToggleCommandPalette()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.toggle"
		);

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Received(1).Toggle();
	}

	[Fact]
	public void RenameWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.rename_workspace"
		);

		FreeTextVariantConfig? config = null;
		wrapper.Plugin.Activate(Arg.Do<FreeTextVariantConfig>(c => config = c));

		command.TryExecute();

		// Verify that the plugin was activated
		wrapper.Plugin.Received(1).Activate(Arg.Any<FreeTextVariantConfig>());

		// Call the callback
		config!.Callback("New workspace name");

		// Verify that the workspace was renamed
		wrapper.Context.WorkspaceManager.ActiveWorkspace.Received(1).Name = "New workspace name";
	}

	[Fact]
	public void CreateWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.create_workspace"
		);

		FreeTextVariantConfig? config = null;
		wrapper.Plugin.Activate(Arg.Do<FreeTextVariantConfig>(c => config = c));

		command.TryExecute();

		// Verify that the plugin was activated.
		wrapper.Plugin.Activate(Arg.Any<FreeTextVariantConfig>());

		// Call the callback.
		config!.Callback("New workspace name");

		// Verify that the workspace was created.
		wrapper.Context.WorkspaceManager.Received(1).Add("New workspace name", null);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommand()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.move_window_to_workspace"
		);

		// When
		command.TryExecute();

		// Verify that the plugin was menu activated.
		wrapper.Plugin.Received(1).Activate(Arg.Any<MenuVariantConfig>());
	}

	[Fact]
	public void MoveWindowToWorkspaceCommandCreator()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);

		// When
		ICommand command = commands.MoveWindowToWorkspaceCommandCreator(wrapper.Workspace);
		command.TryExecute();

		// Verify that MoveWindowToWorkspace was called with the workspace.
		wrapper.Context.WorkspaceManager.Received(1).MoveWindowToWorkspace(wrapper.Workspace, null);
	}

	[Fact]
	public void CreateMoveWindowsToWorkspaceOptions()
	{
		// Given
		Wrapper wrapper = new();

		// When
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
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
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
		ICommand command = commands.MoveMultipleWindowsToWorkspaceCreator(wrapper.Windows, wrapper.Workspace);

		// Then
		command.TryExecute();
		wrapper.Context.WorkspaceManager.Received(1).MoveWindowToWorkspace(wrapper.Workspace, wrapper.Windows[0]);
		wrapper.Context.WorkspaceManager.Received(1).MoveWindowToWorkspace(wrapper.Workspace, wrapper.Windows[1]);
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
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);
		commands.MoveMultipleWindowsToWorkspaceCallback(options);

		// Then
		string[] expectedWorkspaces = new string[] { "Workspace", "Other workspace" };
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<MenuVariantConfig>(c =>
					c.Hint == "Select workspace" && c.Commands.Select(c => c.Title).SequenceEqual(expectedWorkspaces)
				)
			);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.move_multiple_windows_to_workspace"
		);

		// When
		command.TryExecute();

		// Then
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<SelectVariantConfig>(c =>
					c.Hint == "Select windows"
					&& c.Options.Select(y => y.Title).SequenceEqual(new string[] { "Window 0", "Window 1", "Window 2" })
				)
			);
	}

	[Fact]
	public void RemoveWindow()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.remove_window"
		);

		// When
		command.TryExecute();

		// Then
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<MenuVariantConfig>(c =>
					c.Hint == "Select window"
					&& c.ConfirmButtonText == "Remove"
					&& c.Commands.Select(c => c.Title).SequenceEqual(new string[] { "Window 0", "Window 1" })
				)
			);
	}

	[Fact]
	public void RemoveWindowCommandCreator()
	{
		// Given
		Wrapper wrapper = new();
		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);

		// When
		ICommand command = commands.RemoveWindowCommandCreator(wrapper.Windows[0]);
		command.TryExecute();

		// Then
		wrapper.Workspace.Received(1).RemoveWindow(wrapper.Windows[0]);
		wrapper.Workspace.Received(1).DoLayout();
	}

	[Fact]
	public void FindFocusWindow()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new TestUtils.PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.command_palette.find_focus_window"
		);

		// When
		command.TryExecute();

		// Then
		wrapper
			.Plugin.Received(1)
			.Activate(
				Arg.Is<MenuVariantConfig>(c =>
					c.Hint == "Find window"
					&& c.ConfirmButtonText == "Focus"
					&& c.Commands.Select(c => c.Title)
						.SequenceEqual(new string[] { "Window 0", "Window 1", "Window 2" })
				)
			);
	}

	[Fact]
	public void FocusWindowCommandCreator_CannotFindWorkspace()
	{
		// Given
		Wrapper wrapper = new();

		IWindow window = wrapper.Windows[0];
		wrapper.Context.Butler.GetWorkspaceForWindow(window).Returns((IWorkspace?)null);

		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);

		// When
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then
		wrapper.Workspace.DidNotReceive().DoLayout();
		window.DidNotReceive().Focus();
	}

	[Fact]
	public void FocusWindowCommandCreator_WindowIsMinimized()
	{
		// Given
		Wrapper wrapper = new();

		IWindow window = wrapper.Windows[0];
		wrapper.Context.Butler.GetWorkspaceForWindow(window).Returns(wrapper.Workspace);

		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);

		// When
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then
		wrapper.Workspace.Received(1).MinimizeWindowEnd(window);
		wrapper.Context.Butler.Received(1).Activate(Arg.Any<IWorkspace>());
		wrapper.Workspace.Received(1).DoLayout();
		window.Received(1).Focus();
	}

	[Fact]
	public void FocusWindowCommandCreator_WindowIsNotMinimized()
	{
		// Given
		Wrapper wrapper = new();

		IWindow window = wrapper.Windows[0];
		wrapper.Context.Butler.GetWorkspaceForWindow(window).Returns(wrapper.Workspace);
		window.IsMinimized.Returns(false);

		CommandPaletteCommands commands = new(wrapper.Context, wrapper.Plugin);

		// When
		ICommand command = commands.FocusWindowCommandCreator(window);
		command.TryExecute();

		// Then
		wrapper.Workspace.DidNotReceive().MinimizeWindowEnd(window);
		wrapper.Context.Butler.Received(1).Activate(Arg.Any<IWorkspace>());
		wrapper.Workspace.Received(1).DoLayout();
		window.Received(1).Focus();
	}
}
