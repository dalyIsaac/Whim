using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Graphics.Gdi;
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
		fixture.Inject(calls);

		fixture.Inject(
			new ButlerTriggers()
			{
				MonitorWorkspaceChanged = calls.MonitorWorkspaceChanged.Add,
				WindowRouted = calls.WindowRouted.Add
			}
		);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Test code")]
public class ButlerEventHandlersTests
{
	private static void AssertWindowAdded(IWindow window, IWorkspace currentWorkspace, RouteEventArgs actual)
	{
		Assert.Equal(window, actual.Window);
		Assert.Null(actual.PreviousWorkspace);
		Assert.Equal(currentWorkspace, actual.CurrentWorkspace);

		currentWorkspace.Received().AddWindow(window);
		currentWorkspace.Received().DoLayout();

		window.Received().Focus();
	}

	private static void AssertWindowMinimized(IWindow window, IWorkspace currentWorkspace, RouteEventArgs actual)
	{
		Assert.Equal(window, actual.Window);
		Assert.Null(actual.PreviousWorkspace);
		Assert.Equal(currentWorkspace, actual.CurrentWorkspace);

		currentWorkspace.Received().MinimizeWindowStart(window);
		currentWorkspace.Received().DoLayout();

		window.Received().Focus();
	}

	#region WindowAdded
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteWindow(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace routedWorkspace
	)
	{
		// Given a window is routed to a workspace and the workspace manager contains the workspace
		ctx.RouterManager.RouteWindow(window).Returns(routedWorkspace);
		ctx.WorkspaceManager.Contains(routedWorkspace).Returns(true);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the workspace
		ctx.RouterManager.Received().RouteWindow(window);
		pantry.Received().SetWindowWorkspace(window, routedWorkspace);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, routedWorkspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteWindow_WorkspaceDoesNotExist(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace badWorkspace,
		IWorkspace goodWorkspace
	)
	{
		// Given a window is routed to a workspace and the workspace manager does not contain the workspace
		ctx.RouterManager.RouteWindow(window).Returns(badWorkspace);
		ctx.WorkspaceManager.Contains(badWorkspace).Returns(false);
		pantry.GetWorkspaceForMonitor(Arg.Any<IMonitor>()).Returns(goodWorkspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the workspace
		ctx.RouterManager.Received().RouteWindow(window);
		pantry.Received().SetWindowWorkspace(window, goodWorkspace);
		badWorkspace.DidNotReceive().AddWindow(window);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, goodWorkspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteToActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace activeWorkspace
	)
	{
		// Given the router options are set to RouteToActiveWorkspace and the active workspace is in the workspace manager
		ctx.RouterManager.RouteWindow(window).ReturnsNull();
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToActiveWorkspace);
		ctx.WorkspaceManager.ActiveWorkspace.Returns(activeWorkspace);
		ctx.WorkspaceManager.Contains(activeWorkspace).Returns(true);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the active workspace
		pantry.Received().SetWindowWorkspace(window, activeWorkspace);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, activeWorkspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteToLastTrackedActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace lastTrackedActiveWorkspace
	)
	{
		// Given the router options are set to RouteToLastTrackedActiveWorkspace and the last tracked active workspace is in the workspace manager
		ctx.RouterManager.RouteWindow(window).ReturnsNull();
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLastTrackedActiveWorkspace);
		internalCtx.MonitorManager.LastWhimActiveMonitor.Returns(Substitute.For<IMonitor>());
		pantry
			.GetWorkspaceForMonitor(internalCtx.MonitorManager.LastWhimActiveMonitor)
			.Returns(lastTrackedActiveWorkspace);
		ctx.WorkspaceManager.Contains(lastTrackedActiveWorkspace).Returns(true);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the last tracked active workspace
		pantry.Received().SetWindowWorkspace(window, lastTrackedActiveWorkspace);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, lastTrackedActiveWorkspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteToLaunchedWorkspace_MonitorFromWindow_GetMonitorByHandle_Failure(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given the router options are set to RouteToLaunchedWorkspace, the core native manager
		// returns a monitor, but we can't find a monitor with that handle
		ctx.RouterManager.RouteWindow(window).ReturnsNull();
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLaunchedWorkspace);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns((HMONITOR)1);
		internalCtx.MonitorManager.GetMonitorByHandle((HMONITOR)1).ReturnsNull();
		ctx.WorkspaceManager.ActiveWorkspace.Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the workspace
		ctx.RouterManager.Received().RouteWindow(window);
		internalCtx
			.CoreNativeManager.Received()
			.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
		pantry.DidNotReceive().GetWorkspaceForMonitor(Arg.Any<IMonitor>());
		pantry.Received().SetWindowWorkspace(window, workspace);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, workspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteToLaunchedWorkspace_MonitorFromWindow_GetMonitorByHandle_Success(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the router options are set to RouteToLaunchedWorkspace, the core native manager
		// returns a monitor, and we can find a workspace for the monitor
		ctx.RouterManager.RouteWindow(window).ReturnsNull();
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLaunchedWorkspace);
		internalCtx
			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns((HMONITOR)1);
		internalCtx.MonitorManager.GetMonitorByHandle((HMONITOR)1).Returns(monitor);
		pantry.GetWorkspaceForMonitor(monitor).Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the workspace
		ctx.RouterManager.Received().RouteWindow(window);
		internalCtx
			.CoreNativeManager.Received()
			.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);
		pantry.Received().GetWorkspaceForMonitor(monitor);
		pantry.Received().SetWindowWorkspace(window, workspace);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowAdded(window, workspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteWindow_WindowIsMinimized(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace routedWorkspace
	)
	{
		// Given a window is routed to a workspace, the workspace manager contains the workspace, and the window is minimized
		ctx.RouterManager.RouteWindow(window).Returns(routedWorkspace);
		ctx.WorkspaceManager.Contains(routedWorkspace).Returns(true);
		window.IsMinimized.Returns(true);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is routed to the workspace
		ctx.RouterManager.Received().RouteWindow(window);
		pantry.Received().SetWindowWorkspace(window, routedWorkspace);
		routedWorkspace.Received().MinimizeWindowStart(window);

		Assert.Single(triggersCalls.WindowRouted);
		AssertWindowMinimized(window, routedWorkspace, triggersCalls.WindowRouted[0]);
	}
	#endregion

	#region WindowRemoved
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowRemoved_NoWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window
	)
	{
		// Given the pantry does not contain the window
		pantry.GetWorkspaceForWindow(window).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is removed
		sut.PreInitialize();
		ctx.WindowManager.WindowRemoved += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then nothing happens
		pantry.DidNotReceive().RemoveWindow(window);
		Assert.Empty(triggersCalls.WindowRouted);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowRemoved_RemoveWindowFromWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		ButlerTriggersCalls triggersCalls,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given the pantry contains the window and the window is in a workspace
		pantry.GetWorkspaceForWindow(window).Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is removed
		sut.PreInitialize();
		ctx.WindowManager.WindowRemoved += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the window is removed from the workspace
		pantry.Received().RemoveWindow(window);
		workspace.Received().RemoveWindow(window);
		workspace.Received().DoLayout();
		Assert.Single(triggersCalls.WindowRouted);
	}
	#endregion

