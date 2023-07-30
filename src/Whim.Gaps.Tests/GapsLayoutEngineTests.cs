using FluentAssertions;
using Moq;
using Xunit;

namespace Whim.Gaps.Tests;

public class GapsLayoutEngineTests
{
	private static readonly LayoutEngineIdentity _identity = new();

	public static IEnumerable<object[]> DoLayout_Data()
	{
		yield return new object[]
		{
			new GapsConfig() { OuterGap = 10, InnerGap = 5 },
			1,
			100,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 10 + 5,
						Y = 10 + 5,
						Width = 1920 - (10 * 2) - (5 * 2),
						Height = 1080 - (10 * 2) - (5 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		yield return new object[]
		{
			new GapsConfig() { OuterGap = 10, InnerGap = 5 },
			2,
			100,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
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
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 960 + 5,
						Y = 10 + 5,
						Width = 960 - 10 - (5 * 2),
						Height = 1080 - (10 * 2) - (5 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};

		yield return new object[]
		{
			new GapsConfig { OuterGap = 10, InnerGap = 5 },
			1,
			150,
			new IWindowState[]
			{
				new WindowState()
				{
					Window = new Mock<IWindow>().Object,
					Location = new Location<int>()
					{
						X = 15 + 7,
						Y = 15 + 7,
						Width = 1920 - (15 * 2) - (7 * 2),
						Height = 1080 - (15 * 2) - (7 * 2)
					},
					WindowSize = WindowSize.Normal
				}
			}
		};
	}

	[Theory]
	[MemberData(nameof(DoLayout_Data))]
	public void DoLayout(GapsConfig gapsConfig, int windowsCount, int scale, IWindowState[] expectedWindowStates)
	{
		// Given
		ILayoutEngine innerLayoutEngine = new ColumnLayoutEngine(_identity);

		for (int i = 0; i < windowsCount; i++)
		{
			innerLayoutEngine = innerLayoutEngine.AddWindow(new Mock<IWindow>().Object);
		}

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};

		Mock<IMonitor> monitor = new();
		monitor.Setup(m => m.ScaleFactor).Returns(scale);

		// When
		IWindowState[] windowStates = gapsLayoutEngine.DoLayout(location, monitor.Object).ToArray();

		// Then
		windowStates.Should().BeEquivalentTo(expectedWindowStates);
	}

	[Fact]
	public void Count()
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.Count).Returns(5);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When

		// Then
		Assert.Equal(5, gapsLayoutEngine.Count);
		innerLayoutEngine.Verify(ile => ile.Count, Times.Once);
	}

	[Fact]
	public void ContainsWindow()
	{
		// Given
		Mock<IWindow> window = new();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.ContainsWindow(It.IsAny<IWindow>())).Returns(true);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		bool containsWindow = gapsLayoutEngine.ContainsWindow(window.Object);

		// Then
		Assert.True(containsWindow);
		innerLayoutEngine.Verify(ile => ile.ContainsWindow(window.Object), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.FocusWindowInDirection(direction, window.Object));

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		gapsLayoutEngine.FocusWindowInDirection(direction, window.Object);

		// Then
		innerLayoutEngine.Verify(ile => ile.FocusWindowInDirection(direction, window.Object), Times.Once);
	}

	[Fact]
	public void HidePhantomWindows()
	{
		// Given
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.HidePhantomWindows());

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		gapsLayoutEngine.HidePhantomWindows();

		// Then
		innerLayoutEngine.Verify(ile => ile.HidePhantomWindows(), Times.Once);
	}

	#region Add
	[Fact]
	public void Add_Same()
	{
		// Given
		Mock<IWindow> window = new();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.AddWindow(window.Object)).Returns(innerLayoutEngine.Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.AddWindow(window.Object);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Add_NotSame()
	{
		// Given
		Mock<IWindow> window = new();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.AddWindow(window.Object)).Returns(new Mock<ILayoutEngine>().Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.AddWindow(window.Object);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.AddWindow(window.Object), Times.Once);
	}
	#endregion

	#region Remove
	[Fact]
	public void Remove_Same()
	{
		// Given
		Mock<IWindow> window = new();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.RemoveWindow(window.Object)).Returns(innerLayoutEngine.Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.RemoveWindow(window.Object);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Remove_NotSame()
	{
		// Given
		Mock<IWindow> window = new();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.RemoveWindow(window.Object)).Returns(new Mock<ILayoutEngine>().Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.RemoveWindow(window.Object);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.RemoveWindow(window.Object), Times.Once);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Fact]
	public void MoveWindowEdgesInDirection_Same()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object))
			.Returns(innerLayoutEngine.Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_NotSame()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		IPoint<double> deltas = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object))
			.Returns(new Mock<ILayoutEngine>().Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowEdgesInDirection(direction, deltas, window.Object);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.MoveWindowEdgesInDirection(direction, deltas, window.Object), Times.Once);
	}
	#endregion

	#region MoveWindowToPoint
	[Fact]
	public void MoveWindowToPoint_Same()
	{
		// Given
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(ile => ile.MoveWindowToPoint(window.Object, point)).Returns(innerLayoutEngine.Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowToPoint(window.Object, point);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, point), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_NotSame()
	{
		// Given
		Mock<IWindow> window = new();
		IPoint<double> point = new Point<double>();
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(ile => ile.MoveWindowToPoint(window.Object, point))
			.Returns(new Mock<ILayoutEngine>().Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.MoveWindowToPoint(window.Object, point);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.MoveWindowToPoint(window.Object, point), Times.Once);
	}
	#endregion

	#region SwapWindowInDirection
	[Fact]
	public void SwapWindowInDirection_Same()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(ile => ile.SwapWindowInDirection(direction, window.Object))
			.Returns(innerLayoutEngine.Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.SwapWindowInDirection(direction, window.Object);

		// Then
		Assert.Same(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(direction, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection_NotSame()
	{
		// Given
		Mock<IWindow> window = new();
		Direction direction = Direction.Left;
		GapsConfig gapsConfig = new() { OuterGap = 10, InnerGap = 5 };
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(ile => ile.SwapWindowInDirection(direction, window.Object))
			.Returns(new Mock<ILayoutEngine>().Object);

		GapsLayoutEngine gapsLayoutEngine = new(gapsConfig, innerLayoutEngine.Object);

		// When
		ILayoutEngine newLayoutEngine = gapsLayoutEngine.SwapWindowInDirection(direction, window.Object);

		// Then
		Assert.NotSame(gapsLayoutEngine, newLayoutEngine);
		innerLayoutEngine.Verify(ile => ile.SwapWindowInDirection(direction, window.Object), Times.Once);
	}
	#endregion
}
