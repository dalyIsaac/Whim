using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewCommandsTests
{
	[Theory, AutoSubstituteData]
	public void Name(IPlugin plugin)
	{
		// Given
		plugin.Name.Returns("whim.layout_preview");
		LayoutPreviewCommands layoutPreviewCommands = new(plugin);

		// When
		string name = layoutPreviewCommands.PluginName;

		// Then
		Assert.Equal("whim.layout_preview", name);
	}

	[Theory, AutoSubstituteData]
	public void Commands(IPlugin plugin)
	{
		// Given
		LayoutPreviewCommands layoutPreviewCommands = new(plugin);

		// When
		IEnumerable<ICommand> commands = layoutPreviewCommands.Commands;

		// Then
		Assert.Empty(commands);
	}

	[Theory, AutoSubstituteData]
	public void Keybinds(IPlugin plugin)
	{
		// Given
		LayoutPreviewCommands layoutPreviewCommands = new(plugin);

		// When
		IEnumerable<(string commandId, IKeybind keybind)> keybinds = layoutPreviewCommands.Keybinds;

		// Then
		Assert.Empty(keybinds);
	}
}
