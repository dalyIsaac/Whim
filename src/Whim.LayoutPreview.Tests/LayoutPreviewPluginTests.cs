using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using AutoFixture;
using NSubstitute;
using Whim.FloatingLayout;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.LayoutPreview.Tests;

public class LayoutPreviewPluginCustomization : StoreCustomization
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	protected override void PostCustomize(IFixture fixture)
	{
		Workspace workspace = StoreTestUtils.CreateWorkspace(_ctx);
		fixture.Inject(workspace);

		IMonitor monitor = StoreTestUtils.CreateMonitor((HMONITOR)1);

		StoreTestUtils.SetupMonitorAtPoint(
			_ctx,
			_internalCtx,
			_store._root.MutableRootSector,
			new Point<int>(0, 0),
			monitor
		);
		StoreTestUtils.PopulateMonitorWorkspaceMap(_ctx, _store._root.MutableRootSector, monitor, workspace);
	}
}

public class LayoutPreviewPluginTests
{
	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void Name(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		string name = plugin.Name;

		// Then
		Assert.Equal("whim.layout_preview", name);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void PluginCommands(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		IEnumerable<ICommand> commands = plugin.PluginCommands.Commands;

		// Then
		Assert.Empty(commands);
	}

	[Theory, AutoSubstituteData]
	[SuppressMessage("Usage", "NS5000:Received check.")]
	internal void PreInitialize(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.PreInitialize();

		// Then
		ctx.Store.WindowEvents.Received(1).WindowMoveStarted += Arg.Any<EventHandler<WindowMoveStartedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoved += Arg.Any<EventHandler<WindowMovedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowMoveEnded += Arg.Any<EventHandler<WindowMoveEndedEventArgs>>();
		ctx.Store.WindowEvents.Received(1).WindowRemoved += Arg.Any<EventHandler<WindowRemovedEventArgs>>();
		ctx.FilterManager.Received(1).AddTitleMatchFilter(LayoutPreviewWindow.WindowTitle);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void PostInitialize(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.PostInitialize();

		// Then
		Assert.True(true);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void LoadState(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		plugin.LoadState(default);

		// Then
		Assert.True(true);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void SaveState(IContext ctx)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		// When
		JsonElement? state = plugin.SaveState();

		// Then
		Assert.Null(state);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoveStarted_NotDragged(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMoveStartedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	#region WindowMoved
	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoved_NotDragged(IContext ctx, MutableRootSector rootSector)
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
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		ctx.MonitorManager.DidNotReceive().GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoved_MovingEdges(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = Direction.LeftDown
			};

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		ctx.MonitorManager.DidNotReceive().GetMonitorAtPoint(Arg.Any<IPoint<int>>());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoved_Dragged_CannotFindWorkspace(
		IContext ctx,
		MutableRootSector rootSector,
		IWorkspace workspace
	)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			};
		rootSector.MapSector.MonitorWorkspaceMap = rootSector.MapSector.MonitorWorkspaceMap.Clear();

		workspace.ActiveLayoutEngine.ClearReceivedCalls();

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		Assert.Empty(workspace.ActiveLayoutEngine.ReceivedCalls());
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoved_Dragged_IgnoreFloatingLayoutEngine(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		// Given
		workspace = workspace with
		{
			LayoutEngines = ImmutableList<ILayoutEngine>.Empty.Add(
				new FreeLayoutEngine(ctx, new LayoutEngineIdentity())
			),
			ActiveLayoutEngineIndex = 0
		};

		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = window,
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		Assert.Single(workspace.ActiveLayoutEngine.ReceivedCalls());
		Assert.Equal(window, plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoved_Dragged_Success(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window,
		Workspace workspace
	)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMovedEventArgs e =
			new()
			{
				Window = window,
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			};

		workspace.ActiveLayoutEngine.ClearReceivedCalls();

		rootSector.WindowSector.QueueEvent(e);

		// When
		plugin.PreInitialize();
		rootSector.DispatchEvents();

		// Then
		Assert.Single(workspace.ActiveLayoutEngine.ReceivedCalls());
		Assert.Equal(window, plugin.DraggedWindow);
	}
	#endregion

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowMoveEnded(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);
		WindowMoveEndedEventArgs e =
			new()
			{
				Window = Substitute.For<IWindow>(),
				CursorDraggedPoint = null,
				MovedEdges = null
			};

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(e);
		rootSector.DispatchEvents();

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowEvents_WindowRemoved_NotHidden(
		IContext ctx,
		MutableRootSector rootSector,
		IWindow movedWindow,
		IWindow removedWindow
	)
	{
		// Given WindowMoved and WindowRemoved events are queued
		using LayoutPreviewPlugin plugin = new(ctx);
		rootSector.WindowSector.QueueEvent(
			new WindowMovedEventArgs()
			{
				Window = movedWindow,
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			}
		);
		rootSector.WindowSector.QueueEvent(new WindowRemovedEventArgs() { Window = removedWindow });

		// When the events are dispatched
		plugin.PreInitialize();
		rootSector.DispatchEvents();

		// Then
		Assert.Equal(movedWindow, plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowEvents_WindowRemoved_Hidden(IContext ctx, MutableRootSector rootSector, IWindow movedWindow)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow,
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			};
		WindowEventArgs removeArgs = new WindowRemovedEventArgs() { Window = movedWindow };

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(moveArgs);
		rootSector.WindowSector.QueueEvent(removeArgs);
		rootSector.DispatchEvents();

		// Then
		Assert.Null(plugin.DraggedWindow);
	}

	[Theory, AutoSubstituteData<LayoutPreviewPluginCustomization>]
	internal void WindowEvents_WindowFocused(IContext ctx, MutableRootSector rootSector, IWindow movedWindow)
	{
		// Given
		using LayoutPreviewPlugin plugin = new(ctx);

		WindowMovedEventArgs moveArgs =
			new()
			{
				Window = movedWindow,
				CursorDraggedPoint = new Rectangle<int>(),
				MovedEdges = null
			};

		WindowFocusedEventArgs focusArgs = new() { Window = movedWindow };

		// When
		plugin.PreInitialize();
		rootSector.WindowSector.QueueEvent(moveArgs);
		rootSector.WindowSector.QueueEvent(focusArgs);
		rootSector.DispatchEvents();

		// Then
		Assert.Null(plugin.DraggedWindow);
	}
}
