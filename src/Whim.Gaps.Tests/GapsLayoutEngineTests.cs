using FluentAssertions;
using NSubstitute;
using Whim.FloatingLayout;
using Whim.TestUtils;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsLayoutEngineTests
{
	private static readonly LayoutEngineIdentity _identity = new();

	public static TheoryData<GapsConfig, IWindow[], int, IWindowState[]> DoLayout_Data
	{
		get
		{
			TheoryData<GapsConfig, IWindow[], int, IWindowState[]> data = new();

			IWindow window1 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig() { OuterGap = 10, InnerGap = 5 },
				new IWindow[] { window1 },
				100,
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 10 + 5,
							Y = 10 + 5,
							Width = 1920 - (10 * 2) - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2)
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			IWindow window2 = Substitute.For<IWindow>();
			IWindow window3 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig() { OuterGap = 10, InnerGap = 5 },
				new IWindow[] { window2, window3 },
				100,
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 10 + 5,
							Y = 10 + 5,
							Width = 960 - 10 - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2)
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 960 + 5,
							Y = 10 + 5,
							Width = 960 - 10 - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2)
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			IWindow window4 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				new IWindow[] { window4 },
				150,
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window4,
						Rectangle = new Rectangle<int>()
						{
							X = 15 + 7,
							Y = 15 + 7,
							Width = 1920 - (15 * 2) - (7 * 2),
							Height = 1080 - (15 * 2) - (7 * 2)
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			return data;
		}
	}

	[Theory]
	[MemberData(nameof(DoLayout_Data))]
	public void DoLayout(GapsConfig gapsConfig, IWindow[] windows, int scale, IWindowState[] expectedWindowStates)
	{
		// Given
		ILayoutEngine innerLayoutEngine = new ColumnLayoutEngine(_identity);

		foreach (IWindow w in windows)
		{
			innerLayoutEngine = innerLayoutEngine.AddWindow(w);
		}

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		Rectangle<int> rect =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};

		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.ScaleFactor.Returns(scale);

		// When
		IWindowState[] windowStates = gapsLayoutEngine.DoLayout(rect, monitor).ToArray();

		// Then
		windowStates.Should().Equal(expectedWindowStates);
	}

	public static TheoryData<GapsConfig, IWindow, Rectangle<int>, IWindowState[]> DoLayout_OutOfBoundsData
	{
		get
		{
			TheoryData<GapsConfig, IWindow, Rectangle<int>, IWindowState[]> data = new();

			// A window whose width returned by the layout engine as less than zero should not have the
			// gap applied in the x direction
			IWindow window1 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window1,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = -100,
					Height = 1080
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 1080
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose width returned by the layout engine as zero should not have the gap applied
			// in the x direction
			IWindow window2 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window2,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 0,
					Height = 1080
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 1080
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose width is less than the gap should not have the gap applied in the x direction
			IWindow window3 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window3,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 5,
					Height = 1080
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 5,
							Height = 1080
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose height returned by the layout engine as less than zero should not have the
			// gap applied in the y direction
			IWindow window4 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window4,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 1920,
					Height = -100
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window4,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 0
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose height returned by the layout engine as zero should not have the gap applied
			// in the y direction
			IWindow window5 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window5,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 1920,
					Height = 0
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window5,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 0
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose height is less than the gap should not have the gap applied in the y direction
			IWindow window6 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window6,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 1920,
					Height = 5
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window6,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 5
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose width and height are less than the gap should not have the gap applied in
			// either direction
			IWindow window7 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window7,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 5,
					Height = 5
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window7,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 5,
							Height = 5
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			// A window whose width and height are zero should not have the gap applied in either direction
			IWindow window8 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				window8,
				new Rectangle<int>()
				{
					X = 5,
					Y = 5,
					Width = 0,
					Height = 0
				},
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window8,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 0
						},
						WindowSize = WindowSize.Normal
					}
				}
			);

			return data;
		}
	}

	[Theory]
	[MemberData(nameof(DoLayout_OutOfBoundsData))]
	public void DoLayout_OutOfBounds(
		GapsConfig gapsConfig,
		IWindow window,
		Rectangle<int> rect,
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		ILayoutEngine innerLayoutEngine = Substitute.For<ILayoutEngine>();
		innerLayoutEngine
			.DoLayout(rect, Arg.Any<IMonitor>())
			.Returns(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window,
						Rectangle = rect,
						WindowSize = WindowSize.Normal
					}
				}
			);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		IWindowState[] windowStates = gapsLayoutEngine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then
		windowStates.Should().Equal(expectedWindowStates);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_WithFloatingLayoutEngine(GapsConfig gapsConfig, IWindow window)
	{
		// Input
		Rectangle<int> rect = new(0, 0, -300, -300);
		IWindowState[] inputWindowStates =
		{
			new WindowState()
			{
				Window = window,
				Rectangle = rect,
				WindowSize = WindowSize.Normal
			}
		};

		// Given
		ILayoutEngine innerLayoutEngine = Substitute.For<ILayoutEngine>();
		innerLayoutEngine
			.GetLayoutEngine<FloatingLayoutEngine>()
			.Returns(new FloatingLayoutEngine(Substitute.For<IContext>(), _identity));
		innerLayoutEngine.DoLayout(rect, Arg.Any<IMonitor>()).Returns(inputWindowStates);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		IWindowState[] outputWindowStates = gapsLayoutEngine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then
		outputWindowStates.Should().Equal(inputWindowStates);
	}

	[Theory, AutoSubstituteData]
	public void Count(ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.Count.Returns(5);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When

		// Then
		Assert.Equal(5, gapsLayoutEngine.Count);
		_ = innerLayoutEngine.Received(1).Count;
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.ContainsWindow(Arg.Any<IWindow>()).Returns(true);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		bool containsWindow = gapsLayoutEngine.ContainsWindow(window);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.Received(1).ContainsWindow(window);
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);
		innerLayoutEngine.FocusWindowInDirection(direction, window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.FocusWindowInDirection(direction, window);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_NotSame(
		IWindow window,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.FocusWindowInDirection(direction, window).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.FocusWindowInDirection(direction, window);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).FocusWindowInDirection(direction, window);
	}

	#region Add
	[Theory, AutoSubstituteData]
	public void Add_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.AddWindow(window).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.AddWindow(window);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData]
	public void Add_NotSame(IWindow window, ILayoutEngine innerLayoutEngine, ILayoutEngine newInnerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.AddWindow(window).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.AddWindow(window);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).AddWindow(window);
	}
	#endregion

	#region Remove
	[Theory, AutoSubstituteData]
	public void Remove_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.RemoveWindow(window).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.RemoveWindow(window);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}

	[Theory, AutoSubstituteData]
	public void Remove_NotSame(IWindow window, ILayoutEngine innerLayoutEngine, ILayoutEngine newInnerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.RemoveWindow(window).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.RemoveWindow(window);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).RemoveWindow(window);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_NotSame(
		IWindow window,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).MoveWindowEdgesInDirection(direction, deltas, window);
	}
	#endregion

	#region MoveWindowToPoint
	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		IPoint<double> point = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.MoveWindowToPoint(window, point).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowToPoint(window, point);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, point);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_NotSame(
		IWindow window,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		IPoint<double> point = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.MoveWindowToPoint(window, point).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).MoveWindowToPoint(window, point);
	}
	#endregion

	#region SwapWindowInDirection
	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_Same(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.SwapWindowInDirection(direction, window);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_NotSame(
		IWindow window,
		ILayoutEngine innerLayoutEngine,
		ILayoutEngine newInnerLayoutEngine
	)
	{
		// Given
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.SwapWindowInDirection(direction, window).Returns(newInnerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.SwapWindowInDirection(direction, window);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).SwapWindowInDirection(direction, window);
	}
	#endregion

	[Theory, AutoSubstituteData]
	public void GetFirstWindow(IWindow window, ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		innerLayoutEngine.GetFirstWindow().Returns(window);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		IWindow? firstWindow = gapsLayoutEngine.GetFirstWindow();

		// Then
		Assert.Same(window, firstWindow);
		innerLayoutEngine.Received(1).GetFirstWindow();
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_NotSame(ILayoutEngine innerLayoutEngine, ILayoutEngine performCustomActionResult)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };

		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = Substitute.For<IWindow>()
			};
		innerLayoutEngine.PerformCustomAction(action).Returns(performCustomActionResult);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.PerformCustomAction(action);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).PerformCustomAction(action);
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_Same(ILayoutEngine innerLayoutEngine)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };

		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = Substitute.For<IWindow>()
			};
		innerLayoutEngine.PerformCustomAction(action).Returns(innerLayoutEngine);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.PerformCustomAction(action);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		Assert.IsType<GapsLayoutEngine>(newLayoutEngine);
		innerLayoutEngine.Received(1).PerformCustomAction(action);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_NotSame(ILayoutEngine innerLayoutEngine, ILayoutEngine minimizeWindowStartResult)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		GapsLayoutEngine engine = new(gapsConfig, innerLayoutEngine);
		IWindow window = Substitute.For<IWindow>();
		innerLayoutEngine.MinimizeWindowStart(window).Returns(minimizeWindowStartResult);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.IsType<GapsLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_Same(ILayoutEngine innerLayoutEngine, IWindow window)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		GapsLayoutEngine engine = new(gapsConfig, innerLayoutEngine);
		innerLayoutEngine.MinimizeWindowStart(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then
		Assert.Same(engine, newEngine);
		Assert.IsType<GapsLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_NotSame(ILayoutEngine innerLayoutEngine, ILayoutEngine minimizeWindowEndResult)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		GapsLayoutEngine engine = new(gapsConfig, innerLayoutEngine);
		IWindow window = Substitute.For<IWindow>();
		innerLayoutEngine.MinimizeWindowEnd(window).Returns(minimizeWindowEndResult);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.IsType<GapsLayoutEngine>(newEngine);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_Same(ILayoutEngine innerLayoutEngine, IWindow window)
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		GapsLayoutEngine engine = new(gapsConfig, innerLayoutEngine);
		innerLayoutEngine.MinimizeWindowEnd(window).Returns(innerLayoutEngine);

		// When
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then
		Assert.Same(engine, newEngine);
		Assert.IsType<GapsLayoutEngine>(newEngine);
	}
}
