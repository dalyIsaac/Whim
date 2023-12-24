using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class FocusWindowInDirectionTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static IEnumerable<object[]> FocusWindowInDirection_Data()
	{
		// Nested, share grandparent, right
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 1, 4 };

		// Nested, share grandparent, left
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 4, 1 };

		// Same slice, down
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 0, 1 };

		// Same slice, up
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 3, 2 };

		// Last overflow window, top left
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up | Direction.Left, 5, 1 };

		// Slice 1, down across slices
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 3, 4 };
	}

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

		sut.FocusWindowInDirection(direction, windows[focusedWindowIdx]);

		// Then
		windows[expectedWindowIdx].Received(1).Focus();
	}

	public static IEnumerable<object[]> FocusWindowInDirection_NoWindowInDirection_Data()
	{
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 1 };
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 2 };
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 0 };
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 1 };
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 5 };
	}

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
