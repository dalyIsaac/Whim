using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class ImmutableColumnLayoutEngineTests
{
	private static readonly Random rnd = new();

	private static Mock<IWindow> CreateMockWindow()
	{
		Mock<IWindow> window = new();
		window.Setup(w => w.Handle).Returns(new Windows.Win32.Foundation.HWND(rnd.Next()));
		return window;
	}

	private static IWindow CreateWindow() => CreateMockWindow().Object;

	[Fact]
	public void Name_Default()
	{
		// Given
		ImmutableColumnLayoutEngine engine = new();

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Column", name);
	}

	[Fact]
	public void Name_Custom()
	{
		// Given
		ImmutableColumnLayoutEngine engine = new() { Name = "Custom" };

		// When
		string name = engine.Name;

		// Then
		Assert.Equal("Custom", name);
	}

	[Fact]
	public void LeftToRight_Default()
	{
		// Given
		ImmutableColumnLayoutEngine engine = new();

		// When
		bool leftToRight = engine.LeftToRight;

		// Then
		Assert.True(leftToRight);
	}

	[Fact]
	public void LeftToRight_Custom()
	{
		// Given
		ImmutableColumnLayoutEngine engine = new() { LeftToRight = false };

		// When
		bool leftToRight = engine.LeftToRight;

		// Then
		Assert.False(leftToRight);
	}

	[Fact]
	public void Add()
	{
		// Given
		ImmutableColumnLayoutEngine engine = new();

		// When
		IImmutableLayoutEngine newLayoutEngine = engine.Add(CreateWindow());

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Fact]
	public void Remove()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window.Object);

		// When
		IImmutableLayoutEngine newLayoutEngine = engine.Remove(window.Object);

		// Then
		Assert.NotSame(engine, newLayoutEngine);
		Assert.Equal(0, newLayoutEngine.Count);
	}

	[Fact]
	public void Remove_NoChanges()
	{
		// Given
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(CreateWindow());

		// When
		IImmutableLayoutEngine newLayoutEngine = engine.Remove(new Mock<IWindow>().Object);

		// Then
		Assert.Same(engine, newLayoutEngine);
		Assert.Equal(1, newLayoutEngine.Count);
	}

	[Fact]
	public void Contains()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window.Object);

		// When
		bool contains = engine.Contains(window.Object);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void Contains_False()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window.Object);

		// When
		bool contains = engine.Contains(new Mock<IWindow>().Object);

		// Then
		Assert.False(contains);
	}

	#region DoLayout
	[Fact]
	public void DoLayout_Empty()
	{
		// Given
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine();

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		// Then
		Assert.Empty(windowStates);
	}

	[Fact]
	public void DoLayout_LeftToRight_SingleWindow()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window.Object);

		// When
		IWindowState[] windowStates = engine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		// Then
		Assert.Single(windowStates);
		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080
				},
				Window = window.Object,
				WindowSize = WindowSize.Normal
			},
			windowStates[0]
		);
	}

	[Fact]
	public void DoLayout_LeftToRight_MultipleWindows()
	{
		// Given
		IWindow window = CreateWindow();
		IWindow window2 = CreateWindow();
		IWindow window3 = CreateWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window).Add(window2).Add(window3);

		Location<int> location = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		// Then
		Assert.Equal(3, result.Length);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window,
				WindowSize = WindowSize.Normal
			},
			result[0]
		);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 640,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window2,
				WindowSize = WindowSize.Normal
			},
			result[1]
		);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 1280,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window3,
				WindowSize = WindowSize.Normal
			},
			result[2]
		);
	}

	[Fact]
	public void DoLayout_RightToLeft_SingleWindow()
	{
		// Given
		IWindow window = CreateWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }.Add(window);

		Location<int> location = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		// Then
		Assert.Single(result);
		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080
				},
				Window = window,
				WindowSize = WindowSize.Normal
			},
			result[0]
		);
	}

	[Fact]
	public void DoLayout_RightToLeft_MultipleWindows()
	{
		// Given
		IWindow window = CreateWindow();
		IWindow window2 = CreateWindow();
		IWindow window3 = CreateWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(window)
			.Add(window2)
			.Add(window3);

		Location<int> location = new() { Width = 1920, Height = 1080 };

		// When
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		// Then
		Assert.Equal(3, result.Length);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 1280,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window,
				WindowSize = WindowSize.Normal
			},
			result[0]
		);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 640,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window2,
				WindowSize = WindowSize.Normal
			},
			result[1]
		);

		Assert.Equal(
			new WindowState()
			{
				Location = new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 640,
					Height = 1080
				},
				Window = window3,
				WindowSize = WindowSize.Normal
			},
			result[2]
		);
	}
	#endregion

	[Fact]
	public void GetFirstWindow_Null()
	{
		// Given
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine();

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetFirstWindow_SingleWindow()
	{
		// Given
		IWindow window = CreateWindow();
		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(window);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}

	#region FocusWindowInDirection
	[Fact]
	public void FocusWindowInDirection_IgnoreIllegalDirection()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Up, leftWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_WindowNotFound()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> otherWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, otherWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRight()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Right, leftWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRightWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Right, rightWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeft()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, rightWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeftWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, leftWindow.Object);

		// Then
		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeft()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, leftWindow.Object);

		// Then
		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeftWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Left, rightWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRight()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Right, rightWindow.Object);

		// Then
		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRightWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		engine.FocusWindowInDirection(Direction.Right, leftWindow.Object);

		// Then
		rightWindow.Verify(w => w.Focus(), Times.Once);
	}
	#endregion

	#region SwapWindowInDirection
	[Fact]
	public void SwapWindowInDirection_IgnoreIllegalDirection()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Up, leftWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Same(engine, newEngine);
		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_WindowNotFound()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> notFoundWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, notFoundWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Same(engine, newEngine);
		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRight()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, leftWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRightWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, rightWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeft()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, rightWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeftWrapAround()
	{
		// Given
		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(leftWindow.Object)
			.Add(rightWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, leftWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeft()
	{
		// Given
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(rightWindow.Object)
			.Add(leftWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, leftWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeftWrapAround()
	{
		// Given
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(rightWindow.Object)
			.Add(leftWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Left, rightWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRight()
	{
		// Given
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(rightWindow.Object)
			.Add(leftWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, rightWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRightWrapAround()
	{
		// Given
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(rightWindow.Object)
			.Add(leftWindow.Object);

		// When
		IImmutableLayoutEngine newEngine = engine.SwapWindowInDirection(Direction.Right, leftWindow.Object);

		// Then
		IWindowState[] windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToArray();

		Assert.Equal(2, windows.Length);
		Assert.NotSame(engine, newEngine);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}
	#endregion

	[Fact]
	public void MoveWindowEdgesInDirection()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(window.Object)
			.Add(new Mock<IWindow>().Object);

		// When
		IImmutableLayoutEngine newEngine = engine.MoveWindowEdgesInDirection(
			Direction.Up,
			new Point<double>() { X = 0.2 },
			window.Object
		);

		// Then
		Assert.Same(engine, newEngine);
	}

	#region AddWindowAtPoint
	[Fact]
	public void AddWindowAtPoint_LessThan0()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(new Mock<IWindow>().Object);

		// When
		IImmutableLayoutEngine newEngine = engine.AddAtPoint(window.Object, new Point<double>() { X = -10 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(0, windows.FindIndex(w => w.Window == window.Object));
	}

	[Fact]
	public void AddWindowAtPoint_GreaterThanAmountOfWindows()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine().Add(new Mock<IWindow>().Object);

		// When
		IImmutableLayoutEngine newEngine = engine.AddAtPoint(window.Object, new Point<double>() { X = 10 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window.Object));
	}

	[Fact]
	public void AddWindowAtPoint_LeftToRight_Middle()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(new Mock<IWindow>().Object)
			.Add(new Mock<IWindow>().Object);

		// When
		IImmutableLayoutEngine newEngine = engine.AddAtPoint(window.Object, new Point<double>() { X = 0.5 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window.Object));
	}

	[Fact]
	public void AddWindowAtPoint_RightToLeft_Middle()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine() { LeftToRight = false }
			.Add(new Mock<IWindow>().Object)
			.Add(new Mock<IWindow>().Object);

		// When
		IImmutableLayoutEngine newEngine = engine.AddAtPoint(window.Object, new Point<double>() { X = 0.5 });
		List<IWindowState> windows = newEngine
			.DoLayout(new Location<int>() { Width = 1920, Height = 1080 }, new Mock<IMonitor>().Object)
			.ToList();

		// Then
		Assert.NotSame(engine, newEngine);
		Assert.Equal(1, windows.FindIndex(w => w.Window == window.Object));
	}
	#endregion

	[Fact]
	public void HidePhantomWindows()
	{
		// Given
		Mock<IWindow> window = CreateMockWindow();

		IImmutableLayoutEngine engine = new ImmutableColumnLayoutEngine()
			.Add(window.Object)
			.Add(new Mock<IWindow>().Object);

		// When
		engine.HidePhantomWindows();

		// Then nothing...?
	}
}
