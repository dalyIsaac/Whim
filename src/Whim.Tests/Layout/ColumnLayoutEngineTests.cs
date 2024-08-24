using System.Linq;
using AutoFixture;

namespace Whim.Tests;

public class ColumnLayoutEngineCustomization : ICustomization
{
	private static readonly Random rnd = new();

	public void Customize(IFixture fixture)
	{
		IWindow window = Substitute.For<IWindow>();
		window.Handle.Returns(new Windows.Win32.Foundation.HWND(rnd.Next()));
		fixture.Inject(window);
	}
}

public class ColumnLayoutEngineTests
{
	private static readonly LayoutEngineIdentity identity = new();

	[Fact]
	public void Name_Default()
	{
		// Given
		ColumnLayoutEngine engine = new(identity);

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Column", name);
	}

	[Fact]
	public void Name_Custom()
	{
		// Given
		ColumnLayoutEngine engine = new(identity) { Name = "Custom" };

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Custom", name);
	}

	[Fact]
	public void LeftToRight_Default()
	{
		// Given
		ColumnLayoutEngine engine = new(identity);

		// When
		bool leftToRight = engine.LeftToRight;

		// Then
		Assert.True(leftToRight);
	}

	[Fact]
	public void LeftToRight_Custom()
	{
		// Given
		ColumnLayoutEngine engine = new(identity) { LeftToRight = false };

		// When
		bool leftToRight = engine.LeftToRight;

		// Then
		Assert.False(leftToRight);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow(IWindow window)
	{
		// Given
		ColumnLayoutEngine engine = new(identity);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowAlreadyPresent(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.AddWindow(window);

		// Then
		Assert.Same(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(window);

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(0, newLayoutEngine.Count);
	}

	[Theory, AutoSubstituteData]
	public void Remove_NoChanges(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		ILayoutEngine newLayoutEngine = engine.RemoveWindow(Substitute.For<IWindow>());

		// Then
		Assert.Same(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Theory]
	[InlineAutoSubstituteData(0, 0)]
	[InlineAutoSubstituteData(1, 1)]
	[InlineAutoSubstituteData(5, 0)]
	[InlineAutoSubstituteData(0, 5)]
	public void Count(int windowCount, int minimizedWindowCount)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When some windows, and minimized windows are added
		for (int i = 0; i < windowCount; i++)
		{
			engine = engine.AddWindow(Substitute.For<IWindow>());
		}

		for (int i = 0; i < minimizedWindowCount; i++)
		{
			engine = engine.MinimizeWindowStart(Substitute.For<IWindow>());
		}

		// Then the Count is the sum of the windows and minimized window count
		Assert.Equal(windowCount + minimizedWindowCount, engine.Count);
	}

	[Theory, AutoSubstituteData]
	public void Contains(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(window);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void Contains_False(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		bool contains = engine.ContainsWindow(Substitute.For<IWindow>());

		// Then
		Assert.False(contains);
	}

	#region DoLayout
	[Fact]
	public void DoLayout_Empty()
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Empty(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_OnlyMinimizedWindows(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).MinimizeWindowStart(window);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Single(windowStates);
		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = window,
				WindowSize = WindowSize.Minimized,
			},
			windowStates[0]
		);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_LeftToRight_SingleWindow(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		// Then
		Assert.Single(windowStates);
		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080,
				},
				Window = window,
				WindowSize = WindowSize.Normal,
			},
			windowStates[0]
		);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_LeftToRight_MultipleWindows(IWindow window, IWindow window2, IWindow window3)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window).AddWindow(window2).AddWindow(window3);

