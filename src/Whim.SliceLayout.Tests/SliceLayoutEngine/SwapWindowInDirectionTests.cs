using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SwapWindowInDirectionTests
{
	private static readonly LayoutEngineIdentity identity = new();
	private static readonly IRectangle<int> primaryMonitorBounds = new Rectangle<int>(0, 0, 100, 100);
	private static readonly IMonitor primaryMonitor = StoreTestUtils.CreateMonitor();

	public static TheoryData<ParentArea, int, Direction, int, int> SwapWindowInDirection_Swap_Data =>
		new()
		{
			// Nested, share grandparent, right
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 1, 4 },
			// Nested, share grandparent, left
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 4, 1 },
			// Same slice, down
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 0, 1 },
			// Same slice, up
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 3, 2 },
			// Last overflow window, top left
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up | Direction.Left, 5, 1 },
			// Slice 1, down across slices
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 3, 4 },
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(SwapWindowInDirection_Swap_Data))]
	public void SwapWindowInDirection_Swap(
		ParentArea parentArea,
		int windowCount,
		Direction direction,
		int focusedWindowIdx,
		int targetWindowIdx,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, parentArea);
		IWindow[] windows = [.. Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.SwapWindowInDirection(direction, windows[focusedWindowIdx]);
		IWindowState[] windowStates = [.. sut.DoLayout(primaryMonitorBounds, primaryMonitor)];

		// Then
		Assert.Equal(windows[focusedWindowIdx], windowStates[targetWindowIdx].Window);
		Assert.Equal(windows[targetWindowIdx], windowStates[focusedWindowIdx].Window);
	}

	public static TheoryData<ParentArea, int, Direction, int, int, int> SwapWindowInDirection_Rotate_Data =>
		new()
		{
			// Nested, share grandparent, right
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 1, 4, 3 },
			// Nested, share grandparent, left
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 4, 1, 2 },
			// Same slice, down
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 0, 1, 0 },
			// Same slice, up
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 3, 2, 3 },
			// Last overflow window, top left
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up | Direction.Left, 5, 1, 2 },
			// Slice 1, down across slices
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 3, 4, 3 },
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(SwapWindowInDirection_Rotate_Data))]
	public void SwapWindowInDirection_Rotate(
		ParentArea parentArea,
		int windowCount,
		Direction direction,
		int focusedWindowIdx,
		int targetWindowIdx,
		int targetWindowLocationIdx,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, parentArea);
		IWindow[] windows = [.. Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>())];

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		plugin.WindowInsertionType.Returns(WindowInsertionType.Rotate);
		sut = sut.SwapWindowInDirection(direction, windows[focusedWindowIdx]);
		IWindowState[] windowStates = [.. sut.DoLayout(primaryMonitorBounds, primaryMonitor)];

		// Then
		Assert.Equal(windows[focusedWindowIdx], windowStates[targetWindowIdx].Window);
		Assert.Equal(windows[targetWindowIdx], windowStates[targetWindowLocationIdx].Window);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_NoWindowInDirection(IContext ctx, ISliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		IWindowState[] beforeStates = [.. sut.DoLayout(primaryMonitorBounds, primaryMonitor)];

		sut = sut.SwapWindowInDirection(Direction.Up, window);

		IWindowState[] afterStates = [.. sut.DoLayout(primaryMonitorBounds, primaryMonitor)];

		// Then
		Assert.Equal(beforeStates, afterStates);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowNotFound(IContext ctx, ISliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		ILayoutEngine resultSut = sut.SwapWindowInDirection(Direction.Up, window);

		// Then
		Assert.Equal(sut, resultSut);
	}
}
