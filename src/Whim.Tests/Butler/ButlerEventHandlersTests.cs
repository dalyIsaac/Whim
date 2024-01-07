using System;
using System.Collections.Generic;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

internal record ButlerTriggersCalls()
{
	public List<MonitorWorkspaceChangedEventArgs> MonitorWorkspaceChanged { get; } = new();
	public List<RouteEventArgs> WindowRouted { get; } = new();
}

internal class ButlerEventHandlersCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		ButlerTriggersCalls calls = fixture.Create<ButlerTriggersCalls>();
		fixture.Register(
			() =>
				new ButlerTriggers()
				{
					MonitorWorkspaceChanged = (args) => calls.MonitorWorkspaceChanged.Add(args),
					WindowRouted = (args) => calls.WindowRouted.Add(args)
				}
		);
	}
}

public class ButlerEventHandlersTests
{
	// #region WindowAdded
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_NoRouter(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	ButlerTriggersCalls triggersCalls,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// When a window is added
	// 	handlers.PreInitialize();
	// 	ctx.WindowManager.WindowAdded += Raise.EventWith(new WindowEventArgs() { Window = Substitute.For<IWindow>() });

	// 	// Then the window is added to the active workspace
	// 	Assert.Single(triggersCalls.WindowRouted);
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_RouterReturnsInvalidWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// There is a router which routes the window to a different workspace
	// 	ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the active workspace
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_NoRouter_GetMonitorAtWindowCenter(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores,
	// 	IWorkspace imposterWorkspace
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	ctx.RouterManager.RouteWindow(window).Returns(imposterWorkspace);
	// 	ctx.MonitorManager.GetMonitorAtPoint(point: Arg.Any<IPoint<int>>()).Returns(monitors[0]);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the active workspace
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// 	imposterWorkspace.DidNotReceive().AddWindow(window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_Router(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// There is a router which routes the window to a different workspace
	// 	ctx.RouterManager.RouteWindow(window).Returns(workspaces[1]);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the workspace returned by the router
	// 	workspaces[0].DidNotReceive().AddWindow(window);
	// 	workspaces[1].Received(1).AddWindow(window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_RouteToActiveWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.Activate(workspaces[0], monitors[0]);

	// 	// There is a router which routes the window to the active workspace
	// 	ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);
	// 	ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToActiveWorkspace);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the active workspace
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_RouteToLastTrackedActiveWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.Activate(workspaces[0], monitors[0]);

	// 	// There is a router which routes the window to the last tracked active workspace
	// 	ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);
	// 	ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLastTrackedActiveWorkspace);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the active workspace
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_RouteToLaunchedWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.Activate(workspaces[0], monitors[0]);

	// 	// There is a router which routes the window to the last tracked active workspace
	// 	ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);
	// 	ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLaunchedWorkspace);

	// 	// When a window is added
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
	// 		h => workspaceManager.WindowRouted += h,
	// 		h => workspaceManager.WindowRouted -= h,
	// 		() => workspaceManager.WindowAdded(window)
	// 	);

	// 	// Then the window is added to the active workspace
	// 	Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	// }

	// private static void Assert_WindowAddedToWorkspace1(
	// 	IWorkspace[] workspaces,
	// 	IWindow window,
	// 	Assert.RaisedEvent<RouteEventArgs> routeEvent
	// )
	// {
	// 	workspaces[0].Received(1).AddWindow(window);
	// 	workspaces[1].DidNotReceive().AddWindow(window);

	// 	Assert.Equal(workspaces[0], routeEvent.Arguments.CurrentWorkspace);
	// 	Assert.Equal(window, routeEvent.Arguments.Window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowAdded_WindowIsMinimized(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	window.IsMinimized.Returns(true);

	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.Activate(workspaces[0], monitors[0]);

	// 	// When a window is added
	// 	workspaceManager.WindowAdded(window);

	// 	// Then the window is not added to the workspace, and MinimizeWindowStart is called
	// 	workspaces[0].DidNotReceive().AddWindow(window);
	// 	workspaces[1].DidNotReceive().AddWindow(window);
	// 	workspaces[0].Received(1).MinimizeWindowStart(window);
	// }
	// #endregion

	// #region WindowRemoved
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowRemoved_NotFound(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	ButlerTriggers triggers,
	// 	IButlerPantry pantry,
	// 	IButlerChores chores
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// When a window is removed
	// 	workspaceManager.WindowRemoved(window);

	// 	// Then the window is removed from all workspaces
	// 	workspaces[0].DidNotReceive().RemoveWindow(window);
	// 	workspaces[1].DidNotReceive().RemoveWindow(window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowRemoved_Found(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors, IWindow window)
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	workspaceManager.WindowAdded(window);

	// 	// When a window which is in a workspace is removed
	// 	workspaceManager.WindowRemoved(window);

	// 	// Then the window is removed from the workspace
	// 	workspaces[0].Received(1).RemoveWindow(window);
	// 	workspaces[1].DidNotReceive().RemoveWindow(window);
	// }
	// #endregion

	// #region WindowFocused
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowFocused_WindowIsNull(IContext ctx, IInternalContext internalCtx)
	// {
	// 	// Given the window is null
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	IWindow? window = null;

	// 	// When WindowFocused is called
	// 	workspaceManager.WindowFocused(window);

	// 	// Then each workspace is notified, but no workspace is laid out
	// 	WindowFocused_Notification_NoLayout(workspaces, window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowFocused_WindowNotInWorkspace(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given the window is defined, but not in any workspace
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// When WindowFocused is called
	// 	workspaceManager.WindowFocused(window);

	// 	// Then each workspace is notified, but no workspace is laid out
	// 	WindowFocused_Notification_NoLayout(workspaces, window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowFocused_WindowWorkspaceNotShown(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given the window is defined, and in a workspace, but the workspace is not shown
	// 	ctx.MonitorManager.GetEnumerator().Returns(Array.Empty<IMonitor>().AsEnumerable().GetEnumerator());
	// 	IInternalWorkspace[] internalWorkspaces = workspaces.Cast<IInternalWorkspace>().ToArray();
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.WindowAdded(window);

	// 	// When WindowFocused is called
	// 	workspaceManager.WindowFocused(window);

	// 	// Then each workspace is notified, and the workspace is laid out
	// 	internalWorkspaces[0].Received(1).WindowFocused(window);
	// 	internalWorkspaces[1].Received(1).WindowFocused(window);

	// 	workspaces[0].Received(1).DoLayout();
	// 	workspaces[1].DidNotReceive().DoLayout();
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowFocused_WindowWorkspaceIsShown(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given the window is defined, in a workspace, and the workspace is shown
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);
	// 	workspaceManager.WindowAdded(window);
	// 	workspaceManager.WindowFocused(window);

	// 	// When WindowFocused is called
	// 	workspaceManager.WindowFocused(window);

	// 	// Then each workspace is notified, but no workspace is laid out
	// 	WindowFocused_Notification_NoLayout(workspaces, window);
	// }

	// private static void WindowFocused_Notification_NoLayout(IWorkspace[] workspaces, IWindow? window)
	// {
	// 	IInternalWorkspace[] internalWorkspaces = workspaces.Cast<IInternalWorkspace>().ToArray();
	// 	internalWorkspaces[0].Received(1).WindowFocused(window);
	// 	internalWorkspaces[1].Received(1).WindowFocused(window);

	// 	workspaces[0].DidNotReceive().DoLayout();
	// 	workspaces[1].DidNotReceive().DoLayout();
	// }
	// #endregion

	// #region WindowMinimizeStart
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowMinimizeStart_CouldNotFindWindow(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	IWorkspace workspace,
	// 	IWindow window
	// )
	// {
	// 	// Given
	// 	WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });

	// 	// When a window is minimized, but the window is not found in any workspace
	// 	workspaceManager.WindowMinimizeStart(window);

	// 	// Then nothing happens
	// 	workspace.DidNotReceive().RemoveWindow(window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowMinimizeStart(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given
	// 	IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
	// 	WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
	// 	ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

	// 	// A window is added to the workspace
	// 	workspaceManager.WindowAdded(window);

	// 	// When a window is minimized
	// 	workspaceManager.WindowMinimizeStart(window);

	// 	// Then the workspace is notified
	// 	workspace.Received(1).MinimizeWindowStart(window);
	// }
	// #endregion

	// #region WindowMinimizeEnd
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowMinimizeEnd(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given
	// 	IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
	// 	WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
	// 	ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

	// 	// A window is added to the workspace
	// 	workspaceManager.WindowAdded(window);

	// 	// When a window is restored
	// 	workspaceManager.WindowMinimizeEnd(window);

	// 	// Then the window is routed to the workspace
	// 	workspace.Received(1).MinimizeWindowEnd(window);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void WindowMinizeEnd_Fail(IContext ctx, IInternalContext internalCtx, IWindow window)
	// {
	// 	// Given
	// 	IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
	// 	WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
	// 	ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

	// 	// When a window which isn't tracked is restored
	// 	workspaceManager.WindowMinimizeEnd(window);

	// 	// Then the workspace is not notified
	// 	workspace.DidNotReceive().MinimizeWindowEnd(window);
	// }
	// #endregion

	// #region MonitorManager_MonitorsChanged
	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void MonitorManager_MonitorsChanged_Removed(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	IMonitor[] monitors
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	// When a monitor is removed, and a monitor not tracked in the WorkspaceManager is removed
	// 	workspaceManager.MonitorManager_MonitorsChanged(
	// 		this,
	// 		new MonitorsChangedEventArgs()
	// 		{
	// 			AddedMonitors = Array.Empty<IMonitor>(),
	// 			RemovedMonitors = new IMonitor[] { monitors[0], Substitute.For<IMonitor>() },
	// 			UnchangedMonitors = new IMonitor[] { monitors[1] }
	// 		}
	// 	);

	// 	// Then the workspace is deactivated
	// 	workspaces[0].Received(1).Deactivate();
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void MonitorManager_MonitorsChanged_Added_CreateWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	IMonitor[] monitors
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	int calls = 0;
	// 	CreateLeafLayoutEngine[] createLayoutEngines()
	// 	{
	// 		calls++;
	// 		return new CreateLeafLayoutEngine[] { (identity) => Substitute.For<ILayoutEngine>() };
	// 	}
	// 	workspaceManager.CreateLayoutEngines = createLayoutEngines;

	// 	// When a monitor is added
	// 	workspaceManager.MonitorManager_MonitorsChanged(
	// 		this,
	// 		new MonitorsChangedEventArgs()
	// 		{
	// 			AddedMonitors = new IMonitor[] { Substitute.For<IMonitor>() },
	// 			RemovedMonitors = Array.Empty<IMonitor>(),
	// 			UnchangedMonitors = new IMonitor[] { monitors[0], monitors[1] }
	// 		}
	// 	);

	// 	// Then a new workspace is created
	// 	Assert.Equal(1, calls);
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void MonitorManager_MonitorsChanged_Added_UseExistingWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	IMonitor[] monitors
	// )
	// {
	// 	// Given
	// 	ButlerEventHandlers handlers = new(ctx, internalCtx, triggers, pantry, chores);

	// 	workspaceManager.Activate(workspaces[0], monitors[0]);
	// 	workspaceManager.Activate(workspaces[1], monitors[1]);
	// 	workspaces[2].ClearReceivedCalls();

	// 	// When a monitor is added
	// 	workspaceManager.MonitorManager_MonitorsChanged(
	// 		this,
	// 		new MonitorsChangedEventArgs()
	// 		{
	// 			AddedMonitors = new IMonitor[] { Substitute.For<IMonitor>() },
	// 			RemovedMonitors = Array.Empty<IMonitor>(),
	// 			UnchangedMonitors = new IMonitor[] { monitors[0], monitors[1] }
	// 		}
	// 	);

	// 	// Then the workspace is activated
	// 	workspaces[0].Received(1).DoLayout();
	// 	workspaces[1].Received(1).DoLayout();
	// 	workspaces[2].Received(2).DoLayout();
	// }

	// [Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	// internal void MonitorManager_MonitorsChanged_CannotAddWorkspace(
	// 	IContext ctx,
	// 	IInternalContext internalCtx,
	// 	IMonitor[] monitors,
	// 	IWorkspace workspace
	// )
	// {
	// 	// Given
	// 	WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
	// 	workspaceManager.Activate(workspace, monitors[0]);

	// 	// Reset the wrapper
	// 	workspace.ClearReceivedCalls();
	// 	workspaceManager.CreateLayoutEngines = Array.Empty<CreateLeafLayoutEngine>;

	// 	// When a monitor is added
	// 	workspaceManager.MonitorManager_MonitorsChanged(
	// 		this,
	// 		new MonitorsChangedEventArgs()
	// 		{
	// 			AddedMonitors = new IMonitor[] { monitors[0], monitors[1] },
	// 			RemovedMonitors = Array.Empty<IMonitor>(),
	// 			UnchangedMonitors = new IMonitor[] { monitors[0] }
	// 		}
	// 	);

	// 	// Then the workspace is not activated
	// 	Assert.Single(workspaceManager);
	// }
	// #endregion
}
