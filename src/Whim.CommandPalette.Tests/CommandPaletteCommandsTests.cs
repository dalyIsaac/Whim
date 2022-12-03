using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPaletteCommandsTests
{
	private static (
		Mock<IConfigContext>,
		Mock<IWorkspaceManager>,
		Mock<IWorkspace>,
		Mock<ICommandPalettePlugin>
	) CreateMocks()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IWorkspace> workspace = new();

		configContext.SetupGet(x => x.WorkspaceManager).Returns(workspaceManager.Object);
		workspaceManager.SetupGet(w => w.ActiveWorkspace).Returns(workspace.Object);

		Mock<ICommandPalettePlugin> plugin = new();

		return (configContext, workspaceManager, workspace, plugin);
	}

	private static List<CommandPaletteFreeTextActivationConfig> VerifyFreeTextActivated(
		Mock<ICommandPalettePlugin> plugin
	)
	{
		List<CommandPaletteFreeTextActivationConfig> configs = new();
		plugin.Setup(p => p.ActivateWithConfig(Capture.In(configs), It.IsAny<IEnumerable<CommandItem>?>()));
		return configs;
	}

	private static List<CommandPaletteMenuActivationConfig> VerifyMenuActivated(Mock<ICommandPalettePlugin> plugin)
	{
		List<CommandPaletteMenuActivationConfig> configs = new();
		plugin.Setup(p => p.ActivateWithConfig(Capture.In(configs), It.IsAny<IEnumerable<CommandItem>?>()));
		return configs;
	}

	[Fact]
	public void ToggleCommandPaletteCommand()
	{
		(Mock<IConfigContext> configContext, _, _, Mock<ICommandPalettePlugin> plugin) = CreateMocks();
		CommandPaletteCommands commands = new(configContext.Object, plugin.Object);

		commands.ToggleCommandPaletteCommand.Command.TryExecute();
		plugin.Verify(x => x.Activate(), Times.Once);
	}

	[Fact]
	public void RenameWorkspaceCommand()
	{
		(Mock<IConfigContext> configContext, _, _, Mock<ICommandPalettePlugin> plugin) = CreateMocks();
		CommandPaletteCommands commands = new(configContext.Object, plugin.Object);

		List<CommandPaletteFreeTextActivationConfig> configs = VerifyFreeTextActivated(plugin);
		commands.RenameWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was activated.
		plugin.Verify(
			x =>
				x.ActivateWithConfig(
					It.IsAny<CommandPaletteFreeTextActivationConfig>(),
					It.IsAny<IEnumerable<CommandItem>?>()
				),
			Times.Once
		);

		// Call the callback.
		configs[0].Callback("New workspace name");

		// Verify that the workspace name was changed.
		configContext.VerifySet(x => x.WorkspaceManager.ActiveWorkspace.Name = "New workspace name", Times.Once);
	}

	[Fact]
	public void CreateWorkspaceCommand()
	{
		(
			Mock<IConfigContext> configContext,
			Mock<IWorkspaceManager> workspaceManager,
			_,
			Mock<ICommandPalettePlugin> plugin
		) = CreateMocks();
		CommandPaletteCommands commands = new(configContext.Object, plugin.Object);

		// Set up the workspace factory.
		Mock<IWorkspace> newWorkspace = new();
		workspaceManager
			.SetupGet(x => x.WorkspaceFactory)
			.Returns(
				(_, name) =>
				{
					newWorkspace.SetupGet(w => w.Name).Returns(name);
					return newWorkspace.Object;
				}
			);

		List<CommandPaletteFreeTextActivationConfig> configs = VerifyFreeTextActivated(plugin);
		commands.CreateWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was activated.
		plugin.Verify(
			x =>
				x.ActivateWithConfig(
					It.IsAny<CommandPaletteFreeTextActivationConfig>(),
					It.IsAny<IEnumerable<CommandItem>?>()
				),
			Times.Once
		);

		// Call the callback.
		configs[0].Callback("New workspace name");

		// Verify that the workspace was created.
		configContext.Verify(x => x.WorkspaceManager.Add(newWorkspace.Object), Times.Once);

		// Verify the workspace name.
		Assert.Equal("New workspace name", newWorkspace.Object.Name);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommand()
	{
		(
			Mock<IConfigContext> configContext,
			Mock<IWorkspaceManager> workspaceManager,
			_,
			Mock<ICommandPalettePlugin> plugin
		) = CreateMocks();
		CommandPaletteCommands commands = new(configContext.Object, plugin.Object);

		List<CommandPaletteFreeTextActivationConfig> freeTextConfigs = VerifyFreeTextActivated(plugin);
		List<CommandPaletteMenuActivationConfig> menuConfigs = VerifyMenuActivated(plugin);

		commands.MoveWindowToWorkspaceCommand.Command.TryExecute();

		// Verify that the plugin was menu activated.
		plugin.Verify(
			x =>
				x.ActivateWithConfig(
					It.IsAny<CommandPaletteMenuActivationConfig>(),
					It.IsAny<IEnumerable<CommandItem>?>()
				),
			Times.Once
		);
	}

	[Fact]
	public void MoveWindowToWorkspaceCommandCreator()
	{
		(
			Mock<IConfigContext> configContext,
			Mock<IWorkspaceManager> workspaceManager,
			Mock<IWorkspace> workspace,
			Mock<ICommandPalettePlugin> plugin
		) = CreateMocks();
		CommandPaletteCommands commands = new(configContext.Object, plugin.Object);

		CommandItem item = commands.MoveWindowToWorkspaceCommandCreator(workspace.Object);
		item.Command.TryExecute();

		// Verify that MoveWindowToWorkspace was called with the workspace.
		workspaceManager.Verify(x => x.MoveWindowToWorkspace(workspace.Object, null), Times.Once);
	}
}
