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
		public ICommand Command { get; }

		public MocksWrapper(string id)
		{
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);
			Plugin.SetupGet(p => p.Name).Returns("whim.tree_layout");
			MonitorManager.Setup(x => x.ActiveMonitor).Returns(Monitor.Object);

			Command = new PluginCommandsTestUtils(new TreeLayoutCommands(Context.Object, Plugin.Object)).GetCommand(id);
		}
	}

	[Fact]
	public void TestAddTreeDirectionLeftCommand()
	{
		// Given
		MocksWrapper mocks = new("whim.tree_layout.add_tree_direction_left");

		// When
		mocks.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Left), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionRightCommand()
	{
		// Given
		MocksWrapper mocks = new("whim.tree_layout.add_tree_direction_right");

		// When
		mocks.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Right), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionUpCommand()
	{
		// Given
		MocksWrapper mocks = new("whim.tree_layout.add_tree_direction_up");

		// When
		mocks.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Up), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionDownCommand()
	{
		// Given
		MocksWrapper mocks = new("whim.tree_layout.add_tree_direction_down");

		// When
		mocks.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Down), Times.Once);
	}

	[Fact]
	public void TestSplitFocusedWindowCommand()
	{
		// Given
		MocksWrapper mocks = new("whim.tree_layout.split_focused_window");

		// When
		mocks.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SplitFocusedWindow(), Times.Once);
	}
}
