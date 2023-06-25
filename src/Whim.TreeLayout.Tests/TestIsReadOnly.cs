using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestIsReadOnly
{
	[Fact]
	public void IsReadOnly()
	{
		// Given
		TestTreeEngineMocks testTreeEngine = new();

		// When
		bool isReadOnly = testTreeEngine.Engine.IsReadOnly;

		// Then
		Assert.False(isReadOnly);
	}
}
