using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TreeLayoutCommandsTests
{
	[InlineAutoSubstituteData<TreeCustomization>("whim.tree_layout.add_tree_direction_left", Direction.Left)]
	[InlineAutoSubstituteData<TreeCustomization>("whim.tree_layout.add_tree_direction_right", Direction.Right)]
	[InlineAutoSubstituteData<TreeCustomization>("whim.tree_layout.add_tree_direction_up", Direction.Up)]
	[InlineAutoSubstituteData<TreeCustomization>("whim.tree_layout.add_tree_direction_down", Direction.Down)]
	[Theory]
	public void SetAddWindowDirection(
		string commandName,
		Direction direction,
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutCommands commands = new(ctx, plugin);
		ICommand command = new PluginCommandsTestUtils(commands).GetCommand(commandName);

		// When
		command.TryExecute();

		// Then
		plugin.Received().SetAddWindowDirection(monitor, direction);
	}
}
