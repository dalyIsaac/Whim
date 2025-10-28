using AutoFixture;
using Microsoft.UI.Xaml;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.TreeLayout.Bar.Tests;

public class TreeLayoutEngineWidgetViewModelTests
{
	private class Customization : StoreCustomization
	{
		protected override void PostCustomize(IFixture fixture)
		{
			IMonitor monitor = CreateMonitor((HMONITOR)1234);
			fixture.Inject(monitor);

			var root = _store._root.MutableRootSector;

			ILayoutEngine layoutEngine = fixture.Freeze<ILayoutEngine>();
			Workspace workspace = CreateWorkspace() with { LayoutEngines = [layoutEngine] };
			fixture.Inject(workspace);

			PopulateMonitorWorkspaceMap(root, monitor, workspace);
		}
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void IsVisible_WhenDirectionValueIsNull_ReturnsCollapsed(
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

	[Theory, AutoSubstituteData<Customization>]
	internal void IsVisible_WhenDirectionValueIsNotNull_ReturnsVisible(
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

	[Theory, AutoSubstituteData<Customization>]
	internal void AddNodeDirection_WhenDirectionValueIsNull_ReturnsNull(
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

	[InlineAutoSubstituteData<Customization>(Direction.Left, "Left")]
	[InlineAutoSubstituteData<Customization>(Direction.Right, "Right")]
	[InlineAutoSubstituteData<Customization>(Direction.Up, "Up")]
	[InlineAutoSubstituteData<Customization>(Direction.Down, "Down")]
	[Theory]
	internal void AddNodeDirection_WhenDirectionValueIsNotNull_ReturnsStringRepresentation(
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

	[Theory, AutoSubstituteData<Customization>]
	internal void Butler_MonitorWorkspaceChanged_Success(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Down);

		// When
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = monitor,
				CurrentWorkspace = Substitute.For<IWorkspace>(),
				PreviousWorkspace = Substitute.For<IWorkspace>(),
			}
		);
		root.DispatchEvents();

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Butler_MonitorWorkspaceChanged_WrongMonitor(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		IMonitor monitor
	)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// Then should not have PropertyChanged event raised;
		root.MapSector.QueueEvent(
			new MonitorWorkspaceChangedEventArgs()
			{
				Monitor = Substitute.For<IMonitor>(),
				CurrentWorkspace = Substitute.For<IWorkspace>(),
				PreviousWorkspace = Substitute.For<IWorkspace>(),
			}
		);
		CustomAssert.DoesNotPropertyChange(
			h => viewModel.PropertyChanged += h,
			h => viewModel.PropertyChanged -= h,
			root.DispatchEvents
		);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Butler_ActiveLayoutEngineChanged_Success(
		IContext ctx,
		ITreeLayoutPlugin plugin,
		MutableRootSector root,
		IMonitor monitor,
		IWorkspace workspace
	)
	{
		// Given
		plugin.GetAddWindowDirection(monitor).Returns(Direction.Right);
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		plugin.GetAddWindowDirection(monitor).Returns(Direction.Down);

		// When
		root.MapSector.QueueEvent(
			new ActiveLayoutEngineChangedEventArgs()
			{
				Workspace = workspace,
				CurrentLayoutEngine = Substitute.For<ILayoutEngine>(),
				PreviousLayoutEngine = Substitute.For<ILayoutEngine>(),
			}
		);
		root.DispatchEvents();

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData<Customization>]
	internal void Plugin_AddWindowDirectionChanged_Success(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
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
				PreviousDirection = Direction.Right,
			}
		);

		// Then
		Assert.Equal(Direction.Down.ToString(), viewModel.AddNodeDirection);
	}

	[Theory, AutoSubstituteData]
	internal void Dispose(IContext ctx, ITreeLayoutPlugin plugin, IMonitor monitor)
	{
		// Given
		TreeLayoutEngineWidgetViewModel viewModel = new(ctx, plugin, monitor);

		// When
		viewModel.Dispose();

		// Then
		ctx.Store.MapEvents.Received(1).MonitorWorkspaceChanged -= Arg.Any<
			EventHandler<MonitorWorkspaceChangedEventArgs>
		>();
		ctx.Store.WorkspaceEvents.Received(1).ActiveLayoutEngineChanged -= Arg.Any<
			EventHandler<ActiveLayoutEngineChangedEventArgs>
		>();
		plugin.AddWindowDirectionChanged -= Arg.Any<EventHandler<AddWindowDirectionChangedEventArgs>>();
	}
}
