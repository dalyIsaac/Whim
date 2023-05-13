using Moq;
using System.Text.Json;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class TestTreeLayoutPlugin
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; }
		public Mock<IWorkspaceManager> WorkspaceManager { get; }
		public Mock<IMonitorManager> MonitorManager { get; }
		public Mock<IWorkspace> Workspace { get; }
		public TreeLayoutEngine LayoutEngine { get; }
		public Mock<IMonitor> Monitor { get; }

		public MocksWrapper()
		{
			Context = new();
			MonitorManager = new();
			WorkspaceManager = new();
			Workspace = new();
			LayoutEngine = new(Context.Object, "test");
			Monitor = new();

			Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);

			WorkspaceManager.Setup(x => x.ActiveWorkspace).Returns(Workspace.Object);
			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);
			Workspace.Setup(x => x.ActiveLayoutEngine).Returns(LayoutEngine);

			MonitorManager.Setup(x => x.FocusedMonitor).Returns(Monitor.Object);
		}
	}

	[Fact]
	public void Commands()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutPlugin plugin = new(mocks.Context.Object);

		// Then
		Assert.NotEmpty(plugin.PluginCommands.Commands);
		Assert.NotEmpty(plugin.PluginCommands.Keybinds);
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

	[Fact]
	public void SaveState()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutPlugin plugin = new(mocks.Context.Object);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}
}
