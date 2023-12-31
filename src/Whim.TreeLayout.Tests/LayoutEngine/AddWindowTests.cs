using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class AddWindowTests
{
	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsNull(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine result = engine.AddWindow(window);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsWindow(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);
		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window2);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
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
						Rectangle = new Rectangle<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsSplitNode_LastFocusedWindowIsNull(
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsSplitNode_CannotFindLastFocusedWindow(
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window3);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsSplitNode_LastFocusedWindowIsLeft(
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
			.AddWindow(window2);

		wrapper.SetAsLastFocusedWindow(window1);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window3,
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_RootIsSplitNode_AddInDifferentDirection(
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
			.AddWindow(window2);

		wrapper.SetAddWindowDirection(Direction.Down);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window3);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window3));
		Assert.Equal(3, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
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
						Rectangle = new Rectangle<int>()
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
						Rectangle = new Rectangle<int>()
						{
							X = 50,
							Y = 50,
							Width = 50,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowAlreadyPresent(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.AddWindow(window2);
		IWindowState[] windowStates = result.DoLayout(rect, monitor).ToArray();

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window2));
		Assert.Equal(2, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window1,
						Rectangle = new Rectangle<int>()
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
						Rectangle = new Rectangle<int>()
						{
							X = 50,
							Y = 0,
							Width = 50,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowWasMinimized(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		IRectangle<int> rect = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MinimizeWindowStart(window).AddWindow(window);
		IWindowState[] windowStates = result.DoLayout(rect, wrapper.Monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window,
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}
}
