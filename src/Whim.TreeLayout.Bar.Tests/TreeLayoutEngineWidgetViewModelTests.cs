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
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<ITreeLayoutEngine> TreeLayoutEngine { get; } = new();

		public MocksBuilder(bool canGetLayoutEngine)
		{
			Context.SetupGet(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);
			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);
			Workspace.SetupGet(x => x.ActiveLayoutEngine).Returns(TreeLayoutEngine.Object);

			if (canGetLayoutEngine)
			{
				TreeLayoutEngine.Setup(t => t.GetLayoutEngine<ITreeLayoutEngine>()).Returns(TreeLayoutEngine.Object);
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
}
