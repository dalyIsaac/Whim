using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SwapWindowInDirectionTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static IEnumerable<object[]> SwapWindowInDirection_Swap_Data()
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
		IWindow[] windows = Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.SwapWindowInDirection(direction, windows[focusedWindowIdx]);
		IWindowState[] windowStates = sut.DoLayout(
				new Rectangle<int>(0, 0, 100, 100),
				ctx.MonitorManager.PrimaryMonitor
			)
			.ToArray();

		// Then
		Assert.Equal(windows[focusedWindowIdx], windowStates[targetWindowIdx].Window);
		Assert.Equal(windows[targetWindowIdx], windowStates[focusedWindowIdx].Window);
	}

	public static IEnumerable<object[]> SwapWindowInDirection_Rotate_Data()
	{
		// Nested, share grandparent, right
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Right, 1, 4, 3 };

		// Nested, share grandparent, left
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Left, 4, 1, 2 };

		// Same slice, down
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 0, 1, 0 };

		// Same slice, up
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Up, 3, 2, 3 };

		// Last overflow window, top left
		yield return new object[]
		{
			SampleSliceLayouts.CreateNestedLayout(),
			6,
			Direction.Up | Direction.Left,
			5,
			1,
			2
		};

		// Slice 1, down across slices
		yield return new object[] { SampleSliceLayouts.CreateNestedLayout(), 6, Direction.Down, 3, 4, 3 };
	}

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
		IWindow[] windows = Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		plugin.WindowInsertionType.Returns(WindowInsertionType.Rotate);
		sut = sut.SwapWindowInDirection(direction, windows[focusedWindowIdx]);
		IWindowState[] windowStates = sut.DoLayout(
				new Rectangle<int>(0, 0, 100, 100),
				ctx.MonitorManager.PrimaryMonitor
			)
			.ToArray();

		// Then
		Assert.Equal(windows[focusedWindowIdx], windowStates[targetWindowIdx].Window);
		Assert.Equal(windows[targetWindowIdx], windowStates[targetWindowLocationIdx].Window);
	}
}
