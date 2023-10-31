using AutoFixture;
using Microsoft.UI.Xaml;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Bar.Tests;

public class TreeLayoutEngineWidgetViewModelCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();
		IMonitor monitor = fixture.Freeze<IMonitor>();

		ctx.MonitorManager.ActiveMonitor.Returns(monitor);
	}
}

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class TreeLayoutEngineWidgetViewModelTests
{
	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void IsVisible_WhenDirectionValueIsNull_ReturnsCollapsed(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns((Direction?)null);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Collapsed, actual);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void IsVisible_WhenDirectionValueIsNotNull_ReturnsVisible(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Left);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		Visibility actual = viewModel.IsVisible;

		// Then
		Assert.Equal(Visibility.Visible, actual);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void AddNodeDirection_WhenDirectionValueIsNull_ReturnsNull(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		string? actual = viewModel.AddNodeDirection;

		// Then
		Assert.Null(actual);
	}

	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Left, "Left")]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Right, "Right")]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Up, "Up")]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Down, "Down")]
	[Theory]
	public void AddNodeDirection_WhenDirectionValueIsNotNull_ReturnsStringRepresentation(
		Direction direction,
		string expected,
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(direction);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		string? actual = viewModel.AddNodeDirection;

		// Then
		Assert.Equal(expected, actual);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void ToggleDirection_WhenDirectionValueIsNull_DoesNothing(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.ToggleDirection();

		// Then
		Assert.Null(viewModel.AddNodeDirection);
	}

	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Left, Direction.Up)]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Up, Direction.Right)]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Right, Direction.Down)]
	[InlineAutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>(Direction.Down, Direction.Left)]
	[Theory]
	public void ToggleDirection_WhenDirectionValueIsNotNull_TogglesDirection(
		Direction initial,
		Direction expected,
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(initial);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.ToggleDirection();

		// Then
		plugin.Received(1).SetAddWindowDirection(monitor, expected);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void ToggleDirection_EngineIsNull(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns((Direction?)null);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.ToggleDirection();

		// Then
		plugin.DidNotReceive().SetAddWindowDirection(monitor, Arg.Any<Direction>());
		Assert.Null(viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void ToggleDirection_InvalidDirection(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns((Direction)42);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.ToggleDirection();

		// Then
		plugin.Received(1).SetAddWindowDirection(monitor, Arg.Any<Direction>());
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void WorkspaceManager_MonitorWorkspaceChanged_Success(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Down);

		// When
		ctx.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<EventHandler<MonitorWorkspaceChangedEventArgs>>(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				CurrentWorkspace = Substitute.For<IWorkspace>(),
				PreviousWorkspace = Substitute.For<IWorkspace>()
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void WorkspaceManager_MonitorWorkspaceChanged_WrongMonitor(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// Then should not have PropertyChanged event raised
		CustomAssert.DoesNotPropertyChange(
			h => viewModel.PropertyChanged += h,
			h => viewModel.PropertyChanged -= h,
			() =>
				ctx.WorkspaceManager.MonitorWorkspaceChanged += Raise.Event<
					EventHandler<MonitorWorkspaceChangedEventArgs>
				>(
					new MonitorWorkspaceChangedEventArgs()
					{
						Monitor = Substitute.For<IMonitor>(),
						CurrentWorkspace = Substitute.For<IWorkspace>(),
						PreviousWorkspace = Substitute.For<IWorkspace>()
					}
				)
		);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void WorkspaceManager_ActiveLayoutEngineChanged_Success(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		IMonitor monitor,
		IWorkspace workspace
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		ctx.WorkspaceManager.GetWorkspaceForMonitor(monitor).Returns(workspace);
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Down);

		// When
		ctx.WorkspaceManager.ActiveLayoutEngineChanged += Raise.Event<EventHandler<ActiveLayoutEngineChangedEventArgs>>(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				CurrentLayoutEngine = Substitute.For<ILayoutEngine>(),
				PreviousLayoutEngine = Substitute.For<ILayoutEngine>()
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void Plugin_AddWindowDirectionChanged_Success(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		plugin.GetAddWindowDirection(monitor).Returns(Direction.Down);

		// When
		plugin.AddWindowDirectionChanged += Raise.Event<EventHandler<AddWindowDirectionChangedEventArgs>>(
			new AddWindowDirectionChangedEventArgs()
			{
				TreeLayoutEngine = Substitute.For<ILayoutEngine>(),
				CurrentDirection = Direction.Down,
				PreviousDirection = Direction.Right
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "NS5000:Received check.")]
	public void Dispose(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.Dispose();

		// Then
		ctx.WorkspaceManager.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.WorkspaceManager.Received(1).ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
	}

	[Theory, AutoSubstituteData<TreeLayoutEngineWidgetViewModelCustomization>]
	public void ToggleDirectionCommand(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Left);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.ToggleDirectionCommand.Execute(null);

		// Then
		plugin.Received(1).SetAddWindowDirection(monitor, Direction.Up);
	}
}
