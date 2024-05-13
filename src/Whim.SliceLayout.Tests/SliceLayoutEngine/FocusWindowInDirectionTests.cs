using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class FocusWindowInDirectionTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static TheoryData<ParentArea, int, Direction, int, int> FocusWindowInDirection_Data =>
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
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 3, 4 }
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(FocusWindowInDirection_Data))]
	public void FocusWindowInDirection(
		ParentArea parentArea,
		int windowCount,
		Direction direction,
		int focusedWindowIdx,
		int expectedWindowIdx,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, parentArea);
		IWindow[] windows = Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		ILayoutEngine newEngine1 = sut.FocusWindowInDirection(direction, windows[focusedWindowIdx]);
		ILayoutEngine newEngine2 = sut.FocusWindowInDirection(direction, windows[focusedWindowIdx]);

		// Then
		windows[expectedWindowIdx].Received(2).Focus();
		Assert.Same(sut, newEngine1);
		Assert.Same(newEngine1, newEngine2);
	}

	public static TheoryData<ParentArea, int, Direction, int> FocusWindowInDirection_NoWindowInDirection_Data =>
		new()
		{
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 1 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 2 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 0 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 1 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 5 }
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(FocusWindowInDirection_NoWindowInDirection_Data))]
	public void FocusWindowInDirection_NoWindowInDirection(
		ParentArea parentArea,
		int windowCount,
		Direction direction,
		int focusedWindowIdx,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, parentArea);
		IWindow[] windows = Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut.FocusWindowInDirection(direction, windows[focusedWindowIdx]);

		// Then
		foreach (IWindow window in windows)
		{
			window.DidNotReceive().Focus();
		}
	}
}
