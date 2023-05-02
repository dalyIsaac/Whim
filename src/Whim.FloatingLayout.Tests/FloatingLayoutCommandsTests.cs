using Moq;
using Whim.TestUtilities;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutCommandsTests
{
	private class Wrapper
	{
		public Mock<IFloatingLayoutPlugin> Plugin { get; }
		public ICommand Command { get; }

		public Wrapper(string id)
		{
			Plugin = new();
			Plugin.SetupGet(p => p.Name).Returns("whim.floating_layout");

			FloatingLayoutCommands commands = new(Plugin.Object);
			Command = new PluginCommandsTestUtils(commands).GetCommand(id);
		}
	}

	[Fact]
	public void ToggleWindowFloatingCommand()
	{
		// Given
		Wrapper wrapper = new("whim.floating_layout.toggle_window_floating");

		// When
		wrapper.Command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.ToggleWindowFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloatingCommand()
	{
		// Given
		Wrapper wrapper = new("whim.floating_layout.mark_window_as_floating");

		// When
		wrapper.Command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.MarkWindowAsFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsDockedCommand()
	{
		// Given
		Wrapper wrapper = new("whim.floating_layout.mark_window_as_docked");

		// When
		wrapper.Command.TryExecute();

		// Then
		wrapper.Plugin.Verify(p => p.MarkWindowAsDocked(null), Times.Once);
	}
}
