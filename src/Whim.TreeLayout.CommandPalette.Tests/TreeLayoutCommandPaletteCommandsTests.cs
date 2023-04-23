using Moq;
using Whim.CommandPalette;
using Xunit;

namespace Whim.TreeLayout.CommandPalette.Tests;

public class TreeLayoutCommandPaletteCommandsTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IPlugin> TreeLayoutCommandPalettePlugin { get; } = new();
		public Mock<ITreeLayoutPlugin> TreeLayoutPlugin { get; } = new();
		public Mock<ICommandPalettePlugin> CommandPalettePlugin { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();

		public MocksBuilder()
		{
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);
			MonitorManager.Setup(m => m.FocusedMonitor).Returns(Monitor.Object);
		}
	}

	[Fact]
	public void SetDirectionCommand_Activates()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePluginCommands commands =
			new(
				mocks.Context.Object,
				mocks.TreeLayoutCommandPalettePlugin.Object,
				mocks.TreeLayoutPlugin.Object,
				mocks.CommandPalettePlugin.Object
			);

		mocks.TreeLayoutPlugin.Setup(t => t.GetAddWindowDirection(mocks.Monitor.Object)).Returns(Direction.Left);

		// When
		commands.SetDirectionCommand.Command.TryExecute();

		// Then
		mocks.CommandPalettePlugin.Verify(c => c.Activate(It.IsAny<MenuVariantConfig>()), Times.Once);
	}

	[Fact]
	public void SetDirectionCommand_Activates_Fails()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePluginCommands commands =
			new(
				mocks.Context.Object,
				mocks.TreeLayoutCommandPalettePlugin.Object,
				mocks.TreeLayoutPlugin.Object,
				mocks.CommandPalettePlugin.Object
			);

		mocks.TreeLayoutPlugin.Setup(t => t.GetAddWindowDirection(mocks.Monitor.Object)).Returns((Direction?)null);

		// When
		commands.SetDirectionCommand.Command.TryExecute();

		// Then
		mocks.CommandPalettePlugin.Verify(c => c.Activate(It.IsAny<MenuVariantConfig>()), Times.Never);
	}

	[Fact]
	public void CreateSetDirectionCommandItems()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePluginCommands commands =
			new(
				mocks.Context.Object,
				mocks.TreeLayoutCommandPalettePlugin.Object,
				mocks.TreeLayoutPlugin.Object,
				mocks.CommandPalettePlugin.Object
			);

		// When
		CommandItem[] commandItems = commands.CreateSetDirectionCommandItems();

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
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePluginCommands commands =
			new(
				mocks.Context.Object,
				mocks.TreeLayoutCommandPalettePlugin.Object,
				mocks.TreeLayoutPlugin.Object,
				mocks.CommandPalettePlugin.Object
			);

		// When
		commands.SetDirection(direction);

		// Then
		mocks.TreeLayoutPlugin.Verify(
			t => t.SetAddWindowDirection(mocks.Monitor.Object, expectedDirection),
			Times.Once
		);
	}

	[Fact]
	public void SetDirectionCommand_FailsWhenNoActiveTreeLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePluginCommands commands =
			new(
				mocks.Context.Object,
				mocks.TreeLayoutCommandPalettePlugin.Object,
				mocks.TreeLayoutPlugin.Object,
				mocks.CommandPalettePlugin.Object
			);

		// When
		commands.SetDirection("welp");

		// Then
		mocks.TreeLayoutPlugin.Verify(
			t => t.SetAddWindowDirection(It.IsAny<IMonitor>(), It.IsAny<Direction>()),
			Times.Never
		);
	}
}
