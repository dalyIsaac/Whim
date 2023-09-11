using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Whim.Bar.Tests;

public class BarLayoutEngineTests
{
	private class Wrapper
	{
		public ILayoutEngine InnerLayoutEngine { get; } = Substitute.For<ILayoutEngine>();
		public IMonitor Monitor { get; } = Substitute.For<IMonitor>();
		public ILocation<int> Location { get; } = new Location<int>() { Width = 100, Height = 100 };
		public BarConfig BarConfig { get; }

		public Wrapper()
		{
			BarConfig = new(
				leftComponents: new List<BarComponent>(),
				centerComponents: new List<BarComponent>(),
				rightComponents: new List<BarComponent>()
			)
			{
				Height = 30
			};

			Monitor.ScaleFactor.Returns(100);
		}
	}

	[Fact]
	public void Count()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		wrapper.InnerLayoutEngine.Count.Returns(5);

		// When
		int count = engine.Count;

		// Then
		Assert.Equal(5, count);
	}

	[Fact]
	public void AddWindow()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();

		ILayoutEngine addWindowResult = Substitute.For<ILayoutEngine>();
		wrapper.InnerLayoutEngine.AddWindow(window).Returns(addWindowResult);
		addWindowResult.AddWindow(window).Returns(addWindowResult);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Same(newEngine, newEngine2);
	}

	[Fact]
	public void ContainsWindow()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();

		wrapper.InnerLayoutEngine.ContainsWindow(window).Returns(true);

		// When
		bool contains = engine.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void FocusWindowInDirection()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();
		Direction direction = Direction.Left;

		// When
		engine.FocusWindowInDirection(direction, window);

		// Then
		wrapper.InnerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
	}

	[Fact]
	public void GetFirstWindow()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();

		wrapper.InnerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_NotSame()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		ILayoutEngine moveWindowEdgesInDirectionResult = Substitute.For<ILayoutEngine>();
		wrapper.InnerLayoutEngine
			.MoveWindowEdgesInDirection(direction, deltas, window)
			.Returns(moveWindowEdgesInDirectionResult);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_Same()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		wrapper.InnerLayoutEngine
			.MoveWindowEdgesInDirection(direction, deltas, window)
			.Returns(wrapper.InnerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Fact]
	public void MoveWindowToPoint_NotSame()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();
		IPoint<double> point = new Point<double>();

		ILayoutEngine moveWindowToPointResult = Substitute.For<ILayoutEngine>();
		wrapper.InnerLayoutEngine.MoveWindowToPoint(window, point).Returns(moveWindowToPointResult);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Fact]
	public void MoveWindowToPoint_Same()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();
		IPoint<double> point = new Point<double>();

		wrapper.InnerLayoutEngine.MoveWindowToPoint(window, point).Returns(wrapper.InnerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Fact]
	public void RemoveWindow_NotSame()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();

		ILayoutEngine removeWindowResult = Substitute.For<ILayoutEngine>();
		wrapper.InnerLayoutEngine.RemoveWindow(window).Returns(removeWindowResult);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Fact]
	public void RemoveWindow_Same()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);

		IWindow window = Substitute.For<IWindow>();

		wrapper.InnerLayoutEngine.RemoveWindow(window).Returns(wrapper.InnerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Fact]
	public void SwapWindowInDirection_NotSame()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);
		IWindow window = Substitute.For<IWindow>();
		Direction direction = Direction.Left;

		ILayoutEngine swapWindowInDirectionResult = Substitute.For<ILayoutEngine>();
		wrapper.InnerLayoutEngine.SwapWindowInDirection(direction, window).Returns(swapWindowInDirectionResult);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Fact]
	public void SwapWindowInDirection_Same()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);
		IWindow window = Substitute.For<IWindow>();
		Direction direction = Direction.Left;

		wrapper.InnerLayoutEngine.SwapWindowInDirection(direction, window).Returns(wrapper.InnerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Fact]
	public void DoLayout()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine);
		IWindow window1 = Substitute.For<IWindow>();
		IWindow window2 = Substitute.For<IWindow>();

		IWindowState[] expectedWindowStates = new[]
		{
			new WindowState()
			{
				Window = window1,
				Location = new Location<int>()
				{
					Y = 30,
					Width = 50,
					Height = 70
				},
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = window2,
				Location = new Location<int>()
				{
					X = 50,
					Y = 30,
					Width = 50,
					Height = 70
				},
				WindowSize = WindowSize.Normal
			}
		};

		ILocation<int> expectedGivenLocation = new Location<int>()
		{
			Y = 30,
			Width = 100,
			Height = 70
		};

		wrapper.InnerLayoutEngine.DoLayout(expectedGivenLocation, wrapper.Monitor).Returns(expectedWindowStates);

		// When
		IWindowState[] layout = engine.DoLayout(wrapper.Location, wrapper.Monitor).ToArray();

		// Then
		Assert.Equal(2, layout.Length);
		wrapper.InnerLayoutEngine.Received(1).DoLayout(expectedGivenLocation, wrapper.Monitor);
		layout.Should().Equal(expectedWindowStates);
	}
}
