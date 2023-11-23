using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.CommandPalette.Tests;

public class CommandPalettePluginTests
{
	[Theory, AutoSubstituteData]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	public void PreInitialize_ShouldIgnoreTitleMatch(IContext ctx)
	{
		// Given
		CommandPaletteConfig commandPaletteConfig = new(ctx);
		CommandPalettePlugin commandPalettePlugin = new(ctx, commandPaletteConfig);

		// When
		commandPalettePlugin.PreInitialize();

		// Then
		ctx.FilterManager.Received(1).AddTitleMatchFilter(CommandPaletteConfig.Title);
	}
}
