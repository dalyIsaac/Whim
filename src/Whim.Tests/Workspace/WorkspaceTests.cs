using Moq;
using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
	"Reliability",
	"CA2000:Dispose objects before losing scope",
	Justification = "Unnecessary for tests"
)]
public class WorkspaceTests
{
	private class MocksBuilder
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<ILayoutEngine> LayoutEngine { get; } = new();

		public MocksBuilder()
		{
			Context.Setup(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			LayoutEngine.Setup(l => l.ContainsEqual(LayoutEngine.Object)).Returns(true);
		}
	}

	[Fact]
	public void Rename()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

#pragma warning disable IDE0017 // Simplify object initialization
		Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object);
#pragma warning restore IDE0017 // Simplify object initialization

		// When
#pragma warning disable IDE0017 // Simplify object initialization
		workspace.Name = "Workspace2";
#pragma warning restore IDE0017 // Simplify object initialization

		// Then
		Assert.Equal("Workspace2", workspace.Name);
	}

	[Fact]
	public void TrySetLayoutEngine_CannotFindEngine()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.False(result);
	}

	[Fact]
	public void TrySetLayoutEngine_AlreadyActive()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout");

		// Then
		Assert.True(result);
	}

	[Fact]
	public void TrySetLayoutEngine_Success()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<ILayoutEngine> layoutEngine2 = new();
		layoutEngine2.Setup(e => e.Name).Returns("Layout2");

		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object, layoutEngine2.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
	}

	[Fact]
	public void Constructor_FailWhenNoLayoutEngines()
	{
		// Given
		Mock<IContext> context = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		// When
		// Then
		Assert.Throws<ArgumentException>(() => new Workspace(context.Object, "Workspace"));
	}

	[Fact]
	public void DoLayout_CannotFindMonitorForWorkspace()
	{
		// Given
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);

		Mock<IContext> context = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ILayoutEngine> layoutEngine = new();

		using Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object);

		// When
		workspace.DoLayout();

		// Then
		layoutEngine.Verify(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Never);
	}

	[InlineData(true, 0)]
	[InlineData(false, 1)]
	[Theory]
	public void DoLayout(bool isMouseMoving, int showWindowNoActivateTimes)
	{
		// Given
		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
		nativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns(new Location<int>());

		Mock<IMonitor> monitor = new();
		monitor.SetupGet(m => m.WorkingArea).Returns(new Location<int>());
		monitor.SetupGet(m => m.Bounds).Returns(new Location<int>());

		Mock<IMonitorManager> monitorManager = new();
		monitorManager.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor>() { monitor.Object }.GetEnumerator());

		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(monitor.Object);

		Mock<IContext> context = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);
		context.Setup(c => c.MonitorManager).Returns(monitorManager.Object);
		context.Setup(c => c.NativeManager).Returns(nativeManager.Object);

		Mock<IWindow> window = new();
		window.SetupGet(w => w.IsMouseMoving).Returns(isMouseMoving);

		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine
			.Setup(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()))
			.Returns(
				new WindowState[]
				{
					new()
					{
						Location = new Location<int>(),
						Window = window.Object,
						WindowSize = WindowSize.Normal
					}
				}
			);

		using Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object);

		// When
		workspace.DoLayout();

		// Then
		layoutEngine.Verify(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Once);
		nativeManager.Verify(
			n => n.ShowWindowNoActivate(It.IsAny<HWND>()),
			Times.Exactly(showWindowNoActivateTimes * 2)
		);
		nativeManager.Verify(n => n.EndDeferWindowPos(It.IsAny<HDWP>()), Times.Exactly(2));
	}

	[Fact]
	public void Initialize()
	{
		// Given
		Mock<ProxyLayoutEngine> proxyLayoutEngine = new();
		proxyLayoutEngine.Setup(p => p(It.IsAny<ILayoutEngine>())).Returns((ILayoutEngine e) => e);

		Mock<ProxyLayoutEngine> proxyLayoutEngine2 = new();

		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager
			.SetupGet(m => m.ProxyLayoutEngines)
			.Returns(new ProxyLayoutEngine[] { proxyLayoutEngine.Object, proxyLayoutEngine2.Object });

		Mock<IContext> context = new();
		context.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<ILayoutEngine> layoutEngine2 = new();

		Workspace workspace = new(context.Object, "Workspace", layoutEngine.Object, layoutEngine2.Object);

		// When
		workspace.Initialize();

		// Then
		proxyLayoutEngine.Verify(e => e(layoutEngine.Object), Times.Once);
		proxyLayoutEngine.Verify(e => e(layoutEngine2.Object), Times.Once);
		proxyLayoutEngine2.Verify(e => e(layoutEngine.Object), Times.Once);
		proxyLayoutEngine2.Verify(e => e(layoutEngine2.Object), Times.Once);
	}

	[Fact]
	public void WindowFocused_ContainsWindow()
	{
		// Given the window is in the workspace
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();

		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);

		// When
		workspace.WindowFocused(window.Object);

		// Then
		Assert.Equal(window.Object, workspace.LastFocusedWindow);
	}

	[Fact]
	public void WindowFocused_IsPhantomWindow()
	{
		// Given the window is in the workspace
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();

		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);
		mocks.LayoutEngine.Setup(l => l.ContainsEqual(It.IsAny<ILayoutEngine>())).Returns(true);

		// When
		workspace.WindowFocused(window.Object);

		// Then
		Assert.Equal(window.Object, workspace.LastFocusedWindow);
	}

	[Fact]
	public void FocusFirstWindow()
	{
		// Given
		MocksBuilder mocks = new();
		mocks.LayoutEngine.Setup(l => l.GetFirstWindow()).Returns(new Mock<IWindow>().Object);
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When FocusFirstWindow is called
		workspace.FocusFirstWindow();

		// Then the LayoutEngine's GetFirstWindow method is called
		mocks.LayoutEngine.Verify(l => l.GetFirstWindow(), Times.Once);
	}

	[Fact]
	public void NextLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the next one
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
		mocks.LayoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void NextLayoutEngine_LastEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the first one
		Assert.True(Object.ReferenceEquals(mocks.LayoutEngine.Object, workspace.ActiveLayoutEngine));
		mocks.LayoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void NextLayoutEngine_PhantomWindow()
	{
		// Given the last focused window is a phantom window
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

		Mock<IWindow> phantomWindow = new();
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, phantomWindow.Object);
		workspace.WindowFocused(phantomWindow.Object);

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the next one
		Assert.Null(workspace.LastFocusedWindow);
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
		mocks.LayoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void PreviousLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the previous one
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
		mocks.LayoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void PreviousLayoutEngine_FirstEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the last one
		Assert.True(Object.ReferenceEquals(mocks.LayoutEngine.Object, workspace.ActiveLayoutEngine));
		layoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void PreviousLayoutEngine_PhantomWindow()
	{
		// Given the last focused window is a phantom window
		MocksBuilder mocks = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, new Mock<IWindow>().Object);

		Mock<IWindow> phantomWindow = new();
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, phantomWindow.Object);
		workspace.WindowFocused(phantomWindow.Object);

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the previous one
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
		mocks.LayoutEngine.Verify(l => l.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void AddWindow_Fails_AlreadyIncludesPhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);
		mocks.WorkspaceManager.Reset();

		// Reset mocks
		window.Reset();

		// When AddWindow is called
		workspace.AddWindow(window.Object);

		// Then the window is added to the layout engine
		mocks.LayoutEngine.Verify(l => l.Add(window.Object), Times.Never);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void AddWindow_Fails_AlreadyIncludesWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When AddWindow is called
		workspace.AddWindow(window.Object);
		workspace.AddWindow(window.Object);

		// Then the window is added to the layout engine
		mocks.LayoutEngine.Verify(l => l.Add(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void AddWindow_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When AddWindow is called
		workspace.AddWindow(window.Object);

		// Then the window is added to the layout engine
		mocks.LayoutEngine.Verify(l => l.Add(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void RemoveWindow_Fails_AlreadyRemoved()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When RemoveWindow is called
		workspace.RemoveWindow(window.Object);
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.False(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Never);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Fails_CannotFindPhantomLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Never);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Fails_CannotRemovePhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// Phantom window is added
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);
		mocks.WorkspaceManager.Reset();
		mocks.LayoutEngine.Setup(l => l.Remove(window.Object)).Returns(false);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.False(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Success_RemovesPhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// Phantom window is added
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);
		mocks.WorkspaceManager.Reset();
		mocks.LayoutEngine.Setup(l => l.Remove(window.Object)).Returns(true);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.True(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void RemoveWindow_Fails_DidNotRemoveFromLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		mocks.WorkspaceManager.Reset();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.False(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		workspace.WindowFocused(window.Object);
		mocks.WorkspaceManager.Reset();
		mocks.LayoutEngine.Setup(l => l.Remove(window.Object)).Returns(true);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.True(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is not told to focus the window
		mocks.LayoutEngine.Verify(l => l.FocusWindowInDirection(Direction.Up, window.Object), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is told to focus the window
		mocks.LayoutEngine.Verify(l => l.FocusWindowInDirection(Direction.Up, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection_Fails_WindowIsNull()
	{
		// Given
		MocksBuilder mocks = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, null);

		// Then the layout engine is not told to swap the window
		mocks.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, It.IsAny<IWindow>()), Times.Never);
	}

	[Fact]
	public void SwapWindowInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is not told to swap the window
		mocks.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, window.Object), Times.Never);
	}

	[Fact]
	public void SwapWindowInDirection_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is told to swap the window
		mocks.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, window.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowEdgeInDirection_Fails_WindowIsNull()
	{
		// Given
		MocksBuilder mocks = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		double delta = 0.3;

		// When MoveWindowEdgeInDirection is called
		workspace.MoveWindowEdgeInDirection(Direction.Up, delta, null);

		// Then the layout engine is not told to move the window
		mocks.LayoutEngine.Verify(
			l => l.MoveWindowEdgeInDirection(Direction.Up, delta, It.IsAny<IWindow>()),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgeInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		double delta = 0.3;

		// When MoveWindowEdgeInDirection is called
		workspace.MoveWindowEdgeInDirection(Direction.Up, delta, window.Object);

		// Then the layout engine is not told to move the window
		mocks.LayoutEngine.Verify(l => l.MoveWindowEdgeInDirection(Direction.Up, delta, window.Object), Times.Never);
	}

	[Fact]
	public void MoveWindowEdgeInDirection_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		double delta = 0.3;

		// When MoveWindowEdgeInDirection is called
		workspace.MoveWindowEdgeInDirection(Direction.Up, delta, window.Object);

		// Then the layout engine is told to move the window
		mocks.LayoutEngine.Verify(l => l.MoveWindowEdgeInDirection(Direction.Up, delta, window.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_Success_PhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window.Object, point);

		// Then the layout engine is told to move the window
		mocks.LayoutEngine.Verify(l => l.AddWindowAtPoint(window.Object, point), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window.Object, point);

		// Then the layout engine is told to move the window
		mocks.LayoutEngine.Verify(l => l.AddWindowAtPoint(window.Object, point), Times.Once);
	}

	[Fact]
	public void ToString_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When ToString is called
		string result = workspace.ToString();

		// Then the result is as expected
		Assert.Equal("Workspace", result);
	}

	[Fact]
	public void Deactivate()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> phantomWindow = new();

		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		workspace.AddWindow(window2.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, phantomWindow.Object);
		mocks.WorkspaceManager.Reset();

		// When Deactivate is called
		workspace.Deactivate();

		// Then the windows are hidden and DoLayout is called
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
		window.Verify(w => w.Hide(), Times.Once);
		window2.Verify(w => w.Hide(), Times.Once);
		phantomWindow.Verify(w => w.Hide(), Times.Once);
	}

	[Fact]
	public void TryGetWindowLocation()
	{
		// Given
		MocksBuilder mocks = new();

		// Set up DoLayout so it stores the window location.
		Mock<IMonitor> monitor = new();
		monitor
			.Setup(m => m.WorkingArea)
			.Returns(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 100,
					Height = 100
				}
			);

		mocks.WorkspaceManager.Setup(wm => wm.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(monitor.Object);

		Mock<INativeManager> nativeManager = new();
		nativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
		nativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns(new Location<int>());

		Mock<IMonitorManager> monitorManager = new();
		monitorManager.Setup(m => m.GetEnumerator()).Returns(new List<IMonitor>() { monitor.Object }.GetEnumerator());

		mocks.Context.Setup(c => c.NativeManager).Returns(nativeManager.Object);
		mocks.Context.Setup(c => c.MonitorManager).Returns(monitorManager.Object);

		Mock<IWindow> window = new();

		mocks.LayoutEngine
			.Setup(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()))
			.Returns(
				new WindowState[]
				{
					new()
					{
						Location = new Location<int>(),
						Window = window.Object,
						WindowSize = WindowSize.Normal
					}
				}
			);

		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);

		// When TryGetWindowLocation is called
		IWindowState? result = workspace.TryGetWindowLocation(window.Object);

		// Then the result is as expected
		Assert.NotNull(result);
	}

	[Fact]
	public void AddPhantomWindow_Fails_DoesNotContainLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When AddPhantomWindow is called
		workspace.AddPhantomWindow(new Mock<ILayoutEngine>().Object, window.Object);

		// Then the window is not added
		mocks.WorkspaceManager.Verify(wm => wm.AddPhantomWindow(workspace, window.Object), Times.Never);
	}

	[Fact]
	public void AddPhantomWindow_AlreadyContainsPhantomWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// When AddPhantomWindow is called
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// Then the window is not added
		mocks.WorkspaceManager.Verify(wm => wm.AddPhantomWindow(workspace, window.Object), Times.Once);
	}

	[Fact]
	public void AddPhantomWindow_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When AddPhantomWindow is called
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// Then the window is added
		mocks.WorkspaceManager.Verify(wm => wm.AddPhantomWindow(workspace, window.Object), Times.Once);
	}

	[Fact]
	public void RemovePhantomWindow_Fails_DoesNotContainLayoutEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);

		// When RemovePhantomWindow is called
		workspace.RemovePhantomWindow(new Mock<ILayoutEngine>().Object, window.Object);

		// Then the window is not removed
		mocks.WorkspaceManager.Verify(wm => wm.RemovePhantomWindow(window.Object), Times.Never);
	}

	[Fact]
	public void RemovePhantomWindow_Fails_DoesNotContainWindow()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// When RemovePhantomWindow is called
		workspace.RemovePhantomWindow(mocks.LayoutEngine.Object, new Mock<IWindow>().Object);

		// Then the window is not removed
		mocks.WorkspaceManager.Verify(wm => wm.RemovePhantomWindow(window.Object), Times.Never);
	}

	[Fact]
	public void RemovePhantomWindow_Fails_GetsWrongPhantomEngine()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Mock<ILayoutEngine> altLayoutEngine = new();

		// Lie to hit the wrong branch in RemovePhantomWindow
		mocks.LayoutEngine.Setup(e => e.ContainsEqual(altLayoutEngine.Object)).Returns(true);

		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object, altLayoutEngine.Object);
		workspace.AddPhantomWindow(altLayoutEngine.Object, window.Object);
		mocks.LayoutEngine.Setup(e => e.ContainsEqual(mocks.LayoutEngine.Object)).Returns(true);

		// When RemovePhantomWindow is called
		workspace.RemovePhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// Then the window is not removed
		mocks.WorkspaceManager.Verify(wm => wm.RemovePhantomWindow(window.Object), Times.Never);
	}

	[Fact]
	public void RemovePhantomWindow_Success()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddPhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// When RemovePhantomWindow is called
		workspace.RemovePhantomWindow(mocks.LayoutEngine.Object, window.Object);

		// Then the window is removed
		mocks.WorkspaceManager.Verify(wm => wm.RemovePhantomWindow(window.Object), Times.Once);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		MocksBuilder mocks = new();
		Mock<IWindow> window = new();
		Workspace workspace = new(mocks.Context.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);

		// When Dispose is called
		workspace.Dispose();

		// Then the window is minimized
		window.Verify(w => w.ShowMinimized(), Times.Once);
	}
}
