using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutPlugin
{
	public static (Mock<IContext>, Mock<IWorkspaceManager>, Mock<IWorkspace>, Mock<ITreeLayoutEngine>) CreateMocks()
	{
		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IWorkspace> workspace = new();
		Mock<ITreeLayoutEngine> layoutEngine = new();

		context.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(workspace.Object);
		workspace.Setup(x => x.ActiveLayoutEngine).Returns(layoutEngine.Object);
		layoutEngine.Setup(x => x.GetLayoutEngine<ITreeLayoutEngine>()).Returns(layoutEngine.Object);

		return (context, workspaceManager, workspace, layoutEngine);
	}

	[Fact]
	public void TestGetTreeLayoutEngine()
	{
		(Mock<IContext> context, _, _, _) = CreateMocks();
		TreeLayoutPlugin plugin = new(context.Object);
		ITreeLayoutEngine? engine = plugin.GetTreeLayoutEngine();
		Assert.NotNull(engine);
	}

	[Fact]
	public void TestSetAddWindowDirection()
	{
		(Mock<IContext> context, _, _, Mock<ITreeLayoutEngine> layoutEngine) = CreateMocks();
		TreeLayoutPlugin plugin = new(context.Object);
		plugin.SetAddWindowDirection(Direction.Left);
		layoutEngine.VerifySet(x => x.AddNodeDirection = Direction.Left);
	}
}
