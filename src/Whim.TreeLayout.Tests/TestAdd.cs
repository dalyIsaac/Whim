using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestAdd
{
	[Fact]
	public void Add_Root()
	{
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IConfigContext> configContext = new();
		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);

		TreeLayoutEngine engine = new(configContext.Object);

		Mock<IWindow> window = new();
		engine.Add(window.Object);

		Assert.Equal(engine.Root, new LeafNode(window.Object));
	}

	[Fact]
	public void Add_TestTree()
	{
		TestTreeEngine testEngine = new();

		TestTree tree = new(
			leftWindow: testEngine.LeftWindow,
			rightTopLeftTopWindow: testEngine.RightTopLeftTopWindow,
			rightBottomWindow: testEngine.RightBottomWindow,
			rightTopRight1Window: testEngine.RightTopRight1Window,
			rightTopRight2Window: testEngine.RightTopRight2Window,
			rightTopRight3Window: testEngine.RightTopRight3Window,
			rightTopLeftBottomLeftWindow: testEngine.RightTopLeftBottomLeftWindow,
			rightTopLeftBottomRightTopWindow: testEngine.RightTopLeftBottomRightTopWindow,
			rightTopLeftBottomRightBottomWindow: testEngine.RightTopLeftBottomRightBottomWindow
		);
		Assert.Equal(testEngine.Engine.Root, tree.Root);
	}
}
