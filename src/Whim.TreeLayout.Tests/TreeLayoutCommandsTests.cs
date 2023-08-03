using Moq;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutCommandsTests
{
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<ITreeLayoutPlugin> Plugin { get; } = new();
		public TreeLayoutCommands Commands { get; }

		public Wrapper()
		{
			Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);
			MonitorManager.Setup(x => x.ActiveMonitor).Returns(Monitor.Object);

			Plugin.Setup(t => t.Name).Returns("whim.tree_layout");

			Commands = new TreeLayoutCommands(Context.Object, Plugin.Object);
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
		wrapper.Plugin.Verify(x => x.SetAddWindowDirection(wrapper.Monitor.Object, direction));
	}
}
