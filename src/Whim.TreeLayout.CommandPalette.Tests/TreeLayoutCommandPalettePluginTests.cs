using System.Text.Json;
using Whim.CommandPalette;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.CommandPalette.Tests;

public class TreeLayoutCommandPalettePluginTests
{
	[Theory, AutoSubstituteData]
	public void Commands(
		IContext context,
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandPalettePlugin
	)
	{
		// Given
		TreeLayoutCommandPalettePlugin plugin = new(context, treeLayoutPlugin, commandPalettePlugin);

		// Then
		Assert.NotEmpty(plugin.PluginCommands.Commands);
		Assert.Empty(plugin.PluginCommands.Keybinds);
		Assert.Equal("Set tree layout direction", plugin.PluginCommands.Commands.First().Title);
	}

	[Theory, AutoSubstituteData]
	public void SaveState(
		IContext context,
		ITreeLayoutPlugin treeLayoutPlugin,
		ICommandPalettePlugin commandPalettePlugin
	)
	{
		// Given
		TreeLayoutCommandPalettePlugin plugin = new(context, treeLayoutPlugin, commandPalettePlugin);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
