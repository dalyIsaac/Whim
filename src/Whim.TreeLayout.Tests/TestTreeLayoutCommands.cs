using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutCommands
{
	private static Mock<ITreeLayoutPlugin> CreateCommands()
	{
		Mock<ITreeLayoutPlugin> plugin = new();
		return plugin;
	}

	[Fact]
	public void TestAddTreeDirectionLeftCommand()
	{
		Mock<ITreeLayoutPlugin> plugin = CreateCommands();
		TreeLayoutCommands commands = new(plugin.Object);
		CommandItem item = commands.AddTreeDirectionLeftCommand;

		item.Command.TryExecute();

		plugin.Verify(x => x.SetAddWindowDirection(Direction.Left), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionRightCommand()
	{
		Mock<ITreeLayoutPlugin> plugin = CreateCommands();
		TreeLayoutCommands commands = new(plugin.Object);
		CommandItem item = commands.AddTreeDirectionRightCommand;

		item.Command.TryExecute();

		plugin.Verify(x => x.SetAddWindowDirection(Direction.Right), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionUpCommand()
	{
		Mock<ITreeLayoutPlugin> plugin = CreateCommands();
		TreeLayoutCommands commands = new(plugin.Object);
		CommandItem item = commands.AddTreeDirectionUpCommand;

		item.Command.TryExecute();

		plugin.Verify(x => x.SetAddWindowDirection(Direction.Up), Times.Once);
	}

	[Fact]
	public void TestAddTreeDirectionDownCommand()
	{
		Mock<ITreeLayoutPlugin> plugin = CreateCommands();
		TreeLayoutCommands commands = new(plugin.Object);
		CommandItem item = commands.AddTreeDirectionDownCommand;

		item.Command.TryExecute();

		plugin.Verify(x => x.SetAddWindowDirection(Direction.Down), Times.Once);
	}

	[Fact]
	public void TestSplitFocusedWindowCommand()
	{
		Mock<ITreeLayoutPlugin> plugin = CreateCommands();
		Mock<ITreeLayoutEngine> engine = new();
		plugin.Setup(x => x.GetTreeLayoutEngine()).Returns(engine.Object);

		TreeLayoutCommands commands = new(plugin.Object);
		CommandItem item = commands.SplitFocusedWindowCommand;

		item.Command.TryExecute();

		plugin.Verify(x => x.GetTreeLayoutEngine(), Times.Once);
		engine.Verify(x => x.SplitFocusedWindow(), Times.Once);
	}
}
