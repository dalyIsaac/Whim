using Moq;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewWindowTests
{
	#region ShouldContinue
	[Fact]
	public void ShouldContinue_DifferentLength()
	{
		// Given
		IWindowState[] prevWindowStates = Array.Empty<IWindowState>();
		int prevHoveredIndex = -1;
		IWindowState[] windowStates = new IWindowState[1];
		IPoint<int> cursorPoint = new Location<int>();

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_DifferentWindowState()
	{
		// Given
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = -1;
		IWindowState[] windowStates = new IWindowState[]
		{
			prevWindowStates[0],
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Maximized
			},
		};
		IPoint<int> cursorPoint = new Location<int>();

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_HoveredIndexChanged()
	{
		// Given
		Location<int> location = new() { Height = 100, Width = 100 };
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = location,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = 0;
		IWindowState[] windowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = location,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		IPoint<int> cursorPoint = new Location<int>() { X = 100, Y = 101 };

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			windowStates,
			cursorPoint
		);

		// Then
		Assert.True(shouldContinue);
	}

	[Fact]
	public void ShouldContinue_HoveredIndexNotChanged()
	{
		// Given
		Location<int> location = new() { Height = 100, Width = 100 };
		IWindowState[] prevWindowStates = new IWindowState[]
		{
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = location,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Window = new Mock<IWindow>().Object,
				Location = new Location<int>(),
				WindowSize = WindowSize.Normal
			},
		};
		int prevHoveredIndex = 0;
		IPoint<int> cursorPoint = new Location<int>() { X = 50, Y = 50 };

		// When
		bool shouldContinue = LayoutPreviewWindow.ShouldContinue(
			prevWindowStates,
			prevHoveredIndex,
			prevWindowStates,
			cursorPoint
		);

		// Then
		Assert.False(shouldContinue);
	}
	#endregion

	[Fact]
	public void Activate()
	{
		// Given
		Mock<INativeManager> nativeManagerMock = new();
		Mock<IMonitorManager> monitorManagerMock = new();
		Mock<IContext> contextMock = new();
		Mock<IWindow> layoutWindowMock = new();
		Mock<IWindow> movingWindowMock = new();
		Mock<IMonitor> monitorMock = new();

		contextMock.SetupGet(context => context.NativeManager).Returns(nativeManagerMock.Object);
		contextMock.SetupGet(context => context.MonitorManager).Returns(monitorManagerMock.Object);

		monitorManagerMock
			.Setup(mm => mm.GetEnumerator())
			.Returns(new List<IMonitor>() { monitorMock.Object }.GetEnumerator());

		// When
		LayoutPreviewWindow.Activate(
			contextMock.Object,
			layoutWindowMock.Object,
			movingWindowMock.Object,
			monitorMock.Object
		);

		// Then
		nativeManagerMock.Verify(nativeManager => nativeManager.BeginDeferWindowPos(1), Times.Exactly(2));
	}
}
