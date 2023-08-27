using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class MoveSingleWindowEdgeInDirectionTests
{
	[Fact]
	public void MoveWindowEdgesInDirection_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(
			Direction.Left,
			pixelsDeltas,
			new Mock<IWindow>().Object
		);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_CannotFindWindow()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(
			Direction.Left,
			pixelsDeltas,
			new Mock<IWindow>().Object
		);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_CannotMoveRoot()
	{
		// Given
		Mock<IWindow> window1 = new();

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window1.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window1.Object);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_MoveFocusedWindowEdgeTooFar()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = -0.4 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.AddWindow(window3.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window2.Object);

		// Then
		Assert.Same(engine, result);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_MoveAdjacentWindowEdgeTooFar()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.4 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.AddWindow(window3.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window2.Object);

		// Then
		Assert.Same(engine, result);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Horizontal_Data()
	{
		// Move left edge to the left.
		Mock<IWindow>[] leftEdgeLeftWindows = new Mock<IWindow>[] { new(), new(), new() };
		yield return new object[]
		{
			Direction.Left,
			new Point<double>() { X = -0.1 },
			leftEdgeLeftWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = leftEdgeLeftWindows[0].Object,
					Location = new Location<int>()
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
					Window = leftEdgeLeftWindows[1].Object,
					Location = new Location<int>()
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
					Window = leftEdgeLeftWindows[2].Object,
					Location = new Location<int>()
					{
						X = 67,
						Y = 0,
						Width = 33,
						Height = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move left edge to the right.
		Mock<IWindow>[] leftEdgeRightWindows = new Mock<IWindow>[] { new(), new(), new() };
		yield return new object[]
		{
			Direction.Left,
			new Point<double>() { X = 0.1 },
			leftEdgeRightWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = leftEdgeRightWindows[0].Object,
					Location = new Location<int>()
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
					Window = leftEdgeRightWindows[1].Object,
					Location = new Location<int>()
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
					Window = leftEdgeRightWindows[2].Object,
					Location = new Location<int>()
					{
						X = 67,
						Y = 0,
						Width = 33,
						Height = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move right edge to the left.
		Mock<IWindow>[] rightEdgeLeftWindows = new Mock<IWindow>[] { new(), new(), new() };
		yield return new object[]
		{
			Direction.Right,
			new Point<double>() { X = -0.1 },
			rightEdgeLeftWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = rightEdgeLeftWindows[0].Object,
					Location = new Location<int>()
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
					Window = rightEdgeLeftWindows[1].Object,
					Location = new Location<int>()
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
					Window = rightEdgeLeftWindows[2].Object,
					Location = new Location<int>()
					{
						X = 57,
						Y = 0,
						Width = 43,
						Height = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move right edge to the right.
		Mock<IWindow>[] rightEdgeRightWindows = new Mock<IWindow>[] { new(), new(), new() };
		yield return new object[]
		{
			Direction.Right,
			new Point<double>() { X = 0.1 },
			rightEdgeRightWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = rightEdgeRightWindows[0].Object,
					Location = new Location<int>()
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
					Window = rightEdgeRightWindows[1].Object,
					Location = new Location<int>()
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
					Window = rightEdgeRightWindows[2].Object,
					Location = new Location<int>()
					{
						X = 77,
						Y = 0,
						Width = 23,
						Height = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};
	}

	[Theory]
	[MemberData(nameof(MoveWindowEdgesInDirection_Horizontal_Data))]
	public void MoveWindowEdgesInDirection_Horizontal(
		Direction edges,
		IPoint<double> pixelDeltas,
		Mock<IWindow>[] windows,
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		Assert.Equal(3, windows.Length);

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(windows[0].Object)
			.AddWindow(windows[1].Object)
			.AddWindow(windows[2].Object);

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windows[1].Object);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().Equal(windowStates);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Vertical_Data()
	{
		// Move top edge up.
		IWindow[] topEdgeUpWindows = new[]
		{
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object
		};
		yield return new object[]
		{
			Direction.Up,
			new Point<double>() { Y = -0.1 },
			topEdgeUpWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = topEdgeUpWindows[0],
					Location = new Location<int>()
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
					Location = new Location<int>()
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
					Location = new Location<int>()
					{
						X = 0,
						Y = 67,
						Width = 100,
						Height = 33
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move top edge down.
		IWindow[] topEdgeDownWindows = new[]
		{
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object
		};
		yield return new object[]
		{
			Direction.Up,
			new Point<double>() { Y = 0.1 },
			topEdgeDownWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = topEdgeDownWindows[0],
					Location = new Location<int>()
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
					Location = new Location<int>()
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
					Location = new Location<int>()
					{
						X = 0,
						Y = 67,
						Width = 100,
						Height = 33
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom edge up.
		IWindow[] bottomEdgeUpWindows = new[]
		{
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object
		};
		yield return new object[]
		{
			Direction.Down,
			new Point<double>() { Y = -0.1 },
			bottomEdgeUpWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = bottomEdgeUpWindows[0],
					Location = new Location<int>() { Width = 100, Height = 33 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = bottomEdgeUpWindows[1],
					Location = new Location<int>()
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
					Location = new Location<int>()
					{
						Y = 57,
						Height = 43,
						Width = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom edge down.
		IWindow[] bottomEdgeDownWindows = new[]
		{
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object,
			new Mock<IWindow>().Object
		};
		yield return new object[]
		{
			Direction.Down,
			new Point<double>() { Y = 0.1 },
			bottomEdgeDownWindows,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = bottomEdgeDownWindows[0],
					Location = new Location<int>() { Width = 100, Height = 33 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = bottomEdgeDownWindows[1],
					Location = new Location<int>()
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
					Location = new Location<int>()
					{
						Y = 77,
						Height = 23,
						Width = 100
					},
					WindowSize = WindowSize.Normal
				}
			}
		};
	}

	[Theory]
	[MemberData(nameof(MoveWindowEdgesInDirection_Vertical_Data))]
	public void MoveWindowEdgesInDirection_Vertical(
		Direction edges,
		IPoint<double> pixelDeltas,
		IWindow[] windows,
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAddWindowDirection(Direction.Down);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);
		foreach (IWindow window in windows)
		{
			engine = engine.AddWindow(window);
		}

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windows[1]);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().Equal(windowStates);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Diagonal_Data()
	{
		// Move top left window's bottom-right edge up and left.
		Mock<IWindow>[] windows1 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"topLeft",
			Direction.RightDown,
			new Point<double>() { X = -0.1, Y = -0.1 },
			windows1,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows1[0].Object,
					Location = new Location<int>() { Width = 40, Height = 40 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows1[1].Object,
					Location = new Location<int>()
					{
						Y = 40,
						Width = 40,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows1[2].Object,
					Location = new Location<int>()
					{
						X = 40,
						Height = 50,
						Width = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows1[3].Object,
					Location = new Location<int>()
					{
						X = 40,
						Y = 50,
						Width = 60,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move top left window's bottom-right edge down and right.
		Mock<IWindow>[] windows2 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"topLeft",
			Direction.RightDown,
			new Point<double>() { X = 0.1, Y = 0.1 },
			windows2,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows2[0].Object,
					Location = new Location<int>() { Width = 60, Height = 60 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows2[1].Object,
					Location = new Location<int>()
					{
						Y = 60,
						Height = 40,
						Width = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows2[2].Object,
					Location = new Location<int>()
					{
						X = 60,
						Height = 50,
						Width = 40
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows2[3].Object,
					Location = new Location<int>()
					{
						X = 60,
						Y = 50,
						Width = 40,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move top right window's bottom-left edge up and right.
		Mock<IWindow>[] windows3 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"topRight",
			Direction.LeftDown,
			new Point<double>() { X = 0.1, Y = -0.1 },
			windows3,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows3[0].Object,
					Location = new Location<int>() { Width = 60, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows3[1].Object,
					Location = new Location<int>()
					{
						Y = 50,
						Width = 60,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows3[2].Object,
					Location = new Location<int>()
					{
						X = 60,
						Width = 40,
						Height = 40
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows3[3].Object,
					Location = new Location<int>()
					{
						X = 60,
						Y = 40,
						Width = 40,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move top right window's bottom-left edge down and left.
		Mock<IWindow>[] windows4 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"topRight",
			Direction.LeftDown,
			new Point<double>() { X = -0.1, Y = 0.1 },
			windows4,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows4[0].Object,
					Location = new Location<int>() { Width = 40, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows4[1].Object,
					Location = new Location<int>()
					{
						Y = 50,
						Width = 40,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows4[2].Object,
					Location = new Location<int>()
					{
						X = 40,
						Width = 60,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows4[3].Object,
					Location = new Location<int>()
					{
						X = 40,
						Y = 60,
						Width = 60,
						Height = 40
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom left window's top-right edge up and left.
		Mock<IWindow>[] windows5 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"bottomLeft",
			Direction.RightUp,
			new Point<double>() { X = -0.1, Y = -0.1 },
			windows5,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows5[0].Object,
					Location = new Location<int>() { Width = 40, Height = 40 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows5[1].Object,
					Location = new Location<int>()
					{
						Y = 40,
						Width = 40,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows5[2].Object,
					Location = new Location<int>()
					{
						X = 40,
						Width = 60,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows5[3].Object,
					Location = new Location<int>()
					{
						X = 40,
						Y = 50,
						Width = 60,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom left window's top-right edge down and right.
		Mock<IWindow>[] windows6 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"bottomLeft",
			Direction.RightUp,
			new Point<double>() { X = 0.1, Y = 0.1 },
			windows6,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows6[0].Object,
					Location = new Location<int>() { Width = 60, Height = 60 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows6[1].Object,
					Location = new Location<int>()
					{
						Y = 60,
						Width = 60,
						Height = 40
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows6[2].Object,
					Location = new Location<int>()
					{
						X = 60,
						Width = 40,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows6[3].Object,
					Location = new Location<int>()
					{
						X = 60,
						Y = 50,
						Width = 40,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom right window's top-left edge up and right.
		Mock<IWindow>[] windows7 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"bottomRight",
			Direction.LeftUp,
			new Point<double>() { X = 0.1, Y = -0.1 },
			windows7,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows7[0].Object,
					Location = new Location<int>() { Width = 60, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows7[1].Object,
					Location = new Location<int>()
					{
						Y = 50,
						Width = 60,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows7[2].Object,
					Location = new Location<int>()
					{
						X = 60,
						Width = 40,
						Height = 40
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows7[3].Object,
					Location = new Location<int>()
					{
						X = 60,
						Y = 40,
						Width = 40,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		// Move bottom right window's top-left edge down and left.
		Mock<IWindow>[] windows8 = new Mock<IWindow>[] { new(), new(), new(), new() };
		yield return new object[]
		{
			"bottomRight",
			Direction.LeftUp,
			new Point<double>() { X = -0.1, Y = 0.1 },
			windows8,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = windows8[0].Object,
					Location = new Location<int>() { Width = 40, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows8[1].Object,
					Location = new Location<int>()
					{
						Y = 50,
						Width = 40,
						Height = 50
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows8[2].Object,
					Location = new Location<int>()
					{
						X = 40,
						Width = 60,
						Height = 60
					},
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = windows8[3].Object,
					Location = new Location<int>()
					{
						X = 40,
						Y = 60,
						Width = 60,
						Height = 40
					},
					WindowSize = WindowSize.Normal
				}
			}
		};
	}

	[Theory]
	[MemberData(nameof(MoveWindowEdgesInDirection_Diagonal_Data))]
	public void MoveWindowEdgesInDirection_Diagonal(
		string window,
		Direction edges,
		IPoint<double> pixelDeltas,
		Mock<IWindow>[] windows,
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		Assert.Equal(4, windows.Length);
		var (topLeft, bottomLeft, topRight, bottomRight) = (windows[0], windows[1], windows[2], windows[3]);
		Dictionary<string, Mock<IWindow>> windowsDict =
			new()
			{
				{ "topLeft", topLeft },
				{ "topRight", topRight },
				{ "bottomLeft", bottomLeft },
				{ "bottomRight", bottomRight }
			};

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(topLeft.Object)
			.AddWindow(topRight.Object)
			.MoveWindowToPoint(bottomLeft.Object, new Point<double>() { X = 0.25, Y = 0.9 })
			.MoveWindowToPoint(bottomRight.Object, new Point<double>() { X = 0.75, Y = 0.9 });

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windowsDict[window].Object);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);
		expectedWindowStates.Should().Equal(windowStates);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_InvalidEdge()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		IPoint<double> pixelsDeltas = new Point<double>() { X = 0.1, Y = 0.1 };

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.AddWindow(window3.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection((Direction)128, pixelsDeltas, window2.Object);

		// Then
		Assert.Same(engine, result);
	}
}
