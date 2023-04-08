using Moq;
using System;
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
		public Mock<IConfigContext> ConfigContext { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<ILayoutEngine> LayoutEngine { get; } = new();

		public MocksBuilder()
		{
			ConfigContext.Setup(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			LayoutEngine.Setup(l => l.ContainsEqual(LayoutEngine.Object)).Returns(true);
		}
	}

	[Fact]
	public void Rename()
	{
		// Given
		Mock<ILayoutEngine> layoutEngine = new();
		layoutEngine.Setup(e => e.Name).Returns("Layout");

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

#pragma warning disable IDE0017 // Simplify object initialization
		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);
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

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

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

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

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

		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object, layoutEngine2.Object);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
	}

	[Fact]
	public void Constructor_FailWhenNoLayoutEngines()
	{
		// Given
		Mock<IConfigContext> configContext = new();
		Mock<IWorkspaceManager> workspaceManager = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		// When
		// Then
		Assert.Throws<ArgumentException>(() => new Workspace(configContext.Object, "Workspace"));
	}

	[Fact]
	public void DoLayout_CannotFindMonitorForWorkspace()
	{
		// Given
		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ILayoutEngine> layoutEngine = new();

		using Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

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

		Mock<IWorkspaceManager> workspaceManager = new();
		workspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(monitor.Object);

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);
		configContext.Setup(c => c.NativeManager).Returns(nativeManager.Object);

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

		using Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object);

		// When
		workspace.DoLayout();

		// Then
		layoutEngine.Verify(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Once);
		nativeManager.Verify(n => n.ShowWindowNoActivate(It.IsAny<HWND>()), Times.Exactly(showWindowNoActivateTimes));
		nativeManager.Verify(n => n.EndDeferWindowPos(It.IsAny<HDWP>()), Times.Once);
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

		Mock<IConfigContext> configContext = new();
		configContext.Setup(c => c.WorkspaceManager).Returns(workspaceManager.Object);

		Mock<ILayoutEngine> layoutEngine = new();
		Mock<ILayoutEngine> layoutEngine2 = new();

		Workspace workspace = new(configContext.Object, "Workspace", layoutEngine.Object, layoutEngine2.Object);

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

		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);
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

		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);
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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);

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
		Workspace workspace =
			new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object, layoutEngine.Object);
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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);

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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);
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
		Workspace workspace = new(mocks.ConfigContext.Object, "Workspace", mocks.LayoutEngine.Object);
		workspace.AddWindow(window.Object);
		mocks.WorkspaceManager.Reset();
		mocks.LayoutEngine.Setup(l => l.Remove(window.Object)).Returns(true);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.True(result);
		mocks.LayoutEngine.Verify(l => l.Remove(window.Object), Times.Once);
		mocks.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}
}
