using AutoFixture;
using NSubstitute;
using System.Text.Json;
using Whim.TestUtils;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewPluginCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IContext ctx = fixture.Freeze<IContext>();

		IWorkspace workspace = fixture.Freeze<IWorkspace>();

		IMonitor monitor = fixture.Freeze<IMonitor>();
		monitor.WorkingArea.Returns(
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			}
		);

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitor);
		ctx.WorkspaceManager.GetWorkspaceForMonitor(Arg.Any<IMonitor>()).Returns(workspace);
	}
}

public class LayoutPreviewPluginTests
{
	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void Name(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.layout_preview", name);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void PluginCommands(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		IEnumerable<ICommand> commands = plugin.PluginCommands.Commands;

		// Then
		Assert.Empty(commands);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void PreInitialize(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then
		ctx.WindowManager.Received(1).WindowMoveStart += Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.WindowManager.Received(1).WindowMoved += Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.WindowManager.Received(1).WindowMoveEnd += Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.FilterManager.Received(1).IgnoreTitleMatch(LayoutPreviewWindow.WindowTitle);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void PostInitialize(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then
		Assert.True(true);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void LoadState(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.LoadState(default);

		// Then
		Assert.True(true);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void SaveState(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoveStart_NotDragged(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoveStart += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	#region WindowMoved
	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoved_NotDragged(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		ctx.MonitorManager.DidNotReceive().GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoved_MovingEdges(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = Direction.LeftDown
			};

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		ctx.MonitorManager.DidNotReceive().GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoved_Dragged_CannotFindWorkspace(IContext ctx, IWorkspace workspace)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};
		ctx.WorkspaceManager.GetWorkspaceForMonitor(Arg.Any<IMonitor>()).Returns((IWorkspace?)null);

		workspace.ActiveLayoutEngine.ClearReceivedCalls();

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		ctx.MonitorManager.Received(1).GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		ctx.WorkspaceManager.Received(1).GetWorkspaceForMonitor(Arg.Any<IMonitor>());
		Assert.Empty(workspace.ActiveLayoutEngine.ReceivedCalls());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoved_Dragged_Success(IContext ctx, IWindow window, IWorkspace workspace)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = window,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		workspace.ActiveLayoutEngine.ClearReceivedCalls();

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		ctx.MonitorManager.Received(1).GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		ctx.WorkspaceManager.Received(1).GetWorkspaceForMonitor(Arg.Any<IMonitor>());
		Assert.Single(workspace.ActiveLayoutEngine.ReceivedCalls());
		Assert.Equal(window, plugin.DraggedWindow);
	}
	#endregion

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowMoveEnd(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoveEnd += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, e);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowManager_WindowRemoved_NotHidden(IContext ctx, IWindow movedWindow, IWindow removedWindow)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		WindowEventArgs removeArgs = new() { Window = removedWindow };

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, moveArgs);
		ctx.WindowManager.WindowRemoved += Raise.Event<EventHandler<WindowEventArgs>>(ctx.WindowManager, removeArgs);

		// Then
		Assert.Equal(movedWindow, plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	public void WindowManager_WindowRemoved_Shown(IContext ctx, IWindow movedWindow)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow,
				CursorDraggedPoint = new Location<int>(),
				MovedEdges = null
			};

		WindowEventArgs removeArgs = new() { Window = movedWindow };

		// When
		plugin.PreInitialize();
		ctx.WindowManager.WindowMoved += Raise.Event<EventHandler<WindowMovedEventArgs>>(ctx.WindowManager, moveArgs);
		ctx.WindowManager.WindowRemoved += Raise.Event<EventHandler<WindowEventArgs>>(ctx.WindowManager, removeArgs);

		// Then
		Assert.Null(plugin.DraggedWindow);
	}
}
