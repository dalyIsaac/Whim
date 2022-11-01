using Moq;
using Xunit;

namespace Whim.FloatingLayout.Tests;

public class FloatingLayoutCommandsTests
{
	private static Mock<IFloatingLayoutPlugin> CreateFloatingLayoutPluginMock()
	{
		Mock<IFloatingLayoutPlugin> plugin = new();
		return plugin;
	}

	[Fact]
	public void ToggleWindowFloatingCommand()
	{
		Mock<IFloatingLayoutPlugin> plugin = CreateFloatingLayoutPluginMock();
		FloatingLayoutCommands commands = new(plugin.Object);

		commands.ToggleWindowFloatingCommand.Command.TryExecute();

		plugin.Verify(p => p.ToggleWindowFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsFloatingCommand()
	{
		Mock<IFloatingLayoutPlugin> plugin = CreateFloatingLayoutPluginMock();
		FloatingLayoutCommands commands = new(plugin.Object);

		commands.MarkWindowAsFloatingCommand.Command.TryExecute();

		plugin.Verify(p => p.MarkWindowAsFloating(null), Times.Once);
	}

	[Fact]
	public void MarkWindowAsDockedCommand()
	{
		Mock<IFloatingLayoutPlugin> plugin = CreateFloatingLayoutPluginMock();
		FloatingLayoutCommands commands = new(plugin.Object);

		commands.MarkWindowAsDockedCommand.Command.TryExecute();

		plugin.Verify(p => p.MarkWindowAsDocked(null), Times.Once);
	}
}
