#pragma warning disable CA2000 // Dispose objects before losing scope
using Moq;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPalettePluginTests
{
	[Fact]
	public void PreInitialize_ShouldIgnoreTitleMatch()
	{
		// Given
		Mock<IFilterManager> filterManagerMock = new();
		Mock<IContext> contextMock = new();
		contextMock.Setup(x => x.FilterManager).Returns(filterManagerMock.Object);

		CommandPaletteConfig commandPaletteConfig = new(contextMock.Object);
		CommandPalettePlugin commandPalettePlugin = new(contextMock.Object, commandPaletteConfig);

		// When
		commandPalettePlugin.PreInitialize();

		// Then
		contextMock.Verify(x => x.FilterManager.IgnoreTitleMatch(CommandPaletteConfig.Title), Times.Once);
	}
}

#pragma warning restore CA2000 // Dispose objects before losing scope
