using FluentAssertions;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MoveWindowToPointTests
{
	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_RootIsNull(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		TreeLayoutEngine engine = new(wrapper.Context, wrapper.Plugin, wrapper.Identity);
		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window, point);

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_RootIsWindowNode_Right(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

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
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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
	public void MoveWindowToPoint_RootIsWindowNode_Down(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.7 };
		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

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
						Location = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window2,
						Location = new Rectangle<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_RootIsWindowNode_Left(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.5 };
		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

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
						Window = window2,
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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
	public void MoveWindowToPoint_RootIsWindowNode_Up(IWindow window1, IWindow window2, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		IPoint<double> point = new Point<double>() { X = 0.5, Y = 0.3 };
		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window2, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

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
						Window = window2,
						Location = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = window1,
						Location = new Rectangle<int>()
						{
							X = 0,
							Y = 50,
							Width = 100,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				}
			);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_RootIsSplitNode_DoesNotContainPoint(IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 1.7, Y = 0.5 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);

		// Then
		Assert.Same(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
		Assert.False(result.ContainsWindow(window3));
		Assert.Equal(2, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_RootIsSplitNode_AddInDirection(
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 0.7, Y = 0.5 };

		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
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
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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
	public void MoveWindowToPoint_RootIsSplitNode_AddInDifferentDirection(
		IWindow window1,
		IWindow window2,
		IWindow window3,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window1);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		IPoint<double> point = new Point<double>() { X = 0.75, Y = 0.8 };

		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window3, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window1));
		Assert.True(result.ContainsWindow(window2));
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
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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
						Location = new Rectangle<int>()
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

	[InlineAutoSubstituteData(0.25, 0.25)]
	[InlineAutoSubstituteData(0.25, 0.75)]
	[InlineAutoSubstituteData(0.75, 0.25)]
	[InlineAutoSubstituteData(0.75, 0.75)]
	[Theory]
	public void MoveWindowToPoint_AlreadyContainsWindow(double x, double y, IWindow window, IMonitor monitor)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(window);
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		IPoint<double> point = new Point<double>() { X = x, Y = y };

		IRectangle<int> location = new Rectangle<int>() { Width = 100, Height = 100 };

		// When
		ILayoutEngine result = engine.MoveWindowToPoint(window, point);
		IWindowState[] windowStates = result.DoLayout(location, monitor).ToArray();

		// Then
		Assert.NotSame(engine, result);
		Assert.True(result.ContainsWindow(window));
		Assert.Single(windowStates);

		windowStates
			.Should()
			.Equal(
				new IWindowState[]
				{
					new WindowState()
					{
						Window = window,
						Location = new Rectangle<int>()
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
