using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class SwapWindowInDirectionTests
{
	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_RootIsNull(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window);

		// Then
		Assert.Same(engine, result);
		Assert.False(result.ContainsWindow(window));
		Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_NoAdjacentNodeInDirection(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Left, window1);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.Same(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window1,
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
				Window = window2,
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
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_SameParent(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window2,
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
				Window = window1,
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
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_DifferentParents(
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.MoveWindowToPoint(window3, new Point<double>() { X = 0.75, Y = 0.75 });

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.Right, window1);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = window2,
				Location = new Location<int>() { Width = 50, Height = 100 },
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window1,
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
				Window = window3,
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
			.Equal(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_Diagonal(
		IWindow topLeft,
		IWindow topRight,
		IWindow bottomLeft,
		IWindow bottomRight,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(topLeft)
			.AddWindow(topRight)
			.MoveWindowToPoint(bottomLeft, new Point<double>() { X = 0.25, Y = 0.9 })
			.MoveWindowToPoint(bottomRight, new Point<double>() { X = 0.75, Y = 0.9 });

		ILocation<int> location = new Location<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.SwapWindowInDirection(Direction.LeftUp, bottomRight);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);

		new IWindowState[]
		{
			new WindowState()
			{
				Window = bottomRight,
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
				Window = bottomLeft,
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
				Window = topRight,
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
				Window = topLeft,
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
			.Equal(windowStates);
	}
}
