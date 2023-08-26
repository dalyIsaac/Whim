using Moq;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewCommandsTests
{
	[Fact]
	public void Name()
	{
		// Given
		Mock<IPlugin> plugin = new();
		plugin.SetupGet(p => p.Name).Returns("whim.layout_preview");
		LayoutPreviewCommands layoutPreviewCommands = new(plugin.Object);

		// When
		string name = layoutPreviewCommands.PluginName;

		// Then
		Assert.Equal("whim.layout_preview", name);
	}

	[Fact]
	public void Commands()
	{
		// Given
		Mock<IPlugin> plugin = new();
		LayoutPreviewCommands layoutPreviewCommands = new(plugin.Object);

		// When
		IEnumerable<ICommand> commands = layoutPreviewCommands.Commands;

		// Then
		Assert.Empty(commands);
	}

	[Fact]
	public void Keybinds()
	{
		// Given
		Mock<IPlugin> plugin = new();
		LayoutPreviewCommands layoutPreviewCommands = new(plugin.Object);

		// When
		IEnumerable<(string commandId, IKeybind keybind)> keybinds = layoutPreviewCommands.Keybinds;

		// Then
		Assert.Empty(keybinds);
	}
}
