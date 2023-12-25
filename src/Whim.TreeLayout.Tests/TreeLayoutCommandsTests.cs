using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutCommandsTests
{
	private sealed class Wrapper
	{
		public IContext Context { get; } = Substitute.For<IContext>();
		public IMonitor Monitor { get; } = Substitute.For<IMonitor>();
		public ITreeLayoutPlugin Plugin { get; } = Substitute.For<ITreeLayoutPlugin>();
		public TreeLayoutCommands Commands { get; }

		public Wrapper()
		{
			Context.MonitorManager.ActiveMonitor.Returns(Monitor);
			Plugin.Name.Returns("whim.tree_layout");
			Commands = new TreeLayoutCommands(Context, Plugin);
		}
	}

	[InlineData("whim.tree_layout.add_tree_direction_left", Direction.Left)]
	[InlineData("whim.tree_layout.add_tree_direction_right", Direction.Right)]
	[InlineData("whim.tree_layout.add_tree_direction_up", Direction.Up)]
	[InlineData("whim.tree_layout.add_tree_direction_down", Direction.Down)]
	[Theory]
	public void SetAddWindowDirection(string commandName, Direction direction)
	{
		// Given
		Wrapper wrapper = new();
		ICommand command = new PluginCommandsTestUtils(wrapper.Commands).GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		wrapper.Plugin.Received().SetAddWindowDirection(wrapper.Monitor, direction);
	}
}
