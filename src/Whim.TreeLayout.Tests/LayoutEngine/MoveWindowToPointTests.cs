using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MoveWindowToPointTests
{
	[Fact]
	public void MoveWindowToPoint_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window.Object, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsWindowNode_Right()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window1.Object);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsWindowNode_Down()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window1.Object);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.7 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsWindowNode_Left()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window1.Object);

		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsWindowNode_Up()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window1.Object);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.3 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsSplitNode_DoesNotContainPoint()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		IPoint<double> point = new Point<double>() { X = 1.7, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3.Object, point);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.False(result.ContainsWindow(window3.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsSplitNode_AddInDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.True(result.ContainsWindow(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Fact]
	public void MoveWindowToPoint_RootIsSplitNode_AddInDifferentDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		IPoint<double> point = new Point<double>() { X = 0.75, Y = 0.8 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1.Object));
		Assert.True(result.ContainsWindow(window2.Object));
		Assert.True(result.ContainsWindow(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3.Object,
						Location = new Location<int>()
						{
							X = 50,
							Y = 50,
							Width = 50,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[InlineData(0.25, 0.25)]
	[InlineData(0.25, 0.75)]
	[InlineData(0.75, 0.25)]
	[InlineData(0.75, 0.75)]
	[Theory]
	public void MoveWindowToPoint_AlreadyContainsWindow(double x, double y)
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window.Object);
		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window.Object);

		IPoint<double> point = new Point<double>() { X = x, Y = y };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window.Object));
		Assert.Single(windowStates);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window.Object,
						Location = new Location<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}
}
