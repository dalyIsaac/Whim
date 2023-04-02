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
}
