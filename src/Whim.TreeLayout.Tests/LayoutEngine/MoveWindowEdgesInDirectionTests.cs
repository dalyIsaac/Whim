using FluentAssertions;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MoveSingleWindowEdgeInDirectionTests
{
	private static IWindow[] CreateWindows(int count)
	{
		IWindow[] windows = new IWindow[count];
		for (int i = 0; i < count; i++)
		{
			windows[i] = Substitute.For<IWindow>();
		}

		return windows;
	}

	[Fact]
	public void MoveWindowEdgesInDirection_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(
			Direction.Left,
			pixelsDeltas,
			Substitute.For<IWindow>()
		);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_CannotFindWindow(IWindow window1, IWindow window2)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(
			Direction.Left,
			pixelsDeltas,
			Substitute.For<IWindow>()
		);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_CannotMoveRoot(IWindow window1)
	{
		// Given

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window1
		);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window1);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_MoveFocusedWindowEdgeTooFar(
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		IPoint<double> pixelsDeltas = new Point<double>() { X = -0.4 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window2);

		// Then
		Assert.Same(engine, result);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_MoveAdjacentWindowEdgeTooFar(
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new();
		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.4 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window2);

		// Then
		Assert.Same(engine, result);
	}

	public static TheoryData<
		Direction,
		IPoint<double>,
		IWindow[],
		IWindowState[]
	> MoveWindowEdgesInDirection_Horizontal_Data
	{
		get
		{
			TheoryData<Direction, IPoint<double>, IWindow[], IWindowState[]> data = [];

			// Move left edge to the left.
			IWindow[] leftEdgeLeftWindows = CreateWindows(3);
			data.Add(
				Direction.Left,
				new Point<double>() { X = -0.1 },
				leftEdgeLeftWindows,
				[
					new WindowState()
					{
						Window = leftEdgeLeftWindows[0],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 23,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = leftEdgeLeftWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 23,
							Y = 0,
							Width = 43,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = leftEdgeLeftWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move left edge to the right.
			IWindow[] leftEdgeRightWindows = CreateWindows(3);
			data.Add(
				Direction.Left,
				new Point<double>() { X = 0.1 },
				leftEdgeRightWindows,
				[
					new WindowState()
					{
						Window = leftEdgeRightWindows[0],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 43,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = leftEdgeRightWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 43,
							Y = 0,
							Width = 23,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = leftEdgeRightWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 67,
							Y = 0,
							Width = 33,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move right edge to the left.
			IWindow[] rightEdgeLeftWindows = CreateWindows(3);
			data.Add(
				Direction.Right,
				new Point<double>() { X = -0.1 },
				rightEdgeLeftWindows,
				[
					new WindowState()
					{
						Window = rightEdgeLeftWindows[0],
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
						Window = rightEdgeLeftWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 23,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = rightEdgeLeftWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 57,
							Y = 0,
							Width = 43,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move right edge to the right.
			IWindow[] rightEdgeRightWindows = CreateWindows(3);
			data.Add(
				Direction.Right,
				new Point<double>() { X = 0.1 },
				rightEdgeRightWindows,
				[
					new WindowState()
					{
						Window = rightEdgeRightWindows[0],
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
						Window = rightEdgeRightWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 33,
							Y = 0,
							Width = 43,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = rightEdgeRightWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 77,
							Y = 0,
							Width = 23,
							Height = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			return data;
		}
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowEdgesInDirection_Horizontal_Data))]
	public void MoveWindowEdgesInDirection_Horizontal(
		Direction edges,
		IPoint<double> pixelDeltas,
		IWindow[] windows,
		IWindowState[] expectedWindowStates,
		IMonitor monitor
	)
	{
		// Given
		Assert.Equal(3, windows.Length);
		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(windows[0])
			.AddWindow(windows[1])
			.AddWindow(windows[2]);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windows[1]);
		IWindowState[] windowStates = result
			.DoLayout(new Rectangle<int>() { Width = 100, Height = 100 }, monitor)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().Equal(windowStates);
	}

	public static TheoryData<
		Direction,
		IPoint<double>,
		IWindow[],
		IWindowState[]
	> MoveWindowEdgesInDirection_Vertical_Data
	{
		get
		{
			TheoryData<Direction, IPoint<double>, IWindow[], IWindowState[]> data = [];

			// Move top edge up.
			IWindow[] topEdgeUpWindows = CreateWindows(3);
			data.Add(
				Direction.Up,
				new Point<double>() { Y = -0.1 },
				topEdgeUpWindows,
				[
					new WindowState()
					{
						Window = topEdgeUpWindows[0],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 23
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = topEdgeUpWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 23,
							Width = 100,
							Height = 43
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = topEdgeUpWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 67,
							Width = 100,
							Height = 33
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move top edge down.
			IWindow[] topEdgeDownWindows = CreateWindows(3);
			data.Add(
				Direction.Up,
				new Point<double>() { Y = 0.1 },
				topEdgeDownWindows,
				[
					new WindowState()
					{
						Window = topEdgeDownWindows[0],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 0,
							Width = 100,
							Height = 43
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = topEdgeDownWindows[1],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 43,
							Width = 100,
							Height = 23
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = topEdgeDownWindows[2],
						Rectangle = new Rectangle<int>()
						{
							X = 0,
							Y = 67,
							Width = 100,
							Height = 33
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom edge up.
			IWindow[] bottomEdgeUpWindows = CreateWindows(3);
			data.Add(
				Direction.Down,
				new Point<double>() { Y = -0.1 },
				bottomEdgeUpWindows,
				[
					new WindowState()
					{
						Window = bottomEdgeUpWindows[0],
						Rectangle = new Rectangle<int>() { Width = 100, Height = 33 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = bottomEdgeUpWindows[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 33,
							Height = 23,
							Width = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = bottomEdgeUpWindows[2],
						Rectangle = new Rectangle<int>()
						{
							Y = 57,
							Height = 43,
							Width = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom edge down.
			IWindow[] bottomEdgeDownWindows = CreateWindows(3);
			data.Add(
				Direction.Down,
				new Point<double>() { Y = 0.1 },
				bottomEdgeDownWindows,
				[
					new WindowState()
					{
						Window = bottomEdgeDownWindows[0],
						Rectangle = new Rectangle<int>() { Width = 100, Height = 33 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = bottomEdgeDownWindows[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 33,
							Height = 43,
							Width = 100
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = bottomEdgeDownWindows[2],
						Rectangle = new Rectangle<int>()
						{
							Y = 77,
							Height = 23,
							Width = 100
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			return data;
		}
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowEdgesInDirection_Vertical_Data))]
	public void MoveWindowEdgesInDirection_Vertical(
		Direction edges,
		IPoint<double> pixelDeltas,
		IWindow[] windows,
		IWindowState[] expectedWindowStates,
		IMonitor monitor
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAddWindowDirection(Direction.Down);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);
		foreach (IWindow window in windows)
		{
			engine = engine.AddWindow(window);
		}

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windows[1]);
		IWindowState[] windowStates = result
			.DoLayout(new Rectangle<int>() { Width = 100, Height = 100 }, monitor)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().Equal(windowStates);
	}

	public static TheoryData<
		string,
		Direction,
		IPoint<double>,
		IWindow[],
		IWindowState[]
	> MoveWindowEdgesInDirection_Diagonal_Data
	{
		get
		{
			TheoryData<string, Direction, IPoint<double>, IWindow[], IWindowState[]> data = [];

			// Move top left window's bottom-right edge up and left.
			IWindow[] windows1 = CreateWindows(4);
			data.Add(
				"topLeft",
				Direction.RightDown,
				new Point<double>() { X = -0.1, Y = -0.1 },
				windows1,
				[
					new WindowState()
					{
						Window = windows1[0],
						Rectangle = new Rectangle<int>() { Width = 40, Height = 40 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows1[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 40,
							Width = 40,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows1[2],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Height = 50,
							Width = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows1[3],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Y = 50,
							Width = 60,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move top left window's bottom-right edge down and right.
			IWindow[] windows2 = CreateWindows(4);
			data.Add(
				"topLeft",
				Direction.RightDown,
				new Point<double>() { X = 0.1, Y = 0.1 },
				windows2,
				[
					new WindowState()
					{
						Window = windows2[0],
						Rectangle = new Rectangle<int>() { Width = 60, Height = 60 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows2[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 60,
							Height = 40,
							Width = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows2[2],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Height = 50,
							Width = 40
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows2[3],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Y = 50,
							Width = 40,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move top right window's bottom-left edge up and right.
			IWindow[] windows3 = CreateWindows(4);
			data.Add(
				"topRight",
				Direction.LeftDown,
				new Point<double>() { X = 0.1, Y = -0.1 },
				windows3,
				[
					new WindowState()
					{
						Window = windows3[0],
						Rectangle = new Rectangle<int>() { Width = 60, Height = 50 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows3[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 50,
							Width = 60,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows3[2],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Width = 40,
							Height = 40
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows3[3],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Y = 40,
							Width = 40,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move top right window's bottom-left edge down and left.
			IWindow[] windows4 = CreateWindows(4);
			data.Add(
				"topRight",
				Direction.LeftDown,
				new Point<double>() { X = -0.1, Y = 0.1 },
				windows4,
				[
					new WindowState()
					{
						Window = windows4[0],
						Rectangle = new Rectangle<int>() { Width = 40, Height = 50 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows4[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 50,
							Width = 40,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows4[2],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Width = 60,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows4[3],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Y = 60,
							Width = 60,
							Height = 40
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom left window's top-right edge up and left.
			IWindow[] windows5 = CreateWindows(4);
			data.Add(
				"bottomLeft",
				Direction.RightUp,
				new Point<double>() { X = -0.1, Y = -0.1 },
				windows5,
				[
					new WindowState()
					{
						Window = windows5[0],
						Rectangle = new Rectangle<int>() { Width = 40, Height = 40 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows5[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 40,
							Width = 40,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows5[2],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Width = 60,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows5[3],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Y = 50,
							Width = 60,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom left window's top-right edge down and right.
			IWindow[] windows6 = CreateWindows(4);
			data.Add(
				"bottomLeft",
				Direction.RightUp,
				new Point<double>() { X = 0.1, Y = 0.1 },
				windows6,
				[
					new WindowState()
					{
						Window = windows6[0],
						Rectangle = new Rectangle<int>() { Width = 60, Height = 60 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows6[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 60,
							Width = 60,
							Height = 40
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows6[2],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Width = 40,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows6[3],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Y = 50,
							Width = 40,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom right window's top-left edge up and right.
			IWindow[] windows7 = CreateWindows(4);
			data.Add(
				"bottomRight",
				Direction.LeftUp,
				new Point<double>() { X = 0.1, Y = -0.1 },
				windows7,
				[
					new WindowState()
					{
						Window = windows7[0],
						Rectangle = new Rectangle<int>() { Width = 60, Height = 50 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows7[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 50,
							Width = 60,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows7[2],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Width = 40,
							Height = 40
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows7[3],
						Rectangle = new Rectangle<int>()
						{
							X = 60,
							Y = 40,
							Width = 40,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			// Move bottom right window's top-left edge down and left.
			IWindow[] windows8 = CreateWindows(4);
			data.Add(
				"bottomRight",
				Direction.LeftUp,
				new Point<double>() { X = -0.1, Y = 0.1 },
				windows8,
				[
					new WindowState()
					{
						Window = windows8[0],
						Rectangle = new Rectangle<int>() { Width = 40, Height = 50 },
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows8[1],
						Rectangle = new Rectangle<int>()
						{
							Y = 50,
							Width = 40,
							Height = 50
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows8[2],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Width = 60,
							Height = 60
						},
						WindowSize = WindowSize.Normal
					},
					new WindowState()
					{
						Window = windows8[3],
						Rectangle = new Rectangle<int>()
						{
							X = 40,
							Y = 60,
							Width = 60,
							Height = 40
						},
						WindowSize = WindowSize.Normal
					}
				]
			);

			return data;
		}
	}

	[Theory]
	[MemberAutoSubstituteData(nameof(MoveWindowEdgesInDirection_Diagonal_Data))]
	public void MoveWindowEdgesInDirection_Diagonal(
		string window,
		Direction edges,
		IPoint<double> pixelDeltas,
		IWindow[] windows,
		IWindowState[] expectedWindowStates,
		IMonitor monitor
	)
	{
		// Given
		Assert.Equal(4, windows.Length);
		var (topLeft, bottomLeft, topRight, bottomRight) = (windows[0], windows[1], windows[2], windows[3]);
		Dictionary<string, IWindow> windowsDict =
			new()
			{
				{ "topLeft", topLeft },
				{ "topRight", topRight },
				{ "bottomLeft", bottomLeft },
				{ "bottomRight", bottomRight }
			};

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(topLeft)
			.AddWindow(topRight)
			.MoveWindowToPoint(bottomLeft, new Point<double>() { X = 0.25, Y = 0.9 })
			.MoveWindowToPoint(bottomRight, new Point<double>() { X = 0.75, Y = 0.9 });

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windowsDict[window]);
		IWindowState[] windowStates = result
			.DoLayout(new Rectangle<int>() { Width = 100, Height = 100 }, monitor)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);
		expectedWindowStates.Should().Equal(windowStates);
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_InvalidEdge(IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection((Direction)128, pixelsDeltas, window2);

		// Then
		Assert.Same(engine, result);
	}
}
