using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.Bar.Tests;

public class BarLayoutEngineTests
{
	private class Wrapper
	{
		public Mock<ILayoutEngine> InnerLayoutEngine { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
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

			Monitor.SetupGet(m => m.ScaleFactor).Returns(100);
		}
	}

	[Fact]
	public void Count()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		wrapper.InnerLayoutEngine.SetupGet(ile => ile.Count).Returns(5);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;

		Mock<ILayoutEngine> addWindowResult = new();
		wrapper.InnerLayoutEngine.Setup(ile => ile.AddWindow(window)).Returns(addWindowResult.Object);
		addWindowResult.Setup(ile => ile.AddWindow(window)).Returns(addWindowResult.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;

		wrapper.InnerLayoutEngine.Setup(ile => ile.ContainsWindow(window)).Returns(true);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;
		Direction direction = Direction.Left;

		// When
		engine.FocusWindowInDirection(direction, window);

		// Then
		wrapper.InnerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window), Times.Once);
	}

	[Fact]
	public void GetFirstWindow()
	{
		// Given
		Wrapper wrapper = new();
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;

		wrapper.InnerLayoutEngine.Setup(ile => ile.GetFirstWindow()).Returns(window);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		Mock<ILayoutEngine> moveWindowEdgesInDirectionResult = new();
		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window))
			.Returns(moveWindowEdgesInDirectionResult.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window))
			.Returns(wrapper.InnerLayoutEngine.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;
		IPoint<double> point = new Point<double>();

		Mock<ILayoutEngine> moveWindowToPointResult = new();
		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowToPoint(window, point))
			.Returns(moveWindowToPointResult.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;
		IPoint<double> point = new Point<double>();

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.MoveWindowToPoint(window, point))
			.Returns(wrapper.InnerLayoutEngine.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;

		Mock<ILayoutEngine> removeWindowResult = new();
		wrapper.InnerLayoutEngine.Setup(ile => ile.RemoveWindow(window)).Returns(removeWindowResult.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);

		IWindow window = new Mock<IWindow>().Object;

		wrapper.InnerLayoutEngine.Setup(ile => ile.RemoveWindow(window)).Returns(wrapper.InnerLayoutEngine.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);
		IWindow window = new Mock<IWindow>().Object;
		Direction direction = Direction.Left;

		Mock<ILayoutEngine> swapWindowInDirectionResult = new();
		wrapper.InnerLayoutEngine
			.Setup(ile => ile.SwapWindowInDirection(direction, window))
			.Returns(swapWindowInDirectionResult.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);
		IWindow window = new Mock<IWindow>().Object;
		Direction direction = Direction.Left;

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.SwapWindowInDirection(direction, window))
			.Returns(wrapper.InnerLayoutEngine.Object);

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
		BarLayoutEngine engine = new(wrapper.BarConfig, wrapper.InnerLayoutEngine.Object);
		IWindow window1 = new Mock<IWindow>().Object;
		IWindow window2 = new Mock<IWindow>().Object;

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

		wrapper.InnerLayoutEngine
			.Setup(ile => ile.DoLayout(expectedGivenLocation, wrapper.Monitor.Object))
			.Returns(expectedWindowStates);

		// When
		IWindowState[] layout = engine.DoLayout(wrapper.Location, wrapper.Monitor.Object).ToArray();

		// Then
		Assert.Equal(2, layout.Length);
		wrapper.InnerLayoutEngine.Verify(
			ile => ile.DoLayout(expectedGivenLocation, wrapper.Monitor.Object),
			Times.Once
		);
		layout.Should().BeEquivalentTo(expectedWindowStates);
	}
}
