using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutPlugin
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<IWorkspace> Workspace { get; }
		public TreeLayoutEngine LayoutEngine { get; }
		public Mock<IMonitor> Monitor { get; }

		public MocksWrapper()
		{
			Context = new();
			WorkspaceManager = new();
			Workspace = new();
			LayoutEngine = new(Context.Object, "test");
			Monitor = new();

			Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(Workspace.Object);
			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);
			Workspace.Setup(x => x.ActiveLayoutEngine).Returns(LayoutEngine);
		}
	}

	[Fact]
	public void Commands()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutPlugin plugin = new(mocks.Context.Object);

		// When
		IEnumerable<CommandItem> commands = plugin.Commands;

		// Then
		Assert.NotNull(commands);
	}

	[Fact]
	public void SetAddWindowDirection()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutPlugin plugin = new(mocks.Context.Object);

		// When
		plugin.SetAddWindowDirection(mocks.Monitor.Object, Direction.Left);

		// Then
		Assert.Equal(Direction.Left, mocks.LayoutEngine.AddNodeDirection);
	}

	[Fact]
	public void GetAddWindowDirection()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutPlugin plugin = new(mocks.Context.Object);

		// When
		Direction? direction = plugin.GetAddWindowDirection(mocks.Monitor.Object);

		// Then
		Assert.Equal(Direction.Right, direction);
	}
}
