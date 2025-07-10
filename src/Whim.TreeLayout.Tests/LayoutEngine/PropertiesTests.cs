using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class PropertiesTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void Name(IContext ctx, ITreeLayoutPlugin plugin, LayoutEngineIdentity identity)
	{
		// Given
		TreeLayoutEngine engine = new(ctx, plugin, identity) { Name = "Test" };

		// When
		string result = engine.Name;

		// Then
		Assert.Equal("Test", result);
	}
}
