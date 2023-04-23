using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestClear
{
	[Fact]
	public void Clear()
	{
		TestTreeEngineMocks testTreeEngine = new();

		testTreeEngine.Engine.Clear();
		Assert.Empty(testTreeEngine.Engine);
		Assert.Null(testTreeEngine.Engine.Root);
	}
}
