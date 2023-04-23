using Moq;
using Whim.Bar;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

public class TreeLayoutBarPluginTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ITreeLayoutPlugin> TreeLayoutPlugin { get; } = new();
	}

	[Fact]
	public void Name()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutBarPlugin plugin = new(mocks.TreeLayoutPlugin.Object);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.tree_layout.bar", name);
	}

	[Fact]
	public void Commands()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutBarPlugin plugin = new(mocks.TreeLayoutPlugin.Object);

		// When
		IEnumerable<CommandItem> commands = plugin.Commands;

		// Then
		Assert.Empty(commands);
	}

	[Fact]
	public void CreateComponent()
	{
		// Given
		MocksBuilder mocks = new();
		TreeLayoutBarPlugin plugin = new(mocks.TreeLayoutPlugin.Object);

		// When
		BarComponent component = plugin.CreateComponent();

		// Then
		Assert.NotNull(component);
	}
}
