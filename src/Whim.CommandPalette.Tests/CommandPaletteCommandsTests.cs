using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteCommandsTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<IWorkspace> Workspace { get; }
		public Mock<IWorkspace> OtherWorkspace { get; }
		public Mock<ICommandPalettePlugin> Plugin { get; }
		public Mock<IWindow>[] Windows { get; }
		public IWindow[] WindowsArray => Windows.Select(w => w.Object).ToArray();

		public MocksBuilder()
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

			Windows = new Mock<IWindow>[3];
			Windows[0] = new();
			Windows[0].SetupGet(w => w.Title).Returns("Window 0");
			Windows[1] = new();
			Windows[1].SetupGet(w => w.Title).Returns("Window 1");
			Windows[2] = new();
			Windows[2].SetupGet(w => w.Title).Returns("Window 2");

			Workspace.SetupGet(w => w.Windows).Returns(new List<IWindow>() { Windows[0].Object, Windows[1].Object });
			OtherWorkspace.SetupGet(w => w.Windows).Returns(new List<IWindow>() { Windows[2].Object });
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
	public void ToggleCommandPaletteCommand()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		// When
		commands.ToggleCommandPaletteCommand.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.Activate(null), Times.Once);
	}

	[Fact]
	public void RenameWorkspaceCommand()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		List<FreeTextVariantConfig> configs = VerifyFreeTextActivated(mocks.Plugin);
		commands.RenameWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was activated.
		mocks.Plugin.Verify(x => x.Activate(It.IsAny<FreeTextVariantConfig>()), Times.Once);

		// Call the callback.
		configs[0].Callback("New workspace name");

		// Verify that the workspace name was changed.
		mocks.Context.VerifySet(x => x.WorkspaceManager.ActiveWorkspace.Name = "New workspace name", Times.Once);
	}

	[Fact]
	public void CreateWorkspaceCommand()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		// Set up the workspace factory.
		Mock<IWorkspace> newWorkspace = new();
		mocks.WorkspaceManager
			.SetupGet(x => x.WorkspaceFactory)
			.Returns(
				(_, name) =>
				{
					newWorkspace.SetupGet(w => w.Name).Returns(name);
					return newWorkspace.Object;
				}
			);

		List<FreeTextVariantConfig> configs = VerifyFreeTextActivated(mocks.Plugin);
		commands.CreateWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was activated.
		mocks.Plugin.Verify(x => x.Activate(It.IsAny<FreeTextVariantConfig>()), Times.Once);

		// Call the callback.
		configs[0].Callback("New workspace name");

		// Verify that the workspace was created.
		mocks.Context.Verify(x => x.WorkspaceManager.Add(newWorkspace.Object), Times.Once);

		// Verify the workspace name.
		Assert.Equal("New workspace name", newWorkspace.Object.Name);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommand()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		List<FreeTextVariantConfig> freeTextConfigs = VerifyFreeTextActivated(mocks.Plugin);
		List<MenuVariantConfig> menuConfigs = VerifyMenuActivated(mocks.Plugin);

		// When
		commands.MoveWindowToWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was menu activated.
		mocks.Plugin.Verify(x => x.Activate(It.IsAny<MenuVariantConfig>()), Times.Once);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommandCreator()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		// When
		CommandItem item = commands.MoveWindowToWorkspaceCommandCreator(mocks.Workspace.Object);
		item.Command.TryExecute();

		// Verify that MoveWindowToWorkspace was called with the workspace.
		mocks.WorkspaceManager.Verify(x => x.MoveWindowToWorkspace(mocks.Workspace.Object, null), Times.Once);
	}

	[Fact]
	public void CreateMoveWindowsToWorkspaceOptions()
	{
		// Given
		MocksBuilder mocks = new();

		// When
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
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
		MocksBuilder mocks = new();

		// When
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.MoveMultipleWindowsToWorkspaceCreator(mocks.WindowsArray, mocks.Workspace.Object);

		// Then
		item.Command.TryExecute();
		mocks.WorkspaceManager.Verify(
			x => x.MoveWindowToWorkspace(mocks.Workspace.Object, mocks.Windows[0].Object),
			Times.Once
		);
		mocks.WorkspaceManager.Verify(
			x => x.MoveWindowToWorkspace(mocks.Workspace.Object, mocks.Windows[1].Object),
			Times.Once
		);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspaceCallback()
	{
		// Given
		MocksBuilder mocks = new();
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
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		commands.MoveMultipleWindowsToWorkspaceCallback(options);

		// Then
		string[] expectedWorkspaces = new string[] { "Workspace", "Other workspace" };
		mocks.Plugin.Verify(
			x =>
				x.Activate(
					It.Is<MenuVariantConfig>(
						c =>
							c.Hint == "Select workspace"
							&& c.Commands.Select(y => y.Command.Title).SequenceEqual(expectedWorkspaces)
					)
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveMultipleWindowsToWorkspace()
	{
		// Given
		MocksBuilder mocks = new();

		// When
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.MoveMultipleWindowsToWorkspace;

		// Then
		item.Command.TryExecute();
		mocks.Plugin.Verify(
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
	public void GetEnumerator()
	{
		// Given
		MocksBuilder mocks = new();
		CommandPaletteCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		// Then
		Assert.Equal(5, commands.Count());
	}
}
