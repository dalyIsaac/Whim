using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class PerformCustomActionTests
{
	private static readonly LayoutEngineIdentity identity = new();
	private static readonly IRectangle<int> primaryMonitorBounds = new Rectangle<int>(0, 0, 100, 100);

	#region PromoteWindowInStack
	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack_CannotFindWindow(IContext ctx, SliceLayoutPlugin plugin, IWindow untrackedWindow)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.promote",
				Window = untrackedWindow,
				Payload = untrackedWindow,
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}

	public static TheoryData<int, int> PromoteWindowInStack_Data =>
		new()
		{
			// Already highest area
			{ 0, 0 },
			// Promote to higher area, slice to slice
			{ 2, 1 },
			// Promote to higher area, overflow to slice
			{ 5, 3 },
			// Promote to higher area, slice to slice
			{ 3, 1 },
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(PromoteWindowInStack_Data))]
	public void PromoteWindowInStack(
		int focusedWindowIdx,
		int expectedWindowIdx,
		IContext ctx,
		SliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.promote",
				Window = windows[focusedWindowIdx],
				Payload = windows[focusedWindowIdx],
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		Assert.Equal(windows[expectedWindowIdx], afterStates[focusedWindowIdx].Window);
		Assert.Equal(windows[focusedWindowIdx], afterStates[expectedWindowIdx].Window);
	}

	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack_EmptyLayoutEngine(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.promote",
				Window = window,
				Payload = window,
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}
	#endregion

	#region DemoteWindowInStack
	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack_CannotFindWindow(IContext ctx, SliceLayoutPlugin plugin, IWindow untrackedWindow)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.demote",
				Window = untrackedWindow,
				Payload = untrackedWindow,
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}

	public static TheoryData<int, int> DemoteWindowInStack_Data =>
		new()
		{
			// Already lowest area
			{ 5, 5 },
			// Demote to lower area, slice to slice
			{ 1, 2 },
			// Demote to lower area, slice to overflow
			{ 3, 4 },
			// Demote to lower area, slice to slice
			{ 1, 2 },
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(DemoteWindowInStack_Data))]
	public void DemoteWindowInStack(int focusedWindowIdx, int expectedWindowIdx, IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.demote",
				Window = windows[focusedWindowIdx],
				Payload = windows[focusedWindowIdx],
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		Assert.Equal(windows[expectedWindowIdx], afterStates[focusedWindowIdx].Window);
		Assert.Equal(windows[focusedWindowIdx], afterStates[expectedWindowIdx].Window);
	}

	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack_EmptyLayoutEngine(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.demote",
				Window = window,
				Payload = window,
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}
	#endregion

	#region PromoteFocusInStack
	[Theory]
	[InlineAutoSubstituteData(0, 0, true)]
	[InlineAutoSubstituteData(1, 0, true)]
	[InlineAutoSubstituteData(2, 1, true)]
	[InlineAutoSubstituteData(4, 3, true)]
	[InlineAutoSubstituteData(0, 2, false)]
	[InlineAutoSubstituteData(1, 2, false)]
	[InlineAutoSubstituteData(2, 4, false)]
	[InlineAutoSubstituteData(4, 5, false)]
	public void PerformCustomAction_PromoteFocus(
		int focusedWindowIdx,
		int expectedWindowIdx,
		bool promote,
		IContext ctx,
		SliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		ILayoutEngine resultSut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = promote ? plugin.PromoteFocusActionName : plugin.DemoteFocusActionName,
				Window = windows[focusedWindowIdx],
				Payload = windows[focusedWindowIdx],
			}
		);

		// Then
		Assert.Same(sut, resultSut);
		windows[expectedWindowIdx].Received(1).Focus();
	}

	#endregion

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_UnknownAction(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = [.. Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.window.unknown",
				Window = windows[0],
				Payload = windows[0],
			}
		);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>())];

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}
}
