using Moq;
using Whim.CommandPalette;
using Xunit;

namespace Whim.TreeLayout.CommandPalette.Tests;

public class TreeLayoutCommandPalettePluginTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IPlugin> TreeLayoutCommandPalettePlugin { get; } = new();
		public Mock<ITreeLayoutPlugin> TreeLayoutPlugin { get; } = new();
		public Mock<ICommandPalettePlugin> CommandPalettePlugin { get; } = new();
	}

	[Fact]
	public void Commands()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutCommandPalettePlugin plugin =
			new(mocks.Context.Object, mocks.TreeLayoutPlugin.Object, mocks.CommandPalettePlugin.Object);

		// When
		IEnumerable<CommandItem> commands = plugin.Commands;

		// Then
		Assert.Single(commands);
		Assert.Equal("Set tree layout direction", commands.First().Command.Title);
	}
}
