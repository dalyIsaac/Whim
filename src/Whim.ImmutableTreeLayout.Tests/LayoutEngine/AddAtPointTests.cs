using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class AddAtPointTests
{
	[Fact]
	public void AddAtPoint_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window.Object, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddAtPoint_RootIsNull_AddPhantom()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsPhantom(window.Object);
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window.Object, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddAtPoint_RootIsPhantomNode()
	{
		// Given
		Mock<IWindow> phantomWindow = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsPhantom(phantomWindow.Object);

		Mock<IWindow> window = new();
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			phantomWindow.Object
		);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window.Object, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window.Object));
		Assert.Equal(1, result.Count);
	}

	[Fact]
	public void AddAtPoint_RootIsWindowNode_Right()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
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
	public void AddAtPoint_RootIsWindowNode_Down()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.7 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
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
	public void AddAtPoint_RootIsWindowNode_Left()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);

		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
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
					},
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
					}
				}
			);
	}

	[Fact]
	public void AddAtPoint_RootIsWindowNode_Up()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
			window1.Object
		);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.3 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window2.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
				new IWindowState[]
				{
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
					},
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
					}
				}
			);
	}

	[Fact]
	public void AddAtPoint_RootIsSplitNode_DoesNotContainPoint()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		IPoint<double> point = new Point<double>() { X = 1.7, Y = 0.5 };

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window3.Object, point);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.False(result.Contains(window3.Object));
		Assert.Equal(2, result.Count);
	}

	[Fact]
	public void AddAtPoint_RootIsSplitNode_AddInDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
		{
			AddNodeDirection = Direction.Right
		}
			.Add(window1.Object)
			.Add(window2.Object);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window3.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
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
						Window = window2.Object,
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
						Window = window3.Object,
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
	public void AddAtPoint_RootIsSplitNode_AddInDifferentDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1.Object);
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
		{
			AddNodeDirection = Direction.Right
		}
			.Add(window1.Object)
			.Add(window2.Object);

		IPoint<double> point = new Point<double>() { X = 0.75, Y = 0.8 };

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };
		Mock<IMonitor> monitor = new();

		// When
		IImmutableLayoutEngine result = engine.AddAtPoint(window3.Object, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.Contains(window1.Object));
		Assert.True(result.Contains(window2.Object));
		Assert.True(result.Contains(window3.Object));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.BeEquivalentTo(
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
}
