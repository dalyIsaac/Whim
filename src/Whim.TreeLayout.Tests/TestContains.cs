using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestContains
{
	[Fact]
	public void ContainsWindows()
	{
		TestTreeEngine testTreeEngine = new();

		foreach (IWindow window in testTreeEngine.GetWindows())
		{
			Assert.True(testTreeEngine.Engine.Contains(window));
		}
	}

	[StaFact]
	public void ContainsPhantomWindows()
	{
		TestTreeEngine testTreeEngine = new();
		testTreeEngine.ActiveWorkspace.Setup(w => w.LastFocusedWindow).Returns(testTreeEngine.LeftWindow.Object);
		testTreeEngine.Engine.SplitFocusedWindow();

		SplitNode root = (SplitNode)testTreeEngine.Engine.Root!;
		SplitNode left = (SplitNode)root[0].node;
		Assert.True(left[1].node is PhantomNode);
	}
}
