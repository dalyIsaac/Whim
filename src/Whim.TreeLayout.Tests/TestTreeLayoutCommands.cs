using Moq;
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
		CommandItem item = commands.AddTreeDirectionLeftCommand;

		// When
		item.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Left), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionRightCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.AddTreeDirectionRightCommand;

		// When
		item.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Right), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionUpCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.AddTreeDirectionUpCommand;

		// When
		item.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Up), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionDownCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.AddTreeDirectionDownCommand;

		// When
		item.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, Direction.Down), Times.Once);
	}

	[Fact]
	public void TestSplitFocusedWindowCommand()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);
		CommandItem item = commands.SplitFocusedWindowCommand;

		// When
		item.Command.TryExecute();

		// Then
		mocks.Plugin.Verify(x => x.SplitFocusedWindow(), Times.Once);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutCommands commands = new(mocks.Context.Object, mocks.Plugin.Object);

		// When
		IEnumerable<CommandItem> items = commands;

		// Then
		Assert.Equal(5, items.Count());
	}
}
