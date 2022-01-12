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
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();
		engine.Add(CreateWindow());

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] result = engine.DoLayout(location).ToArray();

		Assert.Single(result);

		IWindowLocation windowLocation = result[0];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(1920, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}

	[Fact]
	public void DoLayout_LeftToRight_MultipleWindows()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();
		engine.Add(CreateWindow());
		engine.Add(CreateWindow());
		engine.Add(CreateWindow());

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] result = engine.DoLayout(location).ToArray();

		Assert.Equal(3, result.Length);

		IWindowLocation windowLocation = result[0];
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
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);
		engine.Add(CreateWindow());

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] result = engine.DoLayout(location).ToArray();

		Assert.Single(result);

		IWindowLocation windowLocation = result[0];
		Assert.Equal(0, windowLocation.Location.X);
		Assert.Equal(0, windowLocation.Location.Y);
		Assert.Equal(1920, windowLocation.Location.Width);
		Assert.Equal(1080, windowLocation.Location.Height);
	}

	[Fact]
	public void DoLayout_RightToLeft_MultipleWindows()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);
		engine.Add(CreateWindow());
		engine.Add(CreateWindow());
		engine.Add(CreateWindow());

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] result = engine.DoLayout(location).ToArray();

		Assert.Equal(3, result.Length);

		IWindowLocation windowLocation = result[0];
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
		Logger.Initialize();
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Top, leftWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_WindowNotFound()
	{
		Logger.Initialize();
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> otherWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Left, otherWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Never);
		rightWindow.Verify(w => w.Focus(), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRight()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Right, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusRightWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Right, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeft()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Left, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_LeftToRight_FocusLeftWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Left, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeft()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Left, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusLeftWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Left, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRight()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Right, rightWindow.Object);

		leftWindow.Verify(w => w.Focus(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_RightToLeft_FocusRightWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.FocusWindowInDirection(WindowDirection.Right, leftWindow.Object);

		rightWindow.Verify(w => w.Focus(), Times.Once);
	}
	#endregion

	#region SwapWindowInDirection
	[Fact]
	public void SwapWindowInDirection_IgnoreIllegalDirection()
	{
		Logger.Initialize();
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Top, leftWindow.Object);

		IWindowLocation[] windows = engine.DoLayout(new Location(0, 0, 1920, 1080)).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_WindowNotFound()
	{
		Logger.Initialize();
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> notFoundWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Left, notFoundWindow.Object);

		IWindowLocation[] windows = engine.DoLayout(new Location(0, 0, 1920, 1080)).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRight()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Right, leftWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapRightWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Right, rightWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeft()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Left, rightWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_LeftToRight_SwapLeftWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new();

		Mock<IWindow> leftWindow = CreateMockWindow();
		Mock<IWindow> rightWindow = CreateMockWindow();

		engine.Add(leftWindow.Object);
		engine.Add(rightWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Left, leftWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(rightWindow.Object, windows[0].Window);
		Assert.Equal(0, windows[0].Location.X);

		Assert.Equal(leftWindow.Object, windows[1].Window);
		Assert.Equal(960, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeft()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Left, leftWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapLeftWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Left, rightWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRight()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Right, rightWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}

	[Fact]
	public void SwapWindowInDirection_RightToLeft_SwapRightWrapAround()
	{
		Logger.Initialize(new LoggerConfig());
		ColumnLayoutEngine engine = new(leftToRight: false);

		Mock<IWindow> rightWindow = CreateMockWindow();
		Mock<IWindow> leftWindow = CreateMockWindow();

		engine.Add(rightWindow.Object);
		engine.Add(leftWindow.Object);

		engine.SwapWindowInDirection(WindowDirection.Right, leftWindow.Object);

		Location location = new(0, 0, 1920, 1080);
		IWindowLocation[] windows = engine.DoLayout(location).ToArray();

		Assert.Equal(2, windows.Length);

		Assert.Equal(leftWindow.Object, windows[0].Window);
		Assert.Equal(960, windows[0].Location.X);

		Assert.Equal(rightWindow.Object, windows[1].Window);
		Assert.Equal(0, windows[1].Location.X);
	}
	#endregion
}