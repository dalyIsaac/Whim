using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.SliceLayout.Tests;

public class SliceLayoutEngineTests
{
	private static readonly LayoutEngineIdentity identity = new();
	private static readonly IRectangle<int> primaryMonitorBounds = new Rectangle<int>(0, 0, 100, 100);

	[Theory, AutoSubstituteData]
	public void Name(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		string name = sut.Name;

		// Then
		Assert.Equal("Slice", name);
	}

	[Theory, AutoSubstituteData]
	public void Name_Changed(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout())
		{
			Name = "Paradise Shelduck",
		};

		// When
		sut = sut.AddWindow(window);

		// Then
		Assert.Equal("Paradise Shelduck", sut.Name);
	}

	[Theory]
	[InlineAutoSubstituteData(0, 0)]
	[InlineAutoSubstituteData(1, 1)]
	[InlineAutoSubstituteData(5, 0)]
	[InlineAutoSubstituteData(0, 5)]
	public void Count(int windowCount, int minimizedWindowCount, IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		foreach (IWindow window in Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()))
		{
			sut = sut.AddWindow(window);
		}

		foreach (IWindow window in Enumerable.Range(0, minimizedWindowCount).Select(_ => Substitute.For<IWindow>()))
		{
			sut = sut.MinimizeWindowStart(window);
		}

		// Then
		Assert.Equal(windowCount + minimizedWindowCount, sut.Count);
	}

	#region AddWindow
	[Theory, AutoSubstituteData]
	public void AddWindow(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		ILayoutEngine result = sut.AddWindow(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.True(result.ContainsWindow(window));
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowAlreadyAdded(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given a window has been added
		ILayoutEngine sut = new SliceLayoutEngine(
			ctx,
			plugin,
			identity,
			SampleSliceLayouts.CreateNestedLayout()
		).AddWindow(window);

		// When the window is added again
		ILayoutEngine result = sut.AddWindow(window);

		// Then nothing changes
		Assert.Same(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_Minimized(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		sut = sut.MinimizeWindowStart(window);
		ILayoutEngine result = sut.AddWindow(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.True(result.ContainsWindow(window));
	}
	#endregion

	#region RemoveWindow
	[Theory]
	[InlineAutoSubstituteData(0, 1)]
	[InlineAutoSubstituteData(1, 0)]
	[InlineAutoSubstituteData(5, 3)]
	public void RemoveWindow(int addCount, int removeCount, IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		IWindow[] windows = Enumerable.Range(0, addCount).Select(_ => Substitute.For<IWindow>()).ToArray();

		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		// When
		foreach (IWindow window in windows.Take(removeCount))
		{
			sut = sut.RemoveWindow(window);
		}

		// Then
		Assert.Equal(Math.Max(0, addCount - removeCount), sut.Count);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_Minimized(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		sut = sut.MinimizeWindowStart(window);
		ILayoutEngine result = sut.RemoveWindow(window);

		// Then
		Assert.Equal(0, result.Count);
		Assert.NotSame(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowAlreadyRemoved(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given a window has been added
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout())
			.AddWindow(window)
			.RemoveWindow(window);

		// When the window is added again
		ILayoutEngine result = sut.RemoveWindow(window);

		// Then nothing changes
		Assert.Same(sut, result);
	}
	#endregion

	#region ContainsWindow
	[Theory, AutoSubstituteData]
	public void ContainsWindow_True(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		sut = sut.AddWindow(window);

		// When
		bool contains = sut.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_False(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		bool contains = sut.ContainsWindow(window);

		// Then
		Assert.False(contains);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_MinimizedWindow(
		IContext ctx,
		SliceLayoutPlugin plugin,
		IWindow window,
		IWindow minimizedWindow
	)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		sut = sut.AddWindow(window);
		ILayoutEngine result = sut.MinimizeWindowStart(minimizedWindow);

		// Then
		Assert.True(result.ContainsWindow(minimizedWindow));
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(2, result.Count);
	}

	#endregion

	[Theory, AutoSubstituteData]
	public void GetFirstWindow(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		// When
		IWindow? firstWindow = sut.GetFirstWindow();

		// Then
		Assert.Equal(windows[0], firstWindow);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_Empty(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		IWindow? firstWindow = sut.GetFirstWindow();

		// Then
		Assert.Null(firstWindow);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();

		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		// When
		IWindowState[] beforeStates = sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>()).ToArray();
		sut = sut.MoveWindowEdgesInDirection(Direction.Right, new Point<double>(0.5, 0.5), windows[0]);
		IWindowState[] afterStates = sut.DoLayout(primaryMonitorBounds, Substitute.For<IMonitor>()).ToArray();

		// Then
		beforeStates.Should().BeEquivalentTo(afterStates);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout(IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		IWindow[] windows = Enumerable.Range(0, 6).Select(_ => Substitute.For<IWindow>()).ToArray();
		IWindow[] minimizedWindows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();

		int third = 100 / 3;
		IRectangle<int> rectangle = new Rectangle<int>(0, 0, 100, 100);
		ParentArea area = SliceLayouts.CreateMultiColumnArea([2, 1, 0]);

		IWindowState[] expectedWindowStates =
		[
			new WindowState()
			{
				Rectangle = new Rectangle<int>(0, 0, third, 50),
				Window = windows[0],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(0, 50, third, 50),
				Window = windows[1],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(third, 0, third, 100),
				Window = windows[2],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(2 * third, 0, third, third),
				Window = windows[3],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(2 * third, third, third, third),
				Window = windows[4],
				WindowSize = WindowSize.Normal,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(2 * third, 2 * third, third, third),
				Window = windows[5],
				WindowSize = WindowSize.Normal,
			},
			// Minimized windows
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[0],
				WindowSize = WindowSize.Minimized,
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = minimizedWindows[1],
				WindowSize = WindowSize.Minimized,
			},
		];

		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, area);

		// When
		foreach (IWindow window in windows)
		{
			sut = sut.AddWindow(window);
		}

		foreach (IWindow window in minimizedWindows)
		{
			sut = sut.MinimizeWindowStart(window);
		}

		IWindowState[] windowStates = sut.DoLayout(rectangle, Substitute.For<IMonitor>()).ToArray();

		// Then
		windowStates.Should().BeEquivalentTo(expectedWindowStates);
	}

	#region MinimizeWindowStart
	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		ILayoutEngine result = sut.MinimizeWindowStart(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.NotSame(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowAlreadyAdded(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.MinimizeWindowStart(window);

		// Then
		Assert.Equal(1, sut.Count);
		Assert.NotSame(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowAlreadyMinimized(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		sut = sut.MinimizeWindowStart(window);

		// When
		ILayoutEngine result = sut.MinimizeWindowStart(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.Same(sut, result);
	}
	#endregion

	#region MinimizeWindowEnd
	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		sut = sut.AddWindow(window);
		sut = sut.MinimizeWindowStart(window);

		// When
		ILayoutEngine result = sut.MinimizeWindowEnd(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.NotSame(sut, result);
		Assert.True(result.ContainsWindow(window));
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_WindowAlreadyAdded(IContext ctx, SliceLayoutPlugin plugin, IWindow window)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.MinimizeWindowEnd(window);

		// Then
		Assert.Equal(1, result.Count);
		Assert.Same(sut, result);
		Assert.True(result.ContainsWindow(window));
	}
	#endregion
}
