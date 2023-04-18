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
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<IWorkspace> Workspace2 { get; } = new();
		public Mock<ITreeLayoutEngine> TreeLayoutEngine { get; } = new();
		public Mock<ITreeLayoutEngine> TreeLayoutEngine2 { get; } = new();

		public MocksBuilder(bool canGetLayoutEngine)
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.SetupGet(x => x.MonitorManager).Returns(MonitorManager.Object);

			MonitorManager.Setup(x => x.FocusedMonitor).Returns(Monitor.Object);

			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);

			Workspace.SetupGet(x => x.ActiveLayoutEngine).Returns(TreeLayoutEngine.Object);
			Workspace2.SetupGet(x => x.ActiveLayoutEngine).Returns(TreeLayoutEngine2.Object);

			TreeLayoutEngine2.SetupGet(x => x.AddNodeDirection).Returns(Direction.Down);

			if (canGetLayoutEngine)
			{
				TreeLayoutEngine.Setup(t => t.GetLayoutEngine<ITreeLayoutEngine>()).Returns(TreeLayoutEngine.Object);
				TreeLayoutEngine2.Setup(t => t.GetLayoutEngine<ITreeLayoutEngine>()).Returns(TreeLayoutEngine2.Object);
			}
		}
	}

	[Fact]
	public void IsVisible_WhenDirectionValueIsNull_ReturnsCollapsed()
	{
		// Given
		MocksBuilder mocks = new(false);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Collapsed, actual);
	}

	[Fact]
	public void IsVisible_WhenDirectionValueIsNotNull_ReturnsVisible()
	{
		// Given
		MocksBuilder mocks = new(false);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Monitor.Object) { DirectionValue = Direction.Left };

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Visible, actual);
	}

	[Fact]
	public void AddNodeDirection_WhenDirectionValueIsNull_ReturnsNull()
	{
		// Given
		MocksBuilder mocks = new(false);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

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
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Monitor.Object) { DirectionValue = direction };

		// When
		string? actual = viewModel.AddNodeDirection;

		// Then
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ToggleDirection_WhenDirectionValueIsNull_DoesNothing()
	{
		// Given
		MocksBuilder mocks = new(false);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		Assert.Null(viewModel.DirectionValue);
	}

	[InlineData(Direction.Left, Direction.Up)]
	[InlineData(Direction.Up, Direction.Right)]
	[InlineData(Direction.Right, Direction.Down)]
	[InlineData(Direction.Down, Direction.Left)]
	[Theory]
	public void ToggleDirection_WhenDirectionValueIsNotNull_TogglesDirection(Direction initial, Direction expected)
	{
		// Given
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel =
			new(mocks.Context.Object, mocks.Monitor.Object) { DirectionValue = initial };

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.TreeLayoutEngine.VerifySet(t => t.AddNodeDirection = expected, Times.Once);
	}

	[Fact]
	public void ToggleDirection_EngineIsNull()
	{
		// Given
		MocksBuilder mocks = new(true);
		mocks.TreeLayoutEngine.Setup(t => t.GetLayoutEngine<ITreeLayoutEngine>()).Returns((ITreeLayoutEngine?)null);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.TreeLayoutEngine.VerifySet(t => t.AddNodeDirection = It.IsAny<Direction>(), Times.Never);
	}

	[Fact]
	public void ToggleDirection_InvalidDirection()
	{
		// Given
		MocksBuilder mocks = new(true);
		mocks.TreeLayoutEngine.SetupGet(t => t.AddNodeDirection).Returns((Direction)42);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirection();

		// Then
		mocks.TreeLayoutEngine.VerifySet(t => t.AddNodeDirection = It.IsAny<Direction>(), Times.Never);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_Success()
	{
		// Given
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);
		mocks.WorkspaceManager
			.Setup(x => x.GetWorkspaceForMonitor(mocks.Monitor.Object))
			.Returns(mocks.Workspace2.Object);

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
		Assert.Equal(Direction.Down, viewModel.DirectionValue);
	}

	[Fact]
	public void WorkspaceManager_MonitorWorkspaceChanged_WrongMonitor()
	{
		// Given
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		mocks.WorkspaceManager.Reset();
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
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);
		mocks.Workspace.SetupGet(x => x.ActiveLayoutEngine).Returns(mocks.TreeLayoutEngine2.Object);

		// When
		mocks.WorkspaceManager.Raise(
			x => x.ActiveLayoutEngineChanged += null,
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = mocks.Workspace.Object,
				CurrentLayoutEngine = mocks.TreeLayoutEngine2.Object,
				PreviousLayoutEngine = mocks.TreeLayoutEngine.Object
			}
		);

		// Then
		Assert.Equal(Direction.Down, viewModel.DirectionValue);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

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
		MocksBuilder mocks = new(true);
		TreeLayoutEngineWidgetViewModel viewModel = new(mocks.Context.Object, mocks.Monitor.Object);

		// When
		viewModel.ToggleDirectionCommand.Execute(null);

		// Then
		mocks.TreeLayoutEngine.VerifySet(t => t.AddNodeDirection = It.IsAny<Direction>(), Times.Once);
	}
}
