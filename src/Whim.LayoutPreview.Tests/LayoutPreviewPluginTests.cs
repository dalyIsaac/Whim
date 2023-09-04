using Moq;
using System.Text.Json;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewPluginTests
{
	private class Wrapper
	{
		public Mock<IWindowManager> WindowManager { get; } = new();
		public Mock<IFilterManager> FilterManager { get; } = new();
		public Mock<IMonitorManager> MonitorManager { get; } = new();
		public Mock<IMonitor> Monitor { get; } = new();
		public Mock<IWorkspaceManager> WorkspaceManager { get; } = new();
		public Mock<IWorkspace> Workspace { get; } = new();
		public Mock<ILayoutEngine> LayoutEngine { get; } = new();
		public Mock<IContext> Context { get; } = new();

		public Wrapper()
		{
			Context.Setup(x => x.WindowManager).Returns(WindowManager.Object);
			Context.Setup(x => x.FilterManager).Returns(FilterManager.Object);
			Context.Setup(x => x.MonitorManager).Returns(MonitorManager.Object);
			Context.Setup(x => x.WorkspaceManager).Returns(WorkspaceManager.Object);

			MonitorManager.Setup(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>())).Returns(Monitor.Object);

			Monitor.Setup(x => x.WorkingArea).Returns(new Location<int>());

			WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(Monitor.Object)).Returns(Workspace.Object);

			Workspace.Setup(x => x.ActiveLayoutEngine).Returns(LayoutEngine.Object);
		}
	}

	[Fact]
	public void Name()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.layout_preview", name);
	}

	[Fact]
	public void PreInitialize()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.PreInitialize();

		// Then
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMoveStart += It.IsAny<EventHandler<WindowMovedEventArgs>>(),
			Times.Once
		);
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMoved += It.IsAny<EventHandler<WindowMovedEventArgs>>(),
			Times.Once
		);
		wrapper.WindowManager.VerifyAdd(
			x => x.WindowMoveEnd += It.IsAny<EventHandler<WindowMovedEventArgs>>(),
			Times.Once
		);
		wrapper.FilterManager.Verify(x => x.IgnoreTitleMatch(LayoutPreviewWindow.WindowTitle), Times.Once);
	}

	[Fact]
	public void PostInitialize()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.PostInitialize();

		// Then
		Assert.True(true);
	}

	[Fact]
	public void LoadState()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		// When
		plugin.LoadState(default);

		// Then
		Assert.True(true);
	}

	[Fact]
	public void SaveState()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	[Fact]
	public void WindowMoveStart_NotDragged()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		WindowMovedEventArgs e =
			new()
			{
				Window = new Mock<IWindow>().Object,
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoveStart += null, e);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	#region WindowMoved
	[Fact]
	public void WindowMoved_NotDragged()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		WindowMovedEventArgs e =
			new()
			{
				Window = new Mock<IWindow>().Object,
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, e);

		// Then
		wrapper.MonitorManager.Verify(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>()), Times.Never);
		Assert.Null(plugin.DraggedWindow);
	}

	[Fact]
	public void WindowMoved_MovingEdges()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		WindowMovedEventArgs e =
			new()
			{
				Window = new Mock<IWindow>().Object,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = Direction.LeftDown
			};

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, e);

		// Then
		wrapper.MonitorManager.Verify(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>()), Times.Never);
		Assert.Null(plugin.DraggedWindow);
	}

	[Fact]
	public void WindowMoved_Dragged_CannotFindWorkspace()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		WindowMovedEventArgs e =
			new()
			{
				Window = new Mock<IWindow>().Object,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};
		wrapper.WorkspaceManager.Setup(x => x.GetWorkspaceForMonitor(It.IsAny<IMonitor>())).Returns((IWorkspace?)null);

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, e);

		// Then
		wrapper.MonitorManager.Verify(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>()), Times.Once);
		wrapper.WorkspaceManager.Verify(x => x.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.Workspace.Verify(x => x.ActiveLayoutEngine, Times.Never);
		Assert.Null(plugin.DraggedWindow);
	}

	[Fact]
	public void WindowMoved_Dragged_Success()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		Mock<IWindow> window = new();
		WindowMovedEventArgs e =
			new()
			{
				Window = window.Object,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, e);

		// Then
		wrapper.MonitorManager.Verify(x => x.GetMonitorAtPoint(It.IsAny<IPoint<int>>()), Times.Once);
		wrapper.WorkspaceManager.Verify(x => x.GetWorkspaceForMonitor(It.IsAny<IMonitor>()), Times.Once);
		wrapper.Workspace.Verify(x => x.ActiveLayoutEngine, Times.Once);
		Assert.Equal(window.Object, plugin.DraggedWindow);
	}
	#endregion

	[Fact]
	public void WindowMoveEnd()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);
		WindowMovedEventArgs e =
			new()
			{
				Window = new Mock<IWindow>().Object,
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoveEnd += null, e);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	[Fact]
	public void WindowManager_WindowRemoved_NotHidden()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		Mock<IWindow> movedWindow = new();
		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow.Object,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		Mock<IWindow> removedWindow = new();
		WindowEventArgs removeArgs = new() { Window = removedWindow.Object };

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, moveArgs);
		wrapper.WindowManager.Raise(x => x.WindowRemoved += null, removeArgs);

		// Then
		Assert.Equal(movedWindow.Object, plugin.DraggedWindow);
	}

	[Fact]
	public void WindowManager_WindowRemoved_Shown()
	{
		// Given
		Wrapper wrapper = new();
		using LayoutPreviewPlugin plugin = new(wrapper.Context.Object);

		Mock<IWindow> movedWindow = new();
		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow.Object,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		WindowEventArgs removeArgs = new() { Window = movedWindow.Object };

		// When
		plugin.PreInitialize();
		wrapper.WindowManager.Raise(x => x.WindowMoved += null, moveArgs);
		wrapper.WindowManager.Raise(x => x.WindowRemoved += null, removeArgs);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}
}
