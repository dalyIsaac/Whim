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
			Name = "Paradise Shelduck"
		};

		// When
		sut = sut.AddWindow(window);

		// Then
		Assert.Equal("Paradise Shelduck", sut.Name);
	}

	[Theory]
	[InlineAutoSubstituteData(0)]
	[InlineAutoSubstituteData(1)]
	[InlineAutoSubstituteData(5)]
	public void Count(int windowCount, IContext ctx, SliceLayoutPlugin plugin)
	{
		// Given
		ILayoutEngine sut = new SliceLayoutEngine(ctx, plugin, identity, SampleSliceLayouts.CreateNestedLayout());

		// When
		foreach (IWindow window in Enumerable.Range(0, windowCount).Select(_ => Substitute.For<IWindow>()))
		{
			sut = sut.AddWindow(window);
		}

		// Then
		Assert.Equal(windowCount, sut.Count);
	}

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
}
