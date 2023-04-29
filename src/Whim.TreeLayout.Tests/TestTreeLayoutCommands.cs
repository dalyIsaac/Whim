using Moq;
using Whim.TestUtilities;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutCommands
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<ITreeLayoutPlugin> Plugin { get; } = new();

		public MocksWrapper()
		{
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);

			MonitorManager.Setup(x => x.FocusedMonitor).Returns(Monitor.Object);
		}
	}

	[Fact]
	public void TestAddTreeDirectionLeftCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand item = testUtils.GetCommand("whim.tree_layout.add_tree_direction_left");

		// When
		item.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Left), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionRightCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand item = testUtils.GetCommand("whim.tree_layout.add_tree_direction_right");

		// When
		item.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Right), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionUpCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand item = testUtils.GetCommand("whim.tree_layout.add_tree_direction_up");

		// When
		item.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Up), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionDownCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand item = testUtils.GetCommand("whim.tree_layout.add_tree_direction_down");

		// When
		item.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Down), Times.Once);
	}

	[Fact]
	public void TestSplitFocusedWindowCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand item = testUtils.GetCommand("whim.tree_layout.split_focused_window");

		// When
		item.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SplitFocusedWindow(), Times.Once);
	}
}
