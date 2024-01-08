using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	internal void WindowAdded_WindowIsMinimized(
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
		// Given the window is minimized
		window.IsMinimized.Returns(true);

		ButlerEventHandlers sut = new(ctx, internalCtx, triggers, pantry, chores);

		// When the window is added
		sut.PreInitialize();
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);

		// Then the workspace receives MinimizeWindowStart
		pantry.Received().SetWindowWorkspace(window, workspace);
		workspace.Received().MinimizeWindowStart(window);

		Assert.Empty(triggersCalls.WindowRouted);

		AssertWindowAdded(window, workspace, triggersCalls.WindowRouted[0]);
	}

	#endregion
}
