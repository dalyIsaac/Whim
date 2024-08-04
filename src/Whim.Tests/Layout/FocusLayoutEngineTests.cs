using System.Collections;
using System.Linq;
using FluentAssertions;

namespace Whim.Tests;

public class FocusLayoutEngineTests
{
	private static readonly LayoutEngineIdentity _identity = new();

	[Fact]
	public void Name()
	{
		// Given
		FocusLayoutEngine sut = new(_identity);

		// When
		string name = sut.Name;

		// Then
		Assert.Equal("Focus", name);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_NewWindow(IWindow window)
	{
		// Given
		FocusLayoutEngine sut = new(_identity);

		// When
		ILayoutEngine result = sut.AddWindow(window);

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_ExistingWindow(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.AddWindow(window);

		// Then
		Assert.Same(sut, result);
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_ExistingWindow(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.RemoveWindow(window);

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_NonExistingWindow(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window2);

		// When
		ILayoutEngine result = sut.RemoveWindow(window1);

		// Then
		Assert.Same(sut, result);
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_ExistingWindow(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		bool result = sut.ContainsWindow(window);

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_NonExistingWindow(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		bool result = sut.ContainsWindow(window);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_ExistingWindow(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		IWindow? result = sut.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}

	[Fact]
	public void GetFirstWindow_Empty()
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		IWindow? result = sut.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_Empty(Rectangle<int> rectangle, IMonitor monitor)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		IEnumerable result = sut.DoLayout(rectangle, monitor);

		// Then
		Assert.Empty(result);
	}

	[Theory, AutoSubstituteData]
	public void DoLayout_MultipleWindows(
		Rectangle<int> rectangle,
		IMonitor monitor,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window1);
		sut = sut.AddWindow(window2);
		sut = sut.AddWindow(window3);

		// When
		IWindowState[] result = sut.DoLayout(rectangle, monitor).ToArray();

		// Then
		Assert.Equal(3, result.Length);

		IWindowState[] expected =
		[
			new WindowState()
			{
				Window = window1,
				Rectangle = rectangle,
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = window2,
				Rectangle = rectangle,
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = window3,
				Rectangle = rectangle,
				WindowSize = WindowSize.Normal
			}
		];

		expected.Should().BeEquivalentTo(result);
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowNotInLayout(Direction direction, IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		ILayoutEngine result = sut.FocusWindowInDirection(direction, window);

		// Then
		Assert.Same(sut, result);
		window.DidNotReceive().Focus();
	}

	[Theory]
	[InlineAutoSubstituteData(0, Direction.Right, 1)]
	[InlineAutoSubstituteData(0, Direction.Down, 1)]
	[InlineAutoSubstituteData(0, Direction.Left, 2)]
	[InlineAutoSubstituteData(0, Direction.Up, 2)]
	public void FocusWindowInDirection_WindowInLayout(int focusedIndex, Direction direction, int expectedIndex)
	{
		// Given
		IWindow[] windows = Enumerable.Range(0, 3).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);
		sut = sut.AddWindow(windows[2]);

		// When
		ILayoutEngine result = sut.FocusWindowInDirection(direction, windows[focusedIndex]);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(sut.Count, result.Count);
		Assert.Equal(WindowSize.Normal, windowStates[expectedIndex].WindowSize);
		windowStates[expectedIndex].Window.Received().Focus();
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection(IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.MoveWindowEdgesInDirection(Direction.Left, new Point<double>(), window);

		// Then
		Assert.Same(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_NewWindow(IWindow window, IPoint<double> point)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		ILayoutEngine result = sut.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_ExistingWindow(IWindow window, IPoint<double> point)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window);

		// When
		ILayoutEngine result = sut.MoveWindowToPoint(window, point);

		// Then
		Assert.Same(sut, result);
		Assert.Equal(1, result.Count);
	}

	[Theory]
	[InlineAutoSubstituteData("Focus.toggle_maximized", false, WindowSize.Maximized)]
	[InlineAutoSubstituteData("Focus.toggle_maximized", true, WindowSize.Normal)]
	[InlineAutoSubstituteData("Focus.set_maximized", false, WindowSize.Maximized)]
	[InlineAutoSubstituteData("Focus.set_maximized", true, WindowSize.Maximized)]
	[InlineAutoSubstituteData("Focus.unset_maximized", false, WindowSize.Normal)]
	[InlineAutoSubstituteData("Focus.unset_maximized", true, WindowSize.Normal)]
	public void PerformCustomAction_SetMaximized(
		string actionName,
		bool maximized,
		WindowSize expectedWindowSize,
		IWindow window1,
		IWindow window2
	)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity, maximized);
		sut = sut.AddWindow(window1);
		sut = sut.AddWindow(window2);

		// When
		ILayoutEngine result = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow?>()
			{
				Name = actionName,
				Window = null,
				Payload = null
			}
		);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), Substitute.For<IMonitor>()).ToArray();

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(2, result.Count);

		IWindowState[] expected =
		[
			new WindowState()
			{
				Window = window1,
				Rectangle = new Rectangle<int>(),
				WindowSize = expectedWindowSize
			},
			new WindowState()
			{
				Window = window2,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		];

		expected.Should().BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_UnknownAction(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(window1);
		sut = sut.AddWindow(window2);

		// When
		ILayoutEngine result = sut.PerformCustomAction(
			new LayoutEngineCustomAction<IWindow?>()
			{
				Name = "Focus.unknown",
				Window = null,
				Payload = null
			}
		);

		// Then
		Assert.Same(sut, result);
		Assert.Equal(2, result.Count);
	}

	#region SwapWindowInDirection
	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_NoWindows(Direction direction, IWindow window)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When
		ILayoutEngine result = sut.SwapWindowInDirection(direction, window);

		// Then
		Assert.Same(sut, result);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_CannotFindWindow(Direction direction, IWindow addedWindow, IWindow swapWindow)
	{
		// Given
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(addedWindow);

		// When
		ILayoutEngine result = sut.SwapWindowInDirection(direction, swapWindow);

		// Then
		Assert.Same(sut, result);
	}

	[Theory]
	[InlineAutoSubstituteData(0, Direction.Right, 1)]
	[InlineAutoSubstituteData(0, Direction.Down, 1)]
	[InlineAutoSubstituteData(0, Direction.Left, 2)]
	[InlineAutoSubstituteData(0, Direction.Up, 2)]
	public void SwapWindowInDirection_CanFindWindow(int focusedIndex, Direction direction, int expectedIndex)
	{
		// Given
		IWindow[] windows = Enumerable.Range(0, 3).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);
		sut = sut.AddWindow(windows[2]);

		// When
		ILayoutEngine result = sut.SwapWindowInDirection(direction, windows[focusedIndex]);

		// Then
		Assert.NotSame(sut, result);
		Assert.Equal(sut.Count, result.Count);
		Assert.Equal(windows[expectedIndex], result.GetFirstWindow());
	}
	#endregion

	#region MinimizeWindowStart
	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_ContainsWindow(IMonitor monitor)
	{
		// Given there are two windows in the layout
		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);

		// When the focused window is minimized
		ILayoutEngine result = sut.MinimizeWindowStart(windows[1]);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then all the windows are minimized
		Assert.NotSame(sut, result);
		Assert.Equal(2, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = windows[0],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = windows[1],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_Empty(IMonitor monitor, IWindow newWindow)
	{
		// Given there are two windows in the layout
		ILayoutEngine sut = new FocusLayoutEngine(_identity);

		// When a new window is minimized
		ILayoutEngine result = sut.MinimizeWindowStart(newWindow);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then the window is added to the layout
		Assert.NotSame(sut, result);
		Assert.Equal(1, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = newWindow,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_WindowNotFound(IMonitor monitor, IWindow newWindow)
	{
		// Given there are two windows in the layout
		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);

		// When a new window is minimized
		ILayoutEngine result = sut.MinimizeWindowStart(newWindow);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then the window is added to the layout
		Assert.NotSame(sut, result);
		Assert.Equal(3, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = windows[0],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = windows[1],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = newWindow,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowStart_NonFocusedWindowMinimized(IMonitor monitor)
	{
		// Given there are two windows in the layout
		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);

		// When the non-focused window is minimized
		ILayoutEngine result = sut.MinimizeWindowStart(windows[0]);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then the window is added to the layout
		Assert.Same(sut, result);
		Assert.Equal(2, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = windows[0],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = windows[1],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}
	#endregion

	#region MinimizeWindowEnd
	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_DoesNotContainWindow(IMonitor monitor, IWindow newWindow)
	{
		// Given there are two windows in the layout
		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);

		// When a new window is restored
		ILayoutEngine result = sut.MinimizeWindowEnd(newWindow);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then the window is added to the layout
		Assert.NotSame(sut, result);
		Assert.Equal(3, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = windows[0],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = windows[1],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			},
			new WindowState()
			{
				Window = newWindow,
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void MinimizeWindowEnd_ExistingWindow(IMonitor monitor)
	{
		// Given there are two windows in the layout
		IWindow[] windows = Enumerable.Range(0, 2).Select(_ => Substitute.For<IWindow>()).ToArray();
		ILayoutEngine sut = new FocusLayoutEngine(_identity);
		sut = sut.AddWindow(windows[0]);
		sut = sut.AddWindow(windows[1]);

		// When a new window is restored
		ILayoutEngine result = sut.MinimizeWindowEnd(windows[0]);
		IWindowState[] windowStates = result.DoLayout(new Rectangle<int>(), monitor).ToArray();

		// Then the window is added to the layout
		Assert.NotSame(sut, result);
		Assert.Equal(2, result.Count);
		new IWindowState[]
		{
			new WindowState()
			{
				Window = windows[0],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = windows[1],
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Minimized
			}
		}
			.Should()
			.BeEquivalentTo(windowStates);
	}
	#endregion
}
