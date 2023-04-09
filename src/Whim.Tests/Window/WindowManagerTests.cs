using Moq;
using System;
using Windows.Win32;
using Windows.Win32.UI.Accessibility;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WindowManagerTests
{
	[Fact]
	public void OnWindowAdded()
	{
		// Given
		Mock<IContext> context = new();

		Mock<WorkspaceManager> workspaceManager = new(context.Object);
		workspaceManager.Setup(m => m.WindowAdded(It.IsAny<IWindow>()));

		context.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowAdded += eh,
			eh => windowManager.WindowAdded -= eh,
			() => windowManager.OnWindowAdded(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowAdded(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowFocused()
	{
		// Given
		Mock<IContext> context = new();
		Mock<ICoreNativeManager> coreNativeManager = new();
		Mock<IWindowMessageMonitor> windowMessageMonitor = new();

		Mock<WorkspaceManager> workspaceManager = new(context.Object);
		workspaceManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));

		Mock<MonitorManager> monitorManager =
			new(context.Object, coreNativeManager.Object, windowMessageMonitor.Object);
		monitorManager.Setup(m => m.WindowFocused(It.IsAny<IWindow>()));

		context.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);
		context.SetupGet(m => m.MonitorManager).Returns(monitorManager.Object);

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowFocused += eh,
			eh => windowManager.WindowFocused -= eh,
			() => windowManager.OnWindowFocused(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		monitorManager.Verify(m => m.WindowFocused(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowRemoved()
	{
		// Given
		Mock<IContext> context = new();

		Mock<WorkspaceManager> workspaceManager = new(context.Object);
		workspaceManager.Setup(m => m.WindowRemoved(It.IsAny<IWindow>()));

		context.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowRemoved += eh,
			eh => windowManager.WindowRemoved -= eh,
			() => windowManager.OnWindowRemoved(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowRemoved(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowMinimizeStart()
	{
		// Given
		Mock<IContext> context = new();

		Mock<WorkspaceManager> workspaceManager = new(context.Object);
		workspaceManager.Setup(m => m.WindowMinimizeStart(It.IsAny<IWindow>()));

		context.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeStart += eh,
			eh => windowManager.WindowMinimizeStart -= eh,
			() => windowManager.OnWindowMinimizeStart(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowMinimizeStart(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	[Fact]
	public void OnWindowMinimizeEnd()
	{
		// Given
		Mock<IContext> context = new();

		Mock<WorkspaceManager> workspaceManager = new(context.Object);
		workspaceManager.Setup(m => m.WindowMinimizeEnd(It.IsAny<IWindow>()));

		context.SetupGet(m => m.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ICoreNativeManager> coreNativeManager = new();

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		Mock<IWindow> window = new();

		// When
		var result = Assert.Raises<WindowEventArgs>(
			eh => windowManager.WindowMinimizeEnd += eh,
			eh => windowManager.WindowMinimizeEnd -= eh,
			() => windowManager.OnWindowMinimizeEnd(window.Object)
		);

		// Then
		workspaceManager.Verify(m => m.WindowMinimizeEnd(window.Object), Times.Once);
		Assert.Equal(window.Object, result.Arguments.Window);
	}

	private static Mock<ICoreNativeManager> CreateInitializeCoreNativeManagerMock()
	{
		Mock<ICoreNativeManager> coreNativeManager = new();

		(uint, uint)[] events = new[]
		{
			(PInvoke.EVENT_OBJECT_DESTROY, PInvoke.EVENT_OBJECT_HIDE),
			(PInvoke.EVENT_OBJECT_CLOAKED, PInvoke.EVENT_OBJECT_UNCLOAKED),
			(PInvoke.EVENT_SYSTEM_MOVESIZESTART, PInvoke.EVENT_SYSTEM_MOVESIZEEND),
			(PInvoke.EVENT_SYSTEM_FOREGROUND, PInvoke.EVENT_SYSTEM_FOREGROUND),
			(PInvoke.EVENT_OBJECT_LOCATIONCHANGE, PInvoke.EVENT_OBJECT_LOCATIONCHANGE),
			(PInvoke.EVENT_SYSTEM_MINIMIZESTART, PInvoke.EVENT_SYSTEM_MINIMIZEEND)
		};

		foreach (var (eventMin, eventMax) in events)
		{
			coreNativeManager
				.Setup(n => n.SetWinEventHook(eventMin, eventMax, It.IsAny<WINEVENTPROC>()))
				.Returns(new UnhookWinEventSafeHandle(1));
		}

		return coreNativeManager;
	}

	[Fact]
	public void Initialize()
	{
		// Given
		Mock<IContext> context = new();
		Mock<ICoreNativeManager> coreNativeManager = CreateInitializeCoreNativeManagerMock();
		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		// When
		windowManager.Initialize();

		// Then
		coreNativeManager.Verify(
			n => n.SetWinEventHook(It.IsAny<uint>(), It.IsAny<uint>(), It.IsAny<WINEVENTPROC>()),
			Times.Exactly(6)
		);
	}

	[Fact]
	public void Initialize_Fail()
	{
		// Given
		Mock<IContext> context = new();

		Mock<ICoreNativeManager> coreNativeManager = CreateInitializeCoreNativeManagerMock();
		coreNativeManager
			.Setup(
				n =>
					n.SetWinEventHook(
						PInvoke.EVENT_SYSTEM_MINIMIZESTART,
						PInvoke.EVENT_SYSTEM_MINIMIZEEND,
						It.IsAny<WINEVENTPROC>()
					)
			)
			.Returns(new UnhookWinEventSafeHandle());

		WindowManager windowManager = new(context.Object, coreNativeManager.Object);

		// When
		// Then
		Assert.Throws<InvalidOperationException>(windowManager.Initialize);
	}
}
