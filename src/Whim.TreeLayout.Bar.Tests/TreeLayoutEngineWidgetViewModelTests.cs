using Microsoft.UI.Xaml;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class TreeLayoutEngineWidgetViewModelTests
{
	private class MocksWrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<ITreeLayoutPlugin> Plugin { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IWorkspace> Workspace2 { get; } = new();

		public MocksWrapper(Direction? direction = null)
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);

			MonitorManager.Setup(x => x.ActiveMonitor).Returns(Monitor.Object);

			Plugin.Setup(x => x.GetAddWindowDirection(Monitor.Object)).Returns(direction);
		}
	}

	[Fact]
	public void IsVisible_WhenDirectionValueIsNull_ReturnsCollapsed()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Collapsed, actual);
	}

	[Fact]
	public void IsVisible_WhenDirectionValueIsNotNull_ReturnsVisible()
	{
		// Given
		MocksWrapper mocks = new(Direction.Left);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Visible, actual);
	}

	[Fact]
	public void AddNodeDirection_WhenDirectionValueIsNull_ReturnsNull()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		string? actual = viewModel.AddNodeDirection;

		// Then
		Assert.Null(actual);
	}

	[InlineData(Direction.Left, "Left")]
	[InlineData(Direction.Right, "Right")]
	[InlineData(Direction.Up, "Up")]
	[InlineData(Direction.Down, "Down")]
	[Theory]
	public void AddNodeDirection_WhenDirectionValueIsNotNull_ReturnsStringRepresentation(
		Direction direction,
		string expected
	)
	{
		// Given
		MocksWrapper mocks = new(direction);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		string? actual = viewModel.AddNodeDirection;

		// Then
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ToggleDirection_WhenDirectionValueIsNull_DoesNothing()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		Assert.Null(viewModel.AddNodeDirection);
	}

	[InlineData(Direction.Left, Direction.Up)]
	[InlineData(Direction.Up, Direction.Right)]
	[InlineData(Direction.Right, Direction.Down)]
	[InlineData(Direction.Down, Direction.Left)]
	[Theory]
	public void ToggleDirection_WhenDirectionValueIsNotNull_TogglesDirection(Direction initial, Direction expected)
	{
		// Given
		MocksWrapper mocks = new(initial);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, expected), Times.Once);
	}

	[Fact]
	public void ToggleDirection_EngineIsNull()
	{
		// Given
		MocksWrapper mocks = new(Direction.Left);
		mocks.Plugin.Setup(p => p.GetAddWindowDirection(mocks.Monitor.Object)).Returns((Direction?)null);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, It.IsAny<Direction>()), Times.Never);
		Assert.Null(viewModel.AddNodeDirection);
	}

	[Fact]
	public void ToggleDirection_InvalidDirection()
	{
		// Given
		MocksWrapper mocks = new();
		mocks.Plugin.Setup(p => p.GetAddWindowDirection(mocks.Monitor.Object)).Returns((Direction)42);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, It.IsAny<Direction>()), Times.Never);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_Success()
	{
		// Given
		MocksWrapper mocks = new(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);
		mocks.Plugin.Setup(p => p.GetAddWindowDirection(mocks.Monitor.Object)).Returns(Direction.Down);

		// When
		mocks.WorkspaceManager.Raise(
			x => x.MonitorWorkspaceChanged += null,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = mocks.Monitor.Object,
				NewWorkspace = mocks.Workspace2.Object,
				OldWorkspace = mocks.Workspace.Object
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_WrongMonitor()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		mocks.WorkspaceManager.Raise(
			x => x.MonitorWorkspaceChanged += null,
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = new Mock<IMonitor>().Object,
				NewWorkspace = mocks.Workspace2.Object,
				OldWorkspace = mocks.Workspace.Object
			}
		);

		// Then should not have called anything
		mocks.WorkspaceManager.Verify(x => x.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Never);
	}

	[Fact]
	public void WorkspaceManager_ActiveLayoutEngineChanged_Success()
	{
		// Given
		MocksWrapper mocks = new(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		mocks.WorkspaceManager
			.Setup(wm => wm.GetWorkspaceForMonitor(mocks.Monitor.Object))
			.Returns(mocks.Workspace.Object);
		mocks.Plugin.Setup(p => p.GetAddWindowDirection(It.IsAny<IMonitor>())).Returns(Direction.Down);

		// When
		mocks.WorkspaceManager.Raise(
			x => x.ActiveLayoutEngineChanged += null,
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = mocks.Workspace.Object,
				CurrentLayoutEngine = new Mock<ITreeLayoutEngine>().Object,
				PreviousLayoutEngine = new Mock<ITreeLayoutEngine>().Object
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Fact]
	public void Plugin_AddWindowDirectionChanged_Success()
	{
		// Given
		MocksWrapper mocks = new(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		mocks.Plugin.Setup(p => p.GetAddWindowDirection(It.IsAny<IMonitor>())).Returns(Direction.Down);

		// When
		mocks.Plugin.Raise(
			x => x.AddWindowDirectionChanged += null,
			new AddWindowDirectionChangedEventArgs()
			{
				TreeLayoutEngine = new Mock<ITreeLayoutEngine>().Object,
				CurrentDirection = Direction.Down,
				PreviousDirection = Direction.Right
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		MocksWrapper mocks = new();
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.Dispose();

		// Then
		mocks.WorkspaceManager.VerifyRemove(
			x => x.MonitorWorkspaceChanged -= It.IsAny<EventHandler<MonitorWorkspaceChangedEventArgs>>(),
			Times.Once
		);
		mocks.WorkspaceManager.VerifyRemove(
			x => x.ActiveLayoutEngineChanged -= It.IsAny<EventHandler<ActiveLayoutEngineChangedEventArgs>>(),
			Times.Once
		);
	}

	[Fact]
	public void ToggleDirectionCommand()
	{
		// Given
		MocksWrapper mocks = new(Direction.Left);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Plugin.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirectionCommand.Execute(null);

		// Then
		mocks.Plugin.Verify(x => x.SetAddWindowDirection(mocks.Monitor.Object, It.IsAny<Direction>()), Times.Once);
	}
}