		Rectangle<int> rect = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.Equal(3, result.Length);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window,
				WindowSize = WindowSize.Normal,
			},
			result[0]
		);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 640,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window2,
				WindowSize = WindowSize.Normal,
			},
			result[1]
		);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 1280,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window3,
				WindowSize = WindowSize.Normal,
			},
			result[2]
		);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_RightToLeft_SingleWindow(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }.AddWindow(window);

		Rectangle<int> rect = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.Single(result);
		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080,
				},
				Window = window,
				WindowSize = WindowSize.Normal,
			},
			result[0]
		);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_RightToLeft_MultipleWindows(IWindow window, IWindow window2, IWindow window3)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(window)
			.AddWindow(window2)
			.AddWindow(window3);

		Rectangle<int> rect = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.Equal(3, result.Length);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 1280,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window,
				WindowSize = WindowSize.Normal,
			},
			result[0]
		);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 640,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window2,
				WindowSize = WindowSize.Normal,
			},
			result[1]
		);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>()
				{
					X = 0,
					Y = 0,
					Width = 640,
					Height = 1080,
				},
				Window = window3,
				WindowSize = WindowSize.Normal,
			},
			result[2]
		);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_MinimizedWindows(IWindow window, IWindow window2)
	{
		// Given one window is minimized
		ILayoutEngine engine = new ColumnLayoutEngine(identity)
			.AddWindow(window)
			.AddWindow(window2)
			.MinimizeWindowStart(window2);

		Rectangle<int> rect = new() { Width = 1920, Height = 1080 };

		// When performing a layout
		IWindowState[] result = engine.DoLayout(rect, Substitute.For<IMonitor>()).ToArray();

		// Then the resulting window states will include the minimized window
		Assert.Equal(2, result.Length);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = rect,
				Window = window,
				WindowSize = WindowSize.Normal,
			},
			result[0]
		);

		Assert.Equal(
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = window2,
				WindowSize = WindowSize.Minimized,
			},
			result[1]
		);
	}
	#endregion

	[Fact]
	public void GetFirstWindow_Null()
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_SingleWindow(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}

	#region FocusWindowInDirection
	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_IgnoreIllegalDirection(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Up, leftWindow);

		// Then
		leftWindow.DidNotReceive().Focus();
		rightWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowNotFound(IWindow leftWindow, IWindow rightWindow, IWindow otherWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Left, otherWindow);

		// Then
		leftWindow.DidNotReceive().Focus();
		rightWindow.DidNotReceive().Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_LeftToRight_FocusRight(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Right, leftWindow);

		// Then
		leftWindow.DidNotReceive().Focus();
		rightWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_LeftToRight_FocusRightWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Right, rightWindow);

		// Then
		leftWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_LeftToRight_FocusLeft(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Left, rightWindow);

		// Then
		leftWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_LeftToRight_FocusLeftWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Left, leftWindow);

		// Then
		rightWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_RightToLeft_FocusLeft(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(leftWindow)
			.AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Left, leftWindow);

		// Then
		rightWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_RightToLeft_FocusLeftWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(leftWindow)
			.AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Left, rightWindow);

		// Then
		leftWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_RightToLeft_FocusRight(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(leftWindow)
			.AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Right, rightWindow);

		// Then
		leftWindow.Received(1).Focus();
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_RightToLeft_FocusRightWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(leftWindow)
			.AddWindow(rightWindow);

		// When
		engine.FocusWindowInDirection(Direction.Right, leftWindow);

		// Then
		rightWindow.Received(1).Focus();
	}
	#endregion

	#region SwapWindowInDirection
	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_IgnoreIllegalDirection(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Up, leftWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Same(engine, newEngine);
		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowNotFound(IWindow rightWindow, IWindow leftWindow, IWindow notFoundWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, notFoundWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Same(engine, newEngine);
		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_LeftToRight_SwapRight(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, leftWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(leftWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_LeftToRight_SwapRightWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, rightWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(leftWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_LeftToRight_SwapLeft(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, rightWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(leftWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_LeftToRight_SwapLeftWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(leftWindow).AddWindow(rightWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, leftWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow, windows[0].Window);
		Assert.Equal(0, windows[0].Rectangle.X);

		Assert.Equal(leftWindow, windows[1].Window);
		Assert.Equal(960, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_RightToLeft_SwapLeft(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(rightWindow)
			.AddWindow(leftWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, leftWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(960, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(0, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_RightToLeft_SwapLeftWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(rightWindow)
			.AddWindow(leftWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, rightWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(960, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(0, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_RightToLeft_SwapRight(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(rightWindow)
			.AddWindow(leftWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, rightWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(960, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(0, windows[1].Rectangle.X);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_RightToLeft_SwapRightWrapAround(IWindow rightWindow, IWindow leftWindow)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(rightWindow)
			.AddWindow(leftWindow);

		// When
		ILayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, leftWindow);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow, windows[0].Window);
		Assert.Equal(960, windows[0].Rectangle.X);

		Assert.Equal(rightWindow, windows[1].Window);
		Assert.Equal(0, windows[1].Rectangle.X);
	}
	#endregion

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window).AddWindow(Substitute.For<IWindow>());

		// When
		ILayoutEngine newEngine = engine.MoveWindowEdgesInDirection(
			Direction.Up,
			new Point<double>() { X = 0.2 },
			window
		);

		// Then
		Assert.Same(engine, newEngine);
	}

	#region AddWindowAtPoint
	[Theory, AutoSubstituteData]
	public void AddWindowAtPoint_LessThan0(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(Substitute.For<IWindow>());

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, new Point<double>() { X = -10 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(0, windows.FindIndex(w => w.Window == window));
	}

	[Theory, AutoSubstituteData]
	public void AddWindowAtPoint_GreaterThanAmountOfWindows(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(Substitute.For<IWindow>());

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, new Point<double>() { X = 10 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window));
	}

	[Theory, AutoSubstituteData]
	public void AddWindowAtPoint_LeftToRight_Middle(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity)
			.AddWindow(Substitute.For<IWindow>())
			.AddWindow(Substitute.For<IWindow>());

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, new Point<double>() { X = 0.5 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window));
	}

	[Theory, AutoSubstituteData]
	public void AddWindowAtPoint_RightToLeft_Middle(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity) { LeftToRight = false }
			.AddWindow(Substitute.For<IWindow>())
			.AddWindow(Substitute.For<IWindow>());

		// When
		ILayoutEngine newEngine = engine.MoveWindowToPoint(window, new Point<double>() { X = 0.5 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Rectangle<int>() { Width = 1920, Height = 1080 }, Substitute.For<IMonitor>())
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window));
	}
	#endregion

	[Theory, AutoSubstituteData]
	public void PerformCustomAction(IWindow window)
	{
		// Given
		ILayoutEngine engine = new ColumnLayoutEngine(identity).AddWindow(window);
		LayoutEngineCustomAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = Substitute.For<IWindow>(),
			};

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Same(engine, newEngine);
	}

	#region MinimizeWindowStart
	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowNotFound(IWindow window)
	{
		// Given an empty layout engine
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When an untracked window is minimized
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then the window becomes tracked as a minimized window
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, newEngine.Count);
		Assert.True(newEngine.ContainsWindow(window));
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowAlreadyMinimized(IWindow window)
	{
		// Given a window has been tracked as minimized
		ILayoutEngine engine = new ColumnLayoutEngine(identity).MinimizeWindowStart(window);

		// When the window is minimized again
		ILayoutEngine newEngine = engine.MinimizeWindowStart(window);

		// Then nothing changes
		Assert.Same(engine, newEngine);
		Assert.Equal(1, newEngine.Count);
		Assert.True(newEngine.ContainsWindow(window));
	}
	#endregion

	#region MinimizeWindowEnd
	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_WindowNotFound(IWindow window)
	{
		// Given an empty layout engine
		ILayoutEngine engine = new ColumnLayoutEngine(identity);

		// When a window is restored from an empty layout engine
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then the window is tracked as a normal window
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, newEngine.Count);
		Assert.True(newEngine.ContainsWindow(window));
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_WindowAlreadyMinimized(IWindow window)
	{
		// Given a window has already been restored
		ILayoutEngine engine = new ColumnLayoutEngine(identity).MinimizeWindowEnd(window);

		// When the window is restored again
		ILayoutEngine newEngine = engine.MinimizeWindowEnd(window);

		// Then nothing changes
		Assert.Same(engine, newEngine);
		Assert.Equal(1, newEngine.Count);
		Assert.True(newEngine.ContainsWindow(window));
	}
	#endregion
}
