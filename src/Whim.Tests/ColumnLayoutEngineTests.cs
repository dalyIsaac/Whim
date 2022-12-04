using Moq;
using System;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class ColumnLayoutEngineTests
{
	private static readonly Random rnd = new();

	private static Mock<IWindow> CreateMockWindow()
	{
		Mock<IWindow> window = new();
		window.Setup(w => w.Handle).Returns(new Windows.Win32.Foundation.HWND(rnd.Next()));
		return window;
	}

	private static IWindow CreateWindow() => CreateMockWindow().Object;

	#region DoLayout
	[Fact]
	public void DoLayout_LeftToRight_SingleWindow()
	{
		ColumnLayoutEngine engine = new() { CreateWindow() };

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Height = 1920,
				Width = 1080
			};
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Single(result);

		IWindowState windowLocation = result[0];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(1920, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}

	[Fact]
	public void DoLayout_LeftToRight_MultipleWindows()
	{
		ColumnLayoutEngine engine = new() { CreateWindow(), CreateWindow(), CreateWindow() };

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Height = 1920,
				Width = 1080
			};
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(3, result.Length);

		IWindowState windowLocation = result[0];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);

		windowLocation = result[1];
		Assert.Equal(640, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);

		windowLocation = result[2];
		Assert.Equal(1280, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}

	[Fact]
	public void DoLayout_RightToLeft_SingleWindow()
	{
		ColumnLayoutEngine engine = new(leftToRight: false) { CreateWindow() };

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Single(result);

		IWindowState windowLocation = result[0];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(1920, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}

	[Fact]
	public void DoLayout_RightToLeft_MultipleWindows()
	{
		ColumnLayoutEngine engine = new(leftToRight: false) { CreateWindow(), CreateWindow(), CreateWindow() };

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] result = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(3, result.Length);

		IWindowState windowLocation = result[0];
		Assert.Equal(1280, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);

		windowLocation = result[1];
		Assert.Equal(640, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);

		windowLocation = result[2];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(640, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}
	#endregion

	#region FocusWindowInDirection
	[Fact]
	public void FocusWindowInDirection_IgnoreIllegalDirection()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Up, leftWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_WindowNotFound()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> otherWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Left, otherWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRight()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Right, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRightWrapAround()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Right, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeft()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Left, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeftWrapAround()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Left, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeft()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Left, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeftWrapAround()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Left, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRight()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Right, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRightWrapAround()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(Direction.Right, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}
	#endregion

	#region SwapWindowInDirection
	[Fact]
	public void SwapWindowInDirection_IgnoreIllegalDirection()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Up, leftWindow.Object);

		IWindowState[] windows = engine
			.DoLayout(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080
				},
				new Mock<IMonitor>().Object
			)
			.ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_WindowNotFound()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> notFoundWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Left, notFoundWindow.Object);

		IWindowState[] windows = engine
			.DoLayout(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080
				},
				new Mock<IMonitor>().Object
			)
			.ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRight()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Right, leftWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRightWrapAround()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Right, rightWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeft()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Left, rightWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeftWrapAround()
	{
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(Direction.Left, leftWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeft()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(Direction.Left, leftWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeftWrapAround()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(Direction.Left, rightWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRight()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(Direction.Right, rightWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRightWrapAround()
	{
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(Direction.Right, leftWindow.Object);

		Location<int> location =
			new()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			};
		IWindowState[] windows = engine.DoLayout(location, new Mock<IMonitor>().Object).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}
	#endregion
}
