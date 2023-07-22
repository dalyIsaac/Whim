using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class SwapWindowInDirectionTests
{
	[Fact]
	public void SwapWindowInDirection_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		Mock<IWindow> window = new();
		TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

		// When
		IImmutableLayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window.Object);

		// Then
		Assert.Same(engine, result);
		Assert.False(result.Contains(window.Object));
		Assert.Equal(0, result.Count);
	}

	[Fact]
	public void SwapWindowInDirection_NoAdjacentNodeInDirection()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		Mock<IMonitor> monitor = new();
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		IImmutableLayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window1.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.Same(engine, result);

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
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Fact]
	public void SwapWindowInDirection_SameParent()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		LayoutEngineWrapper wrapper = new();
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

		Mock<IMonitor> monitor = new();
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		IImmutableLayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);

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
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Fact]
	public void SwapWindowInDirection_DifferentParents()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();
		LayoutEngineWrapper wrapper = new();
		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.AddAtPoint(window3.Object, new Point<double>() { X = 0.75, Y = 0.75 });

		Mock<IMonitor> monitor = new();
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		IImmutableLayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window2.Object,
				Location = new Location<int>() { Width = 50, Height = 100 },
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
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Fact]
	public void SwapWindowInDIrection_Diagonal()
	{
		// Given
		Mock<IWindow> topLeft = new();
		Mock<IWindow> topRight = new();
		Mock<IWindow> bottomLeft = new();
		Mock<IWindow> bottomRight = new();

		LayoutEngineWrapper wrapper = new();

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(topLeft.Object)
			.Add(topRight.Object)
			.AddAtPoint(bottomLeft.Object, new Point<double>() { X = 0.25, Y = 0.9 })
			.AddAtPoint(bottomRight.Object, new Point<double>() { X = 0.75, Y = 0.9 });

		Mock<IMonitor> monitor = new();
		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		IImmutableLayoutEngine result = engine.SwapWindowInDirection(Direction.LeftUp, bottomRight.Object);
		IWindowState[] windowStates = result.DoLayout(location, monitor.Object).ToArray();

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = bottomRight.Object,
				Location = new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 50,
					Height = 50
				},
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = topRight.Object,
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
				Window = bottomLeft.Object,
				Location = new Location<int>()
				{
					X = 0,
					Y = 50,
					Width = 50,
					Height = 50
				},
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = topLeft.Object,
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
			.Should()
			.BeEquivalentTo(windowStates);
	}
}
