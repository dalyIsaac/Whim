using Moq;
using Whim.TestUtilities;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutCommandsTests
{
	[Fact]
	public void ToggleWindowFloatingCommand()
	{
		// Given
		Mock<IFloatingLayoutPlugin> plugin = new();
		FloatingLayoutCommands commands = new(plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.floating_layout.toggle_window_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Verify(p => p.ToggleWindowFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloatingCommand()
	{
		// Given
		Mock<IFloatingLayoutPlugin> plugin = new();
		FloatingLayoutCommands commands = new(plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.floating_layout.mark_window_as_floating");

		// When
		command.TryExecute();

		// Then
		plugin.Verify(p => p.MarkWindowAsFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsDockedCommand()
	{
		// Given
		Mock<IFloatingLayoutPlugin> plugin = new();
		FloatingLayoutCommands commands = new(plugin.Object);
		PluginCommandsTestUtils testUtils = new(commands);

		ICommand command = testUtils.GetCommand("whim.floating_layout.mark_window_as_docked");

		// When
		command.TryExecute();

		// Then
		plugin.Verify(p => p.MarkWindowAsDocked(null), Times.Once);
	}
}
