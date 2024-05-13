using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class MoveWindowToPointTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public static TheoryData<ParentArea, int, int, IPoint<double>, int> MoveWindowToPoint_Data =>
		new()
		{
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(0.7, 0.7), 4 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(0.3, 0.3), 0 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(0.3, 0.7), 1 },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 3, new Point<double>(0, 0), 0 }
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowToPoint_Data))]
	public void MoveWindowToPoint(
		ParentArea parentArea,
		int windowCount,
		int windowIdx,
		IPoint<double> point,
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

		sut = sut.MoveWindowToPoint(windows[windowIdx], point);
		IWindowState[] windowStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Equal(windows[expectedWindowIdx], windowStates[windowIdx].Window);
	}

	public static TheoryData<ParentArea, int, IPoint<double>> MoveWindowToPoint_WindowDoesNotMove_Data =>
		new()
		{
			{ SampleSliceLayouts.CreateNestedLayout(), 4, new Point<double>(0.7, 0.7) },
			{ SampleSliceLayouts.CreateNestedLayout(), 0, new Point<double>(0.3, 0.3) },
			{ SampleSliceLayouts.CreateNestedLayout(), 1, new Point<double>(0.3, 0.7) },
			{ SampleSliceLayouts.CreateNestedLayout(), 0, new Point<double>(0, 0) }
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowToPoint_WindowDoesNotMove_Data))]
	public void MoveWindowToPoint_WindowDoesNotMove(
		ParentArea parentArea,
		int windowIdx,
		IPoint<double> point,
		IContext ctx,
		ISliceLayoutPlugin plugin
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, parentArea);
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		sut = sut.MoveWindowToPoint(windows[windowIdx], point);
		IWindowState[] windowStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then the window should not have moved
		Assert.Equal(windows[windowIdx], windowStates[windowIdx].Window);
	}

	public static TheoryData<ParentArea, int, int, IPoint<double>> MoveWindowToPoint_InvalidPoint_Data =>
		new()
		{
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(-0.1, 0.7) },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(0.7, -0.1) },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(1.1, 0.7) },
			{ SampleSliceLayouts.CreateNestedLayout(), 6, 1, new Point<double>(0.7, 1.1) }
		};

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowToPoint_InvalidPoint_Data))]
	public void MoveWindowToPoint_InvalidPoint(
		ParentArea parentArea,
		int windowCount,
		int windowIdx,
		IPoint<double> point,
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

		sut = sut.MoveWindowToPoint(windows[windowIdx], point);
		IWindowState[] windowStates = sut.DoLayout(new Rectangle<int>(0, 0, 100, 100), Substitute.For<IMonitor>())
			.ToArray();

		// Then the window should not have moved
		Assert.Equal(windows[windowIdx], windowStates[windowIdx].Window);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_WindowNotFound(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		ILayoutEngine resultSut = sut.MoveWindowToPoint(window, new Point<double>(0.5, 0.5));

		// Then the window should have been added
		Assert.NotSame(sut, resultSut);
		Assert.Equal(1, resultSut.Count);
	}
}
