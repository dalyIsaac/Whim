using AutoFixture.Xunit2;
using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Bar.Tests;

public class BarLayoutEngineTests
{
	private static BarLayoutEngine CreateSut(ILayoutEngine innerLayoutEngine) =>
		new(
			new BarConfig(
				leftComponents: new List<BarComponent>(),
				centerComponents: new List<BarComponent>(),
				rightComponents: new List<BarComponent>()
			)
			{
				Height = 30
			},
			innerLayoutEngine
		);

	[Theory, AutoSubstituteData]
	public void Count(ILayoutEngine innerLayoutEngine)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);
		innerLayoutEngine.Count.Returns(5);

		// When
		int count = engine.Count;

		// Then
		Assert.Equal(5, count);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow(ILayoutEngine innerLayoutEngine, ILayoutEngine addWindowResult, IWindow window)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.AddWindow(window).Returns(addWindowResult);
		addWindowResult.AddWindow(window).Returns(addWindowResult);

		// When
		ILayoutEngine newEngine = engine.AddWindow(window);
		ILayoutEngine newEngine2 = newEngine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Same(newEngine, newEngine2);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow(ILayoutEngine innerLayoutEngine, IWindow window)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.ContainsWindow(window).Returns(true);

		// When
		bool contains = engine.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection(ILayoutEngine innerLayoutEngine, IWindow window, [Frozen] Direction direction)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		// When
		engine.FocusWindowInDirection(direction, window);

		// Then
		innerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow(ILayoutEngine innerLayoutEngine, IWindow window)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.GetFirstWindow().Returns(window);

		// When
		IWindow? firstWindow = engine.GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_NotSame(
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine moveWindowEdgesResult,
		IWindow window,
		Direction direction,
		Point<double> deltas
	)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(moveWindowEdgesResult);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_Same(
		ILayoutEngine innerLayoutEngine,
		IWindow window,
		Direction direction,
		Point<double> deltas
	)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_NotSame(
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine moveWindowToPointResult,
		IWindow window,
		Point<double> point
	)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.MoveWindowToPoint(window, point).Returns(moveWindowToPointResult);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_Same(ILayoutEngine innerLayoutEngine, IWindow window, Point<double> point)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.MoveWindowToPoint(window, point).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_NotSame(ILayoutEngine innerLayoutEngine, ILayoutEngine removeWindowResult, IWindow window)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.RemoveWindow(window).Returns(removeWindowResult);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_Same(ILayoutEngine innerLayoutEngine, IWindow window)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.RemoveWindow(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.RemoveWindow(window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_NotSame(
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine swapWindowInDirectionResult,
		IWindow window,
		Direction direction
	)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(swapWindowInDirectionResult);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_Same(ILayoutEngine innerLayoutEngine, IWindow window, Direction direction)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(direction, window);

		// Then
		Assert.Same(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout(ILayoutEngine innerLayoutEngine, IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		monitor.ScaleFactor.Returns(100);
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		IWindowState[] expectedWindowStates = new[]
		{
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>()
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
				Rectangle = new Rectangle<int>()
				{
					X = 50,
					Y = 30,
					Width = 50,
					Height = 70
				},
				WindowSize = WindowSize.Normal
			}
		};

		Rectangle<int> expectedGivenRect =
			new()
			{
				Y = 30,
				Width = 100,
				Height = 70
			};

		innerLayoutEngine.DoLayout(expectedGivenRect, monitor).Returns(expectedWindowStates);

		// When
		IWindowState[] layout = engine.DoLayout(new Rectangle<int>() { Width = 100, Height = 100 }, monitor).ToArray();

		// Then
		Assert.Equal(2, layout.Length);
		innerLayoutEngine.Received(1).DoLayout(expectedGivenRect, monitor);
		layout.Should().Equal(expectedWindowStates);
	}

	[Theory]
	[InlineAutoSubstituteData(0)]
	[InlineAutoSubstituteData(-30)]
	public void DoLayout_HeightMustBeNonNegative(int height, ILayoutEngine innerLayoutEngine, IMonitor monitor)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);

		// When
		engine.DoLayout(new Rectangle<int>() { Width = 100, Height = height }, monitor);

		// Then
		innerLayoutEngine.Received(1).DoLayout(Arg.Is<Rectangle<int>>(l => l.Height == 0), monitor);
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_NotSame(ILayoutEngine innerLayoutEngine, ILayoutEngine performCustomActionResult)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = null
			};
		innerLayoutEngine.PerformCustomAction(action).Returns(performCustomActionResult);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.NotSame(engine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_Same(ILayoutEngine innerLayoutEngine)
	{
		// Given
		BarLayoutEngine engine = CreateSut(innerLayoutEngine);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = null
			};
		innerLayoutEngine.PerformCustomAction(action).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Same(engine, newEngine);
	}
}
