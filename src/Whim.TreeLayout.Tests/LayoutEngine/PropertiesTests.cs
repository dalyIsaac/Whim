using Xunit;

namespace Whim.TreeLayout.Tests;

public class PropertiesTests
{
	[Fact]
	public void Name()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine =
			new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity) { Name = "Test" };

		// When
		string result = engine.Name;

		// Then
		Assert.Equal("Test", result);
	}
}
