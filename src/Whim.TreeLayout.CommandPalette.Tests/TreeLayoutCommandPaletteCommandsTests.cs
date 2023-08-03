using Moq;
using Whim.CommandPalette;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.CommandPalette.Tests;

public class TreeLayoutCommandPaletteCommandsTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IPlugin> TreeLayoutCommandPalettePlugin { get; } = new();
		public Mock<ITreeLayoutPlugin> TreeLayoutPlugin { get; } = new();
		public Mock<ICommandPalettePlugin> CommandPalettePlugin { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public TreeLayoutCommandPalettePluginCommands Commands { get; }

		public Wrapper()
		{
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);
			MonitorManager.Setup(m => m.ActiveMonitor).Returns(Monitor.Object);

			TreeLayoutCommandPalettePlugin.Setup(t => t.Name).Returns("whim.tree_layout.command_palette");

			TreeLayoutCommandPalettePluginCommands commands =
				new(
					Context.Object,
					TreeLayoutCommandPalettePlugin.Object,
					TreeLayoutPlugin.Object,
					CommandPalettePlugin.Object
				);

			Commands = new(
				Context.Object,
				TreeLayoutCommandPalettePlugin.Object,
				TreeLayoutPlugin.Object,
				CommandPalettePlugin.Object
			);
		}
	}

	[Fact]
	public void SetDirectionCommand_Activates()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.tree_layout.command_palette.set_direction"
		);
		wrapper.TreeLayoutPlugin.Setup(t => t.GetAddWindowDirection(wrapper.Monitor.Object)).Returns(Direction.Left);

		// When
		command.TryExecute();

		// Then
		wrapper.CommandPalettePlugin.Verify(c => c.Activate(It.IsAny<MenuVariantConfig>()), Times.Once);
	}

	[Fact]
	public void SetDirectionCommand_Activates_Fails()
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(
			"whim.tree_layout.command_palette.set_direction"
		);
		wrapper.TreeLayoutPlugin.Setup(t => t.GetAddWindowDirection(wrapper.Monitor.Object)).Returns((Direction?)null);

		// When
		command.TryExecute();

		// Then
		wrapper.CommandPalettePlugin.Verify(c => c.Activate(It.IsAny<MenuVariantConfig>()), Times.Never);
	}

	[Fact]
	public void CreateSetDirectionCommandItems()
	{
		// Given
		Wrapper wrapper = new();

		// When
		ICommand[] commandItems = wrapper.Commands.CreateSetDirectionCommands();

		// Then
		Assert.Equal(4, commandItems.Length);
	}

	[InlineData("Left", Direction.Left)]
	[InlineData("Right", Direction.Right)]
	[InlineData("Up", Direction.Up)]
	[InlineData("Down", Direction.Down)]
	[Theory]
	public void SetDirection(string direction, Direction expectedDirection)
	{
		// Given
		Wrapper wrapper = new();

		// When
		wrapper.Commands.SetDirection(direction);

		// Then
		wrapper.TreeLayoutPlugin.Verify(
			t => t.SetAddWindowDirection(wrapper.Monitor.Object, expectedDirection),
			Times.Once
		);
	}

	[Fact]
	public void SetDirectionCommand_FailsWhenNoActiveTreeLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();

		// When
		wrapper.Commands.SetDirection("welp");

		// Then
		wrapper.TreeLayoutPlugin.Verify(
			t => t.SetAddWindowDirection(It.IsAny<IMonitor>(), It.IsAny<Direction>()),
			Times.Never
		);
	}
}
