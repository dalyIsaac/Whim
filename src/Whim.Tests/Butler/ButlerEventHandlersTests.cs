using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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
		routedWorkspace.Received().AddWindow(window);

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
		goodWorkspace.Received().AddWindow(window);

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
		activeWorkspace.Received().AddWindow(window);

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
		lastTrackedActiveWorkspace.Received().AddWindow(window);

		Assert.Single(triggersCalls.WindowRouted);

		AssertWindowAdded(window, lastTrackedActiveWorkspace, triggersCalls.WindowRouted[0]);
	}

	[Theory, AutoSubstituteData<ButlerEventHandlersCustomization>]
	internal void WindowAdded_RouteToLaunchedWorkspace_GetMonitorAtPoint_ReturnsNull(
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
		// Given the router options are set to RouteToLaunchedWorkspace and the monitor manager returns null for GetMonitorAtPoint
		ctx.RouterManager.RouteWindow(window).ReturnsNull();
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLaunchedWorkspace);
		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).ReturnsNull();
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
		pantry.Received().SetWindowWorkspace(window, workspace);
		workspace.Received().AddWindow(window);

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

		AssertWindowAdded(window, routedWorkspace, triggersCalls.WindowRouted[0]);
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
		IWorkspace[] workspaces = Enumerable
			.Range(0, 3)
			.Select(_ => Substitute.For<IWorkspace, IInternalWorkspace>())
			.ToArray();
		ctx.WorkspaceManager.GetEnumerator().Returns(workspaces.AsEnumerable().GetEnumerator());

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
		workspace.Received().MinimizeWindowStart(window);
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
		workspace.Received().MinimizeWindowEnd(window);
	}
	#endregion
}
