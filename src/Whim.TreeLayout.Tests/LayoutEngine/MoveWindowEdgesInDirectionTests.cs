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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object);

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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object);

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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(window1.Object);

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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(Direction.Left, pixelsDeltas, window2.Object);

		// Then
		Assert.Same(engine, result);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Horizontal_Data()
	{
		// Move left edge to the left.
		yield return new object[]
		{
			Direction.Left,
			new Point<double>() { X = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Left,
			new Point<double>() { X = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Right,
			new Point<double>() { X = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Right,
			new Point<double>() { X = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, window2.Object);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Vertical_Data()
	{
		// Move top edge up.
		yield return new object[]
		{
			Direction.Up,
			new Point<double>() { Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Up,
			new Point<double>() { Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Down,
			new Point<double>() { Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 100, Height = 33 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			Direction.Down,
			new Point<double>() { Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 100, Height = 33 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
		{
			AddNodeDirection = Direction.Down
		}
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, window2.Object);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);

		expectedWindowStates.Should().BeEquivalentTo(windowStates);
	}

	public static IEnumerable<object[]> MoveWindowEdgesInDirection_Diagonal_Data()
	{
		// Move top left window's bottom-right edge up and left.
		yield return new object[]
		{
			"topLeft",
			Direction.RightDown,
			new Point<double>() { X = -0.1, Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 40, Height = 40 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"topLeft",
			Direction.RightDown,
			new Point<double>() { X = 0.1, Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 60, Height = 60 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"topRight",
			Direction.LeftDown,
			new Point<double>() { X = 0.1, Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 60, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"topRight",
			Direction.LeftDown,
			new Point<double>() { X = -0.1, Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 40, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"bottomLeft",
			Direction.RightUp,
			new Point<double>() { X = -0.1, Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 40, Height = 40 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"bottomLeft",
			Direction.RightUp,
			new Point<double>() { X = 0.1, Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 60, Height = 60 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"bottomRight",
			Direction.LeftUp,
			new Point<double>() { X = 0.1, Y = -0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 60, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		yield return new object[]
		{
			"bottomRight",
			Direction.LeftUp,
			new Point<double>() { X = -0.1, Y = 0.1 },
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>() { Width = 40, Height = 50 },
					WindowSize = WindowSize.Normal
				},
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
					Window = new Mock<IWindow>().Object,
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
		IWindowState[] expectedWindowStates
	)
	{
		// Given
		Mock<IWindow> topLeft = new();
		Mock<IWindow> topRight = new();
		Mock<IWindow> bottomLeft = new();
		Mock<IWindow> bottomRight = new();

		Dictionary<string, Mock<IWindow>> windows =
			new()
			{
				{ "topLeft", topLeft },
				{ "topRight", topRight },
				{ "bottomLeft", bottomLeft },
				{ "bottomRight", bottomRight }
			};

		LayoutEngineWrapper wrapper = new();

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(topLeft.Object)
			.Add(topRight.Object)
			.AddAtPoint(bottomLeft.Object, new Point<double>() { X = 0.25, Y = 0.9 })
			.AddAtPoint(bottomRight.Object, new Point<double>() { X = 0.75, Y = 0.9 });

		Mock<IMonitor> monitor = new();

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection(edges, pixelDeltas, windows[window].Object);
		IWindowState[] windowStates = result
			.DoLayout(new Location<int>() { Width = 100, Height = 100 }, monitor.Object)
			.ToArray();

		// Then
		Assert.NotSame(engine, result);
		expectedWindowStates.Should().BeEquivalentTo(windowStates);
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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		// When
		ILayoutEngine result = engine.MoveWindowEdgesInDirection((Direction)128, pixelsDeltas, window2.Object);

		// Then
		Assert.Same(engine, result);
	}
}
