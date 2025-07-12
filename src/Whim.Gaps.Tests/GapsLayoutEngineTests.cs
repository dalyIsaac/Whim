using FluentAssertions;
using NSubstitute;
using Whim.FloatingWindow;
using Whim.SliceLayout;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsLayoutEngineTests
{
	private static readonly LayoutEngineIdentity _identity = new();

	public static TheoryData<GapsConfig, IWindow[], int, IWindowState[]> DoLayout_Data
	{
		get
		{
			TheoryData<GapsConfig, IWindow[], int, IWindowState[]> data = [];

			IWindow window1 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig() { OuterGap = 10, InnerGap = 5 },
				[window1],
				100,
				[
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 10 + 5,
							Y = 10 + 5,
							Width = 1920 - (10 * 2) - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2),
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);

			IWindow window2 = Substitute.For<IWindow>();
			IWindow window3 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig() { OuterGap = 10, InnerGap = 5 },
				[window2, window3],
				100,
				[
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 10 + 5,
							Y = 10 + 5,
							Width = 960 - 10 - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2),
						},
						WindowSize = WindowSize.Normal,
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 960 + 5,
							Y = 10 + 5,
							Width = 960 - 10 - (5 * 2),
							Height = 1080 - (10 * 2) - (5 * 2),
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);

			IWindow window4 = Substitute.For<IWindow>();
			data.Add(
				new GapsConfig { OuterGap = 10, InnerGap = 5 },
				[window4],
				150,
				[
					new WindowState()
					{
						Window = window4,
						Rectangle = new Rectangle<int>()
						{
							X = 15 + 7,
							Y = 15 + 7,
							Width = 1920 - (15 * 2) - (7 * 2),
							Height = 1080 - (15 * 2) - (7 * 2),
						},
						WindowSize = WindowSize.Normal,
					},
				]
			);

			return data;
		}
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(DoLayout_Data))]
	public void DoLayout(
		GapsConfig gapsConfig,
		IWindow[] windows,
		int scale,
		IWindowState[] expectedWindowStates,
		ISliceLayoutPlugin sliceLayoutPlugin,
		IContext ctx
	)
	{
		// Given
		ILayoutEngine innerLayoutEngine = SliceLayouts.CreateRowLayout(ctx, sliceLayoutPlugin, _identity);

		foreach (IWindow w in windows)
		{
			innerLayoutEngine = innerLayoutEngine.AddWindow(w);
		}

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		Rectangle<int> rect = new()
		{
			X = 0,
			Y = 0,
			Width = 1920,
			Height = 1080,
		};

		IMonitor monitor = Substitute.For<IMonitor>();
		monitor.ScaleFactor.Returns(scale);

		// When
		IWindowState[] windowStates = [.. gapsLayoutEngine.DoLayout(rect, monitor)];

		// Then
		windowStates.Should().Equal(expectedWindowStates);
	}

	public static TheoryData<GapsConfig, IWindow, Rectangle<int>, IWindowState[]> DoLayout_OutOfBoundsData
	{
		get
		{
			TheoryData<GapsConfig, IWindow, Rectangle<int>, IWindowState[]> data = [];

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
					Height = 1080,
				},
				[
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 1080,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 1080,
				},
				[
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 1080,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 1080,
				},
				[
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 5,
							Height = 1080,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = -100,
				},
				[
					new WindowState()
					{
						Window = window4,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 0,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 0,
				},
				[
					new WindowState()
					{
						Window = window5,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 0,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 5,
				},
				[
					new WindowState()
					{
						Window = window6,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 1920,
							Height = 5,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 5,
				},
				[
					new WindowState()
					{
						Window = window7,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 5,
							Height = 5,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
					Height = 0,
				},
				[
					new WindowState()
					{
						Window = window8,
						Rectangle = new Rectangle<int>()
						{
							X = 5,
							Y = 5,
							Width = 0,
							Height = 0,
						},
						WindowSize = WindowSize.Normal,
					},
				]
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
				[
					new WindowState()
					{
						Window = window,
						Rectangle = rect,
						WindowSize = WindowSize.Normal,
					},
				]
			);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		// When
		IWindowState[] windowStates = [.. gapsLayoutEngine.DoLayout(rect, Substitute.For<IMonitor>())];

		// Then
		windowStates.Should().Equal(expectedWindowStates);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	internal void DoLayout_WithProxyFloatingLayoutEngine(IContext context, MutableRootSector root)
	{
		// Input
		GapsConfig gapsConfig = new();

		Rectangle<int> rect1 = new(10, 10, 20, 20);
		Rectangle<int> rect2 = new(30, 30, 40, 40);
		Rectangle<int> rect3 = new(50, 50, 60, 60);

		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 100, 100));

		IWindow window1 = StoreTestUtils.CreateWindow((HWND)1);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);
		IWindow window3 = StoreTestUtils.CreateWindow((HWND)3);

		Workspace workspace = StoreTestUtils.CreateWorkspace(context);
		StoreTestUtils.PopulateThreeWayMap(context, root, monitor, workspace, window1);
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window2, workspace);
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window3, workspace);

		IFloatingWindowPlugin floatingWindowPlugin = Substitute.For<IFloatingWindowPlugin>();
		floatingWindowPlugin.FloatingWindows.Returns(_ => new HashSet<HWND>() { window1.Handle, window2.Handle });

		ILayoutEngine inner1 = Substitute.For<ILayoutEngine>();
		ILayoutEngine inner2 = Substitute.For<ILayoutEngine>();

		inner1.AddWindow(Arg.Any<IWindow>()).Returns(inner2);
		inner1.RemoveWindow(Arg.Any<IWindow>()).Returns(inner1);
		inner2.Count.Returns(1);
		inner2
			.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>())
			.Returns(
				[
					new WindowState()
					{
						Rectangle = rect3,
						Window = window3,
						WindowSize = WindowSize.Normal,
					},
				]
			);

		context.NativeManager.DwmGetWindowRectangle(window1.Handle).Returns(rect1);
		context.NativeManager.DwmGetWindowRectangle(window2.Handle).Returns(rect2);

		ProxyFloatingLayoutEngine floatingLayoutEngine = new(context, floatingWindowPlugin, inner1);

		// Given
		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, floatingLayoutEngine);

		// When
		GapsLayoutEngine gaps1 = (GapsLayoutEngine)gapsLayoutEngine.AddWindow(window1);
		GapsLayoutEngine gaps2 = (GapsLayoutEngine)gaps1.AddWindow(window2);
		GapsLayoutEngine gaps3 = (GapsLayoutEngine)gaps2.AddWindow(window3);

		IWindowState[] outputWindowStates = [.. gaps3.DoLayout(monitor.WorkingArea, monitor)];

		// Then
		Assert.Equal(3, outputWindowStates.Length);

		Assert.Contains(
			outputWindowStates,
			ws =>
				ws.Equals(
					new WindowState()
					{
						Window = window1,
						Rectangle = rect1,
						WindowSize = WindowSize.Normal,
					}
				)
		);

		Assert.Contains(
			outputWindowStates,
			ws =>
				ws.Equals(
					new WindowState()
					{
						Window = window2,
						Rectangle = rect2,
						WindowSize = WindowSize.Normal,
					}
				)
		);

		Assert.Contains(
			outputWindowStates,
			ws =>
				ws.Equals(
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>(60, 60, 40, 40),
						WindowSize = WindowSize.Normal,
					}
				)
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	internal void DoLayout_WithFloatingLayoutEngine(IContext context, MutableRootSector root)
	{
		// Input
		GapsConfig gapsConfig = new();

		Rectangle<int> rect1 = new(10, 10, 20, 20);
		Rectangle<int> rect2 = new(30, 30, 40, 40);

		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);
		monitor.WorkingArea.Returns(new Rectangle<int>(0, 0, 100, 100));

		IWindow window1 = StoreTestUtils.CreateWindow((HWND)1);
		IWindow window2 = StoreTestUtils.CreateWindow((HWND)2);

		Workspace workspace = StoreTestUtils.CreateWorkspace(context);
		StoreTestUtils.PopulateThreeWayMap(context, root, monitor, workspace, window1);
		StoreTestUtils.PopulateWindowWorkspaceMap(context, root, window2, workspace);

		context.NativeManager.DwmGetWindowRectangle(window1.Handle).Returns(rect1);
		context.NativeManager.DwmGetWindowRectangle(window2.Handle).Returns(rect2);

		FloatingLayoutEngine floatingLayoutEngine = new(context, _identity);

		// Given
		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, floatingLayoutEngine);

		// When
		GapsLayoutEngine gaps1 = (GapsLayoutEngine)gapsLayoutEngine.AddWindow(window1);
		GapsLayoutEngine gaps2 = (GapsLayoutEngine)gaps1.AddWindow(window2);

		IWindowState[] outputWindowStates = [.. gaps2.DoLayout(monitor.WorkingArea, monitor)];

		// Then
		Assert.Equal(2, outputWindowStates.Length);

		Assert.Contains(
			outputWindowStates,
			ws =>
				ws.Equals(
					new WindowState()
					{
						Window = window1,
						Rectangle = rect1,
						WindowSize = WindowSize.Normal,
					}
				)
		);

		Assert.Contains(
			outputWindowStates,
			ws =>
				ws.Equals(
					new WindowState()
					{
						Window = window2,
						Rectangle = rect2,
						WindowSize = WindowSize.Normal,
					}
				)
		);
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

		LayoutEngineCustomAction<string> action = new()
		{
			Name = "Action",
			Payload = "payload",
			Window = Substitute.For<IWindow>(),
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

		LayoutEngineCustomAction<string> action = new()
		{
			Name = "Action",
			Payload = "payload",
			Window = Substitute.For<IWindow>(),
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
