using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class PerformCustomActionTests
{
	private static readonly LayoutEngineIdentity identity = new();

	#region PromoteWindowInStack
	[Theory, AutoSubstituteData]
	public void PromoteWindowInStack_CannotFindWindow(IContext ctx, SliceLayoutPlugin plugin, IWindow untrackedWindow)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.stack.promote",
				Window = untrackedWindow,
				Payload = untrackedWindow
			}
		);

		IWindowState[] afterStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}

	public static IEnumerable<object[]> PromoteWindowInStack_Data()
	{
		// Already highest area
		yield return new object[] { 0, 0 };

		// Promote to higher area, slice to slice
		yield return new object[] { 2, 1 };

		// Promote to higher area, overflow to slice
		yield return new object[] { 5, 3 };

		// Promote to higher area, slice to slice
		yield return new object[] { 3, 1 };
	}

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
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.stack.promote",
				Window = windows[focusedWindowIdx],
				Payload = windows[focusedWindowIdx]
			}
		);

		IWindowState[] afterStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Equal(windows[expectedWindowIdx], afterStates[focusedWindowIdx].Window);
		Assert.Equal(windows[focusedWindowIdx], afterStates[expectedWindowIdx].Window);
	}
	#endregion

	#region DemoteWindowInStack
	[Theory, AutoSubstituteData]
	public void DemoteWindowInStack_CannotFindWindow(IContext ctx, SliceLayoutPlugin plugin, IWindow untrackedWindow)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.stack.demote",
				Window = untrackedWindow,
				Payload = untrackedWindow
			}
		);

		IWindowState[] afterStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}

	public static IEnumerable<object[]> DemoteWindowInStack_Data()
	{
		// Already lowest area
		yield return new object[] { 5, 5 };

		// Demote to lower area, slice to slice
		yield return new object[] { 1, 2 };

		// Demote to lower area, slice to overflow
		yield return new object[] { 3, 4 };

		// Demote to lower area, slice to slice
		yield return new object[] { 1, 2 };
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(DemoteWindowInStack_Data))]
	public void DemoteWindowInStack(int focusedWindowIdx, int expectedWindowIdx, IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.stack.demote",
				Window = windows[focusedWindowIdx],
				Payload = windows[focusedWindowIdx]
			}
		);

		IWindowState[] afterStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Equal(windows[expectedWindowIdx], afterStates[focusedWindowIdx].Window);
		Assert.Equal(windows[focusedWindowIdx], afterStates[expectedWindowIdx].Window);
	}
	#endregion

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_UnknownAction(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		IWindowState[] beforeStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		sut = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow>()
			{
				Name = "whim.slice_layout.stack.unknown",
				Window = windows[0],
				Payload = windows[0]
			}
		);

		IWindowState[] afterStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}
}
