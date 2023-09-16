using Microsoft.UI.Dispatching;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
	private class Wrapper
	{
		public Mock<IContext> Context { get; } = new();
		public Mock<IInternalContext> InternalContext { get; } = new();
		public Mock<IInternalWindowManager> InternalWindowManager { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<ILayoutEngine> LayoutEngine { get; } = new();
		public Mock<INativeManager> NativeManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<ICoreNativeManager> CoreNativeManager { get; } = new();
		public Mock<Action<ActiveLayoutEngineChangedEventArgs>> TriggerActiveLayoutEngineChanged = new();
		public Mock<Action<WorkspaceRenamedEventArgs>> TriggerWorkspaceRenamed = new();
		public Mock<Action<WorkspaceEventArgs>> TriggerWorkspaceLayoutStarted = new();
		public Mock<Action<WorkspaceEventArgs>> TriggerWorkspaceLayoutCompleted = new();
		public WorkspaceManagerTriggers Triggers { get; }

		public Wrapper()
		{
			Context.Setup(c => c.WorkspaceManager).Returns(WorkspaceManager.Object);
			Context.Setup(c => c.NativeManager).Returns(NativeManager.Object);
			Context.Setup(c => c.MonitorManager).Returns(MonitorManager.Object);
			Context.Setup(c => c.WindowManager).Returns(InternalWindowManager.As<IWindowManager>().Object);

			InternalContext.Setup(ic => ic.CoreNativeManager).Returns(CoreNativeManager.Object);
			InternalContext.SetupGet(x => x.LayoutLock).Returns(new ReaderWriterLockSlim());

			LayoutEngine.Setup(l => l.ContainsEqual(LayoutEngine.Object)).Returns(true);
			LayoutEngine.Setup(l => l.Name).Returns("Layout");

			// This isn't strictly correct, but it's good enough for testing
			LayoutEngine.Setup(l => l.AddWindow(It.IsAny<IWindow>())).Returns(LayoutEngine.Object);
			LayoutEngine.Setup(l => l.RemoveWindow(It.IsAny<IWindow>())).Returns(LayoutEngine.Object);
			LayoutEngine
				.Setup(
					l =>
						l.MoveWindowEdgesInDirection(
							It.IsAny<Direction>(),
							It.IsAny<IPoint<double>>(),
							It.IsAny<IWindow>()
						)
				)
				.Returns(LayoutEngine.Object);
			LayoutEngine
				.Setup(l => l.SwapWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()))
				.Returns(LayoutEngine.Object);

			MonitorManager
				.Setup(m => m.GetEnumerator())
				.Returns(new List<IMonitor>() { Monitor.Object }.GetEnumerator());

			WorkspaceManager.Setup(wm => wm.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(Monitor.Object);

			Monitor
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

			NativeManager.Setup(n => n.BeginDeferWindowPos(It.IsAny<int>())).Returns((HDWP)1);
			NativeManager.Setup(n => n.GetWindowOffset(It.IsAny<HWND>())).Returns(new Location<int>());

			Triggers = new()
			{
				ActiveLayoutEngineChanged = TriggerActiveLayoutEngineChanged.Object,
				WorkspaceRenamed = TriggerWorkspaceRenamed.Object,
				WorkspaceLayoutStarted = TriggerWorkspaceLayoutStarted.Object,
				WorkspaceLayoutCompleted = TriggerWorkspaceLayoutCompleted.Object
			};

			Setup_ExecuteTask();
		}

		public Wrapper Setup_PassGarbageCollection()
		{
			CoreNativeManager.Setup(c => c.IsWindow(It.IsAny<HWND>())).Returns(true);

			Mock<IWindow> window = new();
			window.Setup(w => w.Equals(It.IsAny<IWindow>())).Returns(true);

			InternalWindowManager.Setup(wm => wm.Windows.ContainsKey(It.IsAny<HWND>())).Returns(true);

			return this;
		}

		public Wrapper Setup_ExecuteTask()
		{
			CoreNativeManager
				.Setup(c => c.ExecuteTask(It.IsAny<Func<DispatcherQueueHandler>>(), It.IsAny<CancellationToken>()))
				.Callback<Func<DispatcherQueueHandler>, CancellationToken>(
					(task, _) =>
					{
						var result = task();
						result.Invoke();
					}
				);

			return this;
		}
	}

	[Fact]
	[System.Diagnostics.CodeAnalysis.SuppressMessage(
		"Style",
		"IDE0017:Simplify object initialization",
		Justification = "It's a test"
	)]
	public void Rename()
	{
		// Given
		Wrapper wrapper = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.Name = "Workspace2";

		// Then
		Assert.Equal("Workspace2", workspace.Name);
	}

	[Fact]
	public void TrySetLayoutEngine_CannotFindEngine()
	{
		// Given
		Wrapper wrapper = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.False(result);
	}

	[Fact]
	public void TrySetLayoutEngine_AlreadyActive()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.WorkspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout");

		// Then
		Assert.True(result);
	}

	[Fact]
	public void TrySetLayoutEngine_Success()
	{
		// Given
		Wrapper wrapper = new();

		Mock<ILayoutEngine> layoutEngine2 = new();
		layoutEngine2.Setup(e => e.Name).Returns("Layout2");

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object, layoutEngine2.Object }
			);

		// When
		bool result = workspace.TrySetLayoutEngine("Layout2");

		// Then
		Assert.True(result);
	}

	[Fact]
	public void Constructor_FailWhenNoLayoutEngines()
	{
		// Given
		Wrapper wrapper = new();

		// When
		// Then
		Assert.Throws<ArgumentException>(
			() =>
				new Workspace(
					wrapper.Context.Object,
					wrapper.InternalContext.Object,
					wrapper.Triggers,
					"Workspace",
					Array.Empty<ILayoutEngine>()
				)
		);
	}

	#region DoLayout
	[Fact]
	public void DoLayout_CannotFindMonitorForWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.WorkspaceManager.Setup(m => m.GetMonitorForWorkspace(It.IsAny<IWorkspace>())).Returns(null as IMonitor);

		using Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.DoLayout();

		// Then
		wrapper.LayoutEngine.Verify(e => e.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Never);
		wrapper.TriggerWorkspaceLayoutStarted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Never);
		wrapper.TriggerWorkspaceLayoutCompleted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Never);
	}

	[Fact]
	public void DoLayout_MinimizedWindow()
	{
		// Given
		Wrapper wrapper = new Wrapper().Setup_PassGarbageCollection();

		Mock<IWindow> window = new();

		using Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.AddWindow(window.Object);
		wrapper.TriggerWorkspaceLayoutStarted.Invocations.Clear();
		wrapper.TriggerWorkspaceLayoutCompleted.Invocations.Clear();
		workspace.WindowMinimizeStart(window.Object);

		// Then
		wrapper.NativeManager.Verify(n => n.ShowWindowNoActivate(It.IsAny<HWND>()), Times.Never);
		window.Verify(w => w.ShowMinimized(), Times.Once);
		wrapper.TriggerWorkspaceLayoutStarted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Once);
		wrapper.TriggerWorkspaceLayoutCompleted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Once);
	}

	[Fact]
	public void DoLayout_GarbageCollect_IsNotAWindow()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWindow> window = new();

		using Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		wrapper.CoreNativeManager.Setup(c => c.IsWindow(It.IsAny<HWND>())).Returns(false);

		// When
		workspace.AddWindow(window.Object);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
		wrapper.TriggerWorkspaceLayoutStarted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Never);
	}

	[Fact]
	public void DoLayout_GarbageCollect_HandleIsNotManaged()
	{
		// Given
		Wrapper wrapper = new();

		Mock<IWindow> window = new();

		using Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		wrapper.CoreNativeManager.Setup(c => c.IsWindow(It.IsAny<HWND>())).Returns(true);
		wrapper.InternalWindowManager
			.Setup(wm => wm.Windows.TryGetValue(It.IsAny<HWND>(), out It.Ref<IWindow?>.IsAny))
			.Returns(false);

		// When
		workspace.AddWindow(window.Object);

		// Then
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
		wrapper.TriggerWorkspaceLayoutStarted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Never);
	}

	[Fact]
	public void DoLayout_TakeLatestDoLayout()
	{
		// Given
		Wrapper wrapper = new Wrapper().Setup_PassGarbageCollection();

		Mock<IWindow> window = new();

		using Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new[] { wrapper.LayoutEngine.Object, new Mock<ILayoutEngine>().Object }
			);

		wrapper.CoreNativeManager
			.Setup(c => c.ExecuteTask(It.IsAny<Func<DispatcherQueueHandler>>(), It.IsAny<CancellationToken>()))
			.Returns(() =>
			{
				TaskCompletionSource<object> tcs = new();
				return tcs.Task;
			});

		// When
		workspace.AddWindow(window.Object);
		wrapper.TriggerWorkspaceLayoutStarted.Invocations.Clear();
		wrapper.TriggerWorkspaceLayoutCompleted.Invocations.Clear();
		workspace.DoLayout();
		wrapper.Setup_ExecuteTask();
		workspace.DoLayout();

		// Then
		wrapper.NativeManager.Verify(n => n.ShowWindowNoActivate(It.IsAny<HWND>()), Times.Never);
		window.Verify(w => w.ShowMinimized(), Times.Never);
		wrapper.TriggerWorkspaceLayoutStarted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Exactly(2));
		wrapper.TriggerWorkspaceLayoutCompleted.Verify(e => e.Invoke(It.IsAny<WorkspaceEventArgs>()), Times.Once);
	}
	#endregion

	[Fact]
	public void ContainsWindow_False()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		bool result = workspace.ContainsWindow(window.Object);

		// Then
		Assert.False(result);
	}

	[Fact]
	public void ContainsWindow_True_NormalWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When
		bool result = workspace.ContainsWindow(window.Object);

		// Then
		Assert.True(result);
	}

	[Fact]
	public void ContainsWindow_True_MinimizedWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);

		// When
		bool result = workspace.ContainsWindow(window.Object);

		// Then
		Assert.True(result);
	}

	#region WindowFocused
	[Fact]
	public void WindowFocused_ContainsWindow()
	{
		// Given the window is in the workspace
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When
		workspace.WindowFocused(window.Object);

		// Then
		Assert.Equal(window.Object, workspace.LastFocusedWindow);
	}

	[Fact]
	public void WindowFocused_DoesNotContainWindow()
	{
		// Given the window is not in the workspace
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.WindowFocused(window.Object);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}

	[Fact]
	public void WindowFocused_WindowIsNull()
	{
		// Given the window is null
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.WindowFocused(window.Object);
		workspace.WindowFocused(null);

		// Then
		Assert.Null(workspace.LastFocusedWindow);
	}
	#endregion

	[Fact]
	public void FocusFirstWindow()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.LayoutEngine.Setup(l => l.GetFirstWindow()).Returns(new Mock<IWindow>().Object);
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When FocusFirstWindow is called
		workspace.FocusFirstWindow();

		// Then the LayoutEngine's GetFirstWindow method is called
		wrapper.LayoutEngine.Verify(l => l.GetFirstWindow(), Times.Once);
	}

	[Fact]
	public void NextLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object, layoutEngine.Object }
			);

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the next one
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
	}

	[Fact]
	public void NextLayoutEngine_LastEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object, layoutEngine.Object }
			);

		// When NextLayoutEngine is called
		workspace.NextLayoutEngine();
		workspace.NextLayoutEngine();

		// Then the active layout engine is set to the first one
		Assert.True(Object.ReferenceEquals(wrapper.LayoutEngine.Object, workspace.ActiveLayoutEngine));
	}

	[Fact]
	public void PreviousLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object, layoutEngine.Object }
			);

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the previous one
		Assert.True(Object.ReferenceEquals(layoutEngine.Object, workspace.ActiveLayoutEngine));
	}

	[Fact]
	public void PreviousLayoutEngine_FirstEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<ILayoutEngine> layoutEngine = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object, layoutEngine.Object }
			);

		// When PreviousLayoutEngine is called
		workspace.PreviousLayoutEngine();
		workspace.PreviousLayoutEngine();

		// Then the active layout engine is set to the last one
		Assert.True(Object.ReferenceEquals(wrapper.LayoutEngine.Object, workspace.ActiveLayoutEngine));
	}

	[Fact]
	public void AddWindow_Fails_AlreadyIncludesWindow()
	{
		// Given
		Wrapper wrapper = new Wrapper().Setup_PassGarbageCollection();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When AddWindow is called
		workspace.AddWindow(window.Object);
		workspace.AddWindow(window.Object);

		// Then the window is added to the layout engine
		wrapper.LayoutEngine.Verify(l => l.AddWindow(window.Object), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void AddWindow_Success()
	{
		// Given
		Wrapper wrapper = new Wrapper().Setup_PassGarbageCollection();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When AddWindow is called
		workspace.AddWindow(window.Object);

		// Then the window is added to the layout engine
		wrapper.LayoutEngine.Verify(l => l.AddWindow(window.Object), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void RemoveWindow_Fails_AlreadyRemoved()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When RemoveWindow is called
		workspace.RemoveWindow(window.Object);
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.False(result);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Never);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Fails_DidNotRemoveFromLayoutEngine()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		wrapper.WorkspaceManager.Invocations.Clear();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.False(result);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
	}

	[Fact]
	public void RemoveWindow_Success()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		workspace.WindowFocused(window.Object);
		wrapper.WorkspaceManager.Invocations.Clear();
		wrapper.LayoutEngine.Setup(l => l.RemoveWindow(window.Object)).Returns(new Mock<ILayoutEngine>().Object);

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is removed from the layout engine
		Assert.True(result);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Once);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void RemoveWindow_MinimizedWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		window.Setup(w => w.IsMinimized).Returns(true);
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);

		wrapper.WorkspaceManager.Invocations.Clear();
		wrapper.LayoutEngine.Invocations.Clear();

		// When RemoveWindow is called
		bool result = workspace.RemoveWindow(window.Object);

		// Then the window is not removed from the layout engine
		Assert.True(result);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Never);
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is not told to focus the window
		wrapper.LayoutEngine.Verify(l => l.FocusWindowInDirection(Direction.Up, window.Object), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_Fails_WindowIsMinimized()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is not told to focus the window
		wrapper.LayoutEngine.Verify(l => l.FocusWindowInDirection(Direction.Up, window.Object), Times.Never);
	}

	[Fact]
	public void FocusWindowInDirection_Success()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When FocusWindowInDirection is called
		workspace.FocusWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is told to focus the window
		wrapper.LayoutEngine.Verify(l => l.FocusWindowInDirection(Direction.Up, window.Object), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection_Fails_WindowIsNull()
	{
		// Given
		Wrapper wrapper = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, null);

		// Then the layout engine is not told to swap the window
		wrapper.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, It.IsAny<IWindow>()), Times.Never);
	}

	[Fact]
	public void SwapWindowInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is not told to swap the window
		wrapper.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, window.Object), Times.Never);
	}

	[Fact]
	public void SwapWindowInDirection_Success()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When SwapWindowInDirection is called
		workspace.SwapWindowInDirection(Direction.Up, window.Object);

		// Then the layout engine is told to swap the window
		wrapper.LayoutEngine.Verify(l => l.SwapWindowInDirection(Direction.Up, window.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_Fails_WindowIsNull()
	{
		// Given
		Wrapper wrapper = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, null);

		// Then the layout engine is not told to move the window
		wrapper.LayoutEngine.Verify(
			l => l.MoveWindowEdgesInDirection(Direction.Up, deltas, It.IsAny<IWindow>()),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_Fails_DoesNotContainWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window.Object);

		// Then the layout engine is not told to move the window
		wrapper.LayoutEngine.Verify(
			l => l.MoveWindowEdgesInDirection(Direction.Up, deltas, window.Object),
			Times.Never
		);
	}

	[Fact]
	public void MoveWindowEdgesInDirection_Success()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		IPoint<double> deltas = new Point<double>() { X = 0.3, Y = 0 };

		// When MoveWindowEdgesInDirection is called
		workspace.MoveWindowEdgesInDirection(Direction.Up, deltas, window.Object);

		// Then the layout engine is told to move the window
		wrapper.LayoutEngine.Verify(l => l.MoveWindowEdgesInDirection(Direction.Up, deltas, window.Object), Times.Once);
	}

	[Fact]
	public void MoveWindowToPoint_Success_AddWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		// Set up MoveWindowToPoint to return a new layout engine.
		Mock<ILayoutEngine> resultingEngine = new();
		resultingEngine.Setup(l => l.Name).Returns("Resulting engine");
		wrapper.LayoutEngine.Setup(l => l.MoveWindowToPoint(window.Object, point)).Returns(resultingEngine.Object);

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window.Object, point);

		// Then the layout engine is told to move the window
		wrapper.LayoutEngine.Verify(l => l.MoveWindowToPoint(window.Object, point), Times.Once);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_Success_WindowIsMinimized()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		// Set up MoveWindowToPoint to return a new layout engine.
		Mock<ILayoutEngine> resultingEngine = new();
		resultingEngine.Setup(l => l.Name).Returns("Resulting engine");
		wrapper.LayoutEngine.Setup(l => l.MoveWindowToPoint(window.Object, point)).Returns(resultingEngine.Object);

		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);

		wrapper.LayoutEngine.Invocations.Clear();

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window.Object, point);

		// Then the layout engine is told to move the window
		wrapper.LayoutEngine.Verify(l => l.MoveWindowToPoint(window.Object, point), Times.Once);
		wrapper.LayoutEngine.Verify(l => l.RemoveWindow(window.Object), Times.Never);
	}

	[Fact]
	public void MoveWindowToPoint_Success_WindowAlreadyExists()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		workspace.AddWindow(window.Object);
		IPoint<double> point = new Point<double>() { X = 0.3, Y = 0.3 };

		wrapper.LayoutEngine.Reset();

		// Set up MoveWindowToPoint to return a new layout engine.
		Mock<ILayoutEngine> moveWindowToPointResult = new();
		moveWindowToPointResult.Setup(l => l.Name).Returns("Move window to result");

		wrapper.LayoutEngine
			.Setup(l => l.MoveWindowToPoint(It.IsAny<IWindow>(), point))
			.Returns(moveWindowToPointResult.Object);

		// When MoveWindowToPoint is called
		workspace.MoveWindowToPoint(window.Object, point);

		// Then the layout engine is told to remove and add the window
		wrapper.LayoutEngine.Verify(l => l.MoveWindowToPoint(window.Object, point), Times.Once);
	}

	[Fact]
	public void ToString_Success()
	{
		// Given
		Wrapper wrapper = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When ToString is called
		string result = workspace.ToString();

		// Then the result is as expected
		Assert.Equal("Workspace", result);
	}

	[Fact]
	public void Deactivate()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Mock<IWindow> window2 = new();

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		workspace.AddWindow(window2.Object);
		wrapper.WorkspaceManager.Invocations.Clear();

		// When Deactivate is called
		workspace.Deactivate();

		// Then the windows are hidden and DoLayout is called
		wrapper.WorkspaceManager.Verify(wm => wm.GetMonitorForWorkspace(workspace), Times.Never);
		window.Verify(w => w.Hide(), Times.Once);
		window2.Verify(w => w.Hide(), Times.Once);
	}

	[Fact]
	public void TryGetWindowLocation()
	{
		// Given
		Wrapper wrapper = new Wrapper().Setup_PassGarbageCollection();

		Mock<IWindow> window = new();

		wrapper.LayoutEngine
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

		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When TryGetWindowLocation is called
		IWindowState? result = workspace.TryGetWindowLocation(window.Object);

		// Then the result is as expected
		Assert.NotNull(result);
	}

	[Fact]
	public void TryGetWindowLocation_MinimizedWindow()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);

		// When
		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);
		IWindowState windowState = workspace.TryGetWindowLocation(window.Object)!;

		// Then
		Assert.Equal(window.Object, windowState.Window);
		Assert.Equal(0, windowState.Location.X);
		Assert.Equal(0, windowState.Location.Y);
		Assert.Equal(0, windowState.Location.Width);
		Assert.Equal(0, windowState.Location.Height);
		Assert.Equal(WindowSize.Minimized, windowState.WindowSize);
	}

	[Fact]
	public void Dispose()
	{
		// Given
		Wrapper wrapper = new();
		wrapper.WorkspaceManager
			.Setup(wm => wm.GetMonitorForWorkspace(It.IsAny<IWorkspace>()))
			.Returns((IMonitor?)null);
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When Dispose is called
		workspace.Dispose();

		// Then the window is minimized
		window.Verify(w => w.ShowMinimized(), Times.Once);
	}

	[Fact]
	public void WindowMinimizeStart()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When WindowMinimizeStart is called
		workspace.WindowMinimizeStart(window.Object);

		// Then
		wrapper.LayoutEngine.Verify(e => e.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void WindowMinimizeStart_Twice()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);

		// When WindowMinimizeStart is called
		workspace.WindowMinimizeStart(window.Object);
		workspace.WindowMinimizeStart(window.Object);

		// Then the window is only removed the first time
		wrapper.LayoutEngine.Verify(e => e.RemoveWindow(window.Object), Times.Once);
	}

	[Fact]
	public void WindowMinimizeEnd()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		workspace.WindowMinimizeStart(window.Object);
		wrapper.LayoutEngine.Invocations.Clear();

		// When WindowMinimizeEnd is called
		workspace.WindowMinimizeEnd(window.Object);

		// Then
		wrapper.LayoutEngine.Verify(e => e.AddWindow(window.Object), Times.Once);
	}

	[Fact]
	public void WindowMinimizeEnd_NotMinimized()
	{
		// Given
		Wrapper wrapper = new();
		Mock<IWindow> window = new();
		Workspace workspace =
			new(
				wrapper.Context.Object,
				wrapper.InternalContext.Object,
				wrapper.Triggers,
				"Workspace",
				new ILayoutEngine[] { wrapper.LayoutEngine.Object }
			);
		workspace.AddWindow(window.Object);
		wrapper.LayoutEngine.Invocations.Clear();

		// When WindowMinimizeEnd is called
		workspace.WindowMinimizeEnd(window.Object);

		// Then
		wrapper.LayoutEngine.Verify(e => e.AddWindow(window.Object), Times.Never);
	}
}
