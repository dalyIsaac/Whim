using System.Text.Json;
using Whim.Bar;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

public class TreeLayoutBarPluginTests
{
	[Theory, AutoSubstituteData]
	public void Name(ITreeLayoutPlugin treeLayoutPlugin)
	{
		// Given
		TreeLayoutBarPlugin plugin = new(treeLayoutPlugin);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.tree_layout.bar", name);
	}

	[Theory, AutoSubstituteData]
	public void Commands(ITreeLayoutPlugin treeLayoutPlugin)
	{
		// Given
		TreeLayoutBarPlugin plugin = new(treeLayoutPlugin);

		// Then
		Assert.Empty(plugin.PluginCommands.Commands);
		Assert.Empty(plugin.PluginCommands.Keybinds);
	}

	[Theory, AutoSubstituteData]
	public void CreateComponent(ITreeLayoutPlugin treeLayoutPlugin)
	{
		// Given
		TreeLayoutBarPlugin plugin = new(treeLayoutPlugin);

		// When
		BarComponent component = plugin.CreateComponent();

		// Then
		Assert.NotNull(component);
	}

	[Theory, AutoSubstituteData]
	public void SaveState(ITreeLayoutPlugin treeLayoutPlugin)
	{
		// Given
		TreeLayoutBarPlugin plugin = new(treeLayoutPlugin);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
