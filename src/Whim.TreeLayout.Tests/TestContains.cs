using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestContains
{
	[Fact]
	public void ContainsWindows()
	{
		TestTreeEngineMocks testTreeEngine = new();

		foreach (IWindow window in testTreeEngine.GetWindows())
		{
			Assert.Contains(window, testTreeEngine.Engine);
		}
	}

	[Fact]
	public void ContainsPhantomWindows()
	{
		// Given
		TestTreeEngineMocks testTreeEngine = new();
		testTreeEngine.ActiveWorkspace.Setup(w => w.LastFocusedWindow).Returns(testTreeEngine.LeftWindow.Object);

		// When
		testTreeEngine.SplitFocusedWindowWrapper(new Mock<IContext>().Object);

		// Then
		SplitNode root = (SplitNode)testTreeEngine.Engine.Root!;
		SplitNode left = (SplitNode)root[0].node;
		Assert.True(left[1].node is PhantomNode);
	}
}
