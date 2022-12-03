using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutPlugin
{
	public static (
		Mock<IConfigContext>,
		Mock<IWorkspaceManager>,
		Mock<IWorkspace>,
		Mock<ITreeLayoutEngine>
	) CreateMocks()
	{
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		Mock<IWorkspace> workspace = new();
		Mock<ITreeLayoutEngine> layoutEngine = new();

		configContext.Setup(x => x.WorkspaceManager).Returns(workspaceManager.Object);
		workspaceManager.Setup(x => x.ActiveWorkspace).Returns(workspace.Object);
		workspace.Setup(x => x.ActiveLayoutEngine).Returns(layoutEngine.Object);
		layoutEngine.Setup(x => x.GetLayoutEngine<ITreeLayoutEngine>()).Returns(layoutEngine.Object);

		return (configContext, workspaceManager, workspace, layoutEngine);
	}

	[Fact]
	public void TestGetTreeLayoutEngine()
	{
		(Mock<IConfigContext> configContext, _, _, _) = CreateMocks();
		TreeLayoutPlugin plugin = new(configContext.Object);
		ITreeLayoutEngine? engine = plugin.GetTreeLayoutEngine();
		Assert.NotNull(engine);
	}

	[Fact]
	public void TestSetAddWindowDirection()
	{
		(Mock<IConfigContext> configContext, _, _, Mock<ITreeLayoutEngine> layoutEngine) = CreateMocks();
		TreeLayoutPlugin plugin = new(configContext.Object);
		plugin.SetAddWindowDirection(Direction.Left);
		layoutEngine.VerifySet(x => x.AddNodeDirection = Direction.Left);
	}
}
