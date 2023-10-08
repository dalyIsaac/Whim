using Xunit;

namespace Whim.TreeLayout.Tests;

public class PropertiesTests
{
	[Fact]
	public void Name()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity) { Name = "Test" };

		// When
		string result = engine.Name;

		// Then
		Assert.Equal("Test", result);
	}
}