	#region WindowFocused
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowFocused_WindowIsNull(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		// Given the window is null
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 3);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is focused
		sut.PreInitialize();
		ctx.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			ctx.WindowManager,
			new WindowFocusedEventArgs() { Window = null }
		);

		// Then each workspace receives WindowFocused(null), but nothing else happens
		foreach (IWorkspace workspace in workspaces)
		{
			((IInternalWorkspace)workspace).Received().WindowFocused(null);
		}
		pantry.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowFocused_NoWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry
	)
	{
		// Given the pantry does not have a workspace for the window
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 3);
		pantry.GetWorkspaceForWindow(Arg.Any<IWindow>()).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is focused
		sut.PreInitialize();
		ctx.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			ctx.WindowManager,
			new WindowFocusedEventArgs() { Window = Substitute.For<IWindow>() }
		);

		// Then nothing happens
		foreach (IWorkspace workspace in workspaces)
		{
			((IInternalWorkspace)workspace).Received().WindowFocused(Arg.Any<IWindow>());
		}
		pantry.Received(1).GetWorkspaceForWindow(Arg.Any<IWindow>());
		pantry.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowFocused_WorkspaceIsNotActivated(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given the pantry contains the window and the window is in a workspace
		pantry.GetWorkspaceForWindow(window).Returns(workspace);
		pantry.GetMonitorForWorkspace(workspace).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is focused
		sut.PreInitialize();
		ctx.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			ctx.WindowManager,
			new WindowFocusedEventArgs() { Window = window }
		);

		// Then the window is not routed to a workspace
		pantry.Received().GetWorkspaceForWindow(window);
		pantry.Received().GetMonitorForWorkspace(workspace);
		workspace.DidNotReceive().AddWindow(window);
		chores.DidNotReceive().Activate(workspace);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowFocused_WorkspaceIsActivated(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		IWindow window,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the pantry contains the window and the window is in a workspace
		pantry.GetWorkspaceForWindow(window).Returns(workspace);
		pantry.GetMonitorForWorkspace(workspace).Returns(monitor);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is focused
		sut.PreInitialize();
		ctx.WindowManager.WindowFocused += Raise.Event<EventHandler<WindowFocusedEventArgs>>(
			ctx.WindowManager,
			new WindowFocusedEventArgs() { Window = window }
		);

		// Then the window is routed to a workspace
		pantry.Received().GetWorkspaceForWindow(window);
		pantry.Received().GetMonitorForWorkspace(workspace);
		workspace.DidNotReceive().AddWindow(window);
		chores.DidNotReceive().Activate(workspace);
	}
	#endregion

	#region WindowMinimizeStart
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowMinimizeStart_NoWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		IWindow window
	)
	{
		// Given the pantry does not have a workspace for the window
		pantry.GetWorkspaceForWindow(window).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is minimized
		sut.PreInitialize();
		ctx.WindowManager.WindowMinimizeStart += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then nothing happens
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowMinimizeStart_WorkspaceForWindowIsNotActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given the pantry has a workspace for the window
		pantry.GetWorkspaceForWindow(window).Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is minimized
		sut.PreInitialize();
		ctx.WindowManager.WindowMinimizeStart += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then MinimizeWindowStart is called on the workspace
		workspace.Received(1).MinimizeWindowStart(window);
		workspace.Received(1).DoLayout();
	}
	#endregion

	#region WindowMinimizeEnd
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowMinimizeEnd_NoWorkspaceForWindow(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IWindow window
	)
	{
		// Given the pantry does not have a workspace for the window
		pantry.GetWorkspaceForWindow(window).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is unminimized
		sut.PreInitialize();
		ctx.WindowManager.WindowMinimizeEnd += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then nothing happens
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowMinimizeEnd_WorkspaceForWindowIsNotActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IWindow window,
		IWorkspace workspace
	)
	{
		// Given the pantry has a workspace for the window
		pantry.GetWorkspaceForWindow(window).Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, Substitute.For<IButlerChores>());

		// When the window is unminimized
		sut.PreInitialize();
		ctx.WindowManager.WindowMinimizeEnd += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then MinimizeWindowEnd is called on the workspace
		workspace.Received(1).MinimizeWindowEnd(window);
		workspace.Received(1).DoLayout();
	}
	#endregion

	#region MonitorManager_MonitorsChanged
	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_RemovedMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores,
		IWorkspace workspace
	)
	{
		// Given a workspace is on a monitor that is removed
		IMonitor[] monitors = CreateMonitors(ctx, 3);
		pantry.GetWorkspaceForMonitor(monitors[0]).Returns(workspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is removed
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = new[] { monitors[0] },
				UnchangedMonitors = monitors[1..],
				AddedMonitors = Array.Empty<IMonitor>()
			}
		);

		// Then the monitor is removed from the pantry
		pantry.Received().RemoveMonitor(monitors[0]);
		workspace.Received().Deactivate();
		chores.Received().LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_RemovedMonitor_NoWorkspaceForMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerChores chores,
		IButlerPantry pantry
	)
	{
		// Given a monitor is removed and the pantry does not have a workspace for the monitor
		IMonitor[] monitors = CreateMonitors(ctx, 3);
		pantry.GetWorkspaceForMonitor(monitors[0]).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is removed
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = new[] { monitors[0] },
				UnchangedMonitors = monitors[1..],
				AddedMonitors = Array.Empty<IMonitor>()
			}
		);

		// Then nothing happens
		pantry.Received().RemoveMonitor(monitors[0]);
		pantry.DidNotReceive().SetMonitorWorkspace(Arg.Any<IMonitor>(), Arg.Any<IWorkspace>());
		chores.Received().LayoutAllActiveWorkspaces();
	}

	private static IMonitor[] CreateMonitors(IContext context, int count)
	{
		IMonitor[] monitors = Enumerable.Range(0, count).Select(_ => Substitute.For<IMonitor>()).ToArray();
		context.MonitorManager.GetEnumerator().Returns(monitors.AsEnumerable().GetEnumerator());
		return monitors;
	}

	private static IWorkspace[] CreateWorkspaces(IContext context, int count)
	{
		IWorkspace[] workspaces = Enumerable
			.Range(0, count)
			.Select(_ => Substitute.For<IWorkspace, IInternalWorkspace>())
			.ToArray();
		context.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());
		return workspaces;
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_AddedMonitor_UseSpareWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerChores chores,
		IButlerPantry pantry,
		IMonitor newMonitor
	)
	{
		// Given a monitor is added and the pantry has a spare workspace
		IMonitor[] monitors = CreateMonitors(ctx, 2);
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 3);
		ctx.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());

		pantry.GetMonitorForWorkspace(workspaces[2]).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is added
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = monitors,
				AddedMonitors = new[] { newMonitor }
			}
		);

		// Then the monitor is added to the pantry
		pantry.Received().SetMonitorWorkspace(newMonitor, workspaces[2]);
		chores.Received().Activate(workspaces[2], newMonitor);
		chores.Received().LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_AddedMonitor_CreateWorkspace_Succeeds(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerChores chores,
		IButlerPantry pantry,
		IMonitor newMonitor,
		IWorkspace newWorkspace
	)
	{
		// Given a monitor is added and the pantry does not have a spare workspace
		IMonitor[] monitors = CreateMonitors(ctx, 2);
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 2);
		ctx.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());

		ctx.WorkspaceManager.Add().Returns(newWorkspace);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is added
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = monitors,
				AddedMonitors = new[] { newMonitor }
			}
		);

		// Then the monitor is added to the pantry
		pantry.Received().SetMonitorWorkspace(newMonitor, newWorkspace);
		chores.Received().Activate(newWorkspace, newMonitor);
		chores.Received().LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_AddedMonitor_CreateWorkspace_Fails(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerChores chores,
		IButlerPantry pantry
	)
	{
		// Given a monitor is added and the pantry does not have a spare workspace
		IMonitor[] monitors = CreateMonitors(ctx, 2);
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 2);
		ctx.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());

		ctx.WorkspaceManager.Add().ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is added
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = monitors,
				AddedMonitors = new[] { monitors[1] }
			}
		);

		// Then the monitor is not added to the pantry
		pantry.DidNotReceive().SetMonitorWorkspace(monitors[1], workspaces[0]);
		chores.DidNotReceive().Activate(workspaces[0]);
		chores.Received().LayoutAllActiveWorkspaces();
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void MonitorsChanged_RemovedAndAddedMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerChores chores,
		IButlerPantry pantry,
		IMonitor newMonitor
	)
	{
		// Given a monitor is removed and another is added
		IMonitor[] monitors = CreateMonitors(ctx, 3);
		IWorkspace[] workspaces = CreateWorkspaces(ctx, 3);
		ctx.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());

		pantry.GetWorkspaceForMonitor(monitors[0]).Returns(workspaces[0]);
		pantry.GetWorkspaceForMonitor(monitors[1]).Returns(workspaces[1]);
		pantry.GetWorkspaceForMonitor(monitors[2]).Returns(workspaces[2]);

		pantry.GetMonitorForWorkspace(workspaces[0]).ReturnsNull();

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When a monitor is removed and another is added
		sut.PreInitialize();
		ctx.MonitorManager.MonitorsChanged += Raise.Event<EventHandler<MonitorsChangedEventArgs>>(
			ctx.MonitorManager,
			new MonitorsChangedEventArgs()
			{
				RemovedMonitors = new[] { monitors[0] },
				UnchangedMonitors = new[] { monitors[1], monitors[2] },
				AddedMonitors = new[] { newMonitor }
			}
		);

		// Then the monitor is removed from the pantry
		pantry.Received().RemoveMonitor(monitors[0]);
		pantry.Received().SetMonitorWorkspace(newMonitor, workspaces[0]);
		workspaces[0].Received().Deactivate();
		chores.Received().LayoutAllActiveWorkspaces();

		// And the monitor is added to the pantry
		pantry.Received().SetMonitorWorkspace(newMonitor, workspaces[0]);
		chores.Received().Activate(workspaces[0], newMonitor);
		chores.Received().LayoutAllActiveWorkspaces();
	}
	#endregion

	#region Dispose
	[Theory, AutoSubstituteData]
	internal void Dispose_Disposes(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IButlerPantry pantry,
		IButlerChores chores
	)
	{
		// Given the event handlers are disposed
		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the event handlers are disposed
		sut.Dispose();

		// Then the event handlers are disposed
		ctx.WindowManager.WindowAdded -= Arg.Any<EventHandler<WindowEventArgs>>();
		ctx.WindowManager.WindowRemoved -= Arg.Any<EventHandler<WindowEventArgs>>();
		ctx.WindowManager.WindowFocused -= Arg.Any<EventHandler<WindowFocusedEventArgs>>();
		ctx.WindowManager.WindowMinimizeStart -= Arg.Any<EventHandler<WindowEventArgs>>();
		ctx.WindowManager.WindowMinimizeEnd -= Arg.Any<EventHandler<WindowEventArgs>>();
		ctx.MonitorManager.MonitorsChanged -= Arg.Any<EventHandler<MonitorsChangedEventArgs>>();
	}
	#endregion
}
