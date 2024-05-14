using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

public class ButlerChoresCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		// By default, create two monitors.
		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1.WorkingArea.Returns(
			new Rectangle<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			}
		);
		monitor1.Handle.Returns((HMONITOR)1);

		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2.WorkingArea.Returns(
			new Rectangle<int>()
			{
				X = 1920,
				Y = 360,
				Width = 1080,
				Height = 720
			}
		);
		monitor2.Handle.Returns((HMONITOR)2);

		IMonitor[] monitors = new[] { monitor1, monitor2 };
		fixture.Inject(monitors);

		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		MonitorManagerUtils.SetupMonitors(ctx, monitors);

		Butler butler = new(ctx, internalCtx);
		ctx.Butler.Returns(butler);
		internalCtx.ButlerEventHandlers.Returns(butler.EventHandlers);

		// Don't route things.
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);
	}
}

public class ButlerChoresTests
{
	#region Utils
	private static void WindowAdded(IInternalContext internalCtx, IWindow window)
	{
		// Raise the WindowAdded event.
		internalCtx.ButlerEventHandlers.OnWindowAdded(new WindowEventArgs() { Window = window });
	}

	private static void ActivateWorkspacesOnMonitors(
		ButlerChores butlerChores,
		IWorkspace[] workspaces,
		IMonitor[] monitors
	)
	{
		for (int i = 0; i < workspaces.Length; i++)
		{
			butlerChores.Activate(workspaces[i], monitors[i]);
		}
	}

	private static void ClearWorkspaceReceivedCalls(IWorkspace[] workspaces)
	{
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.ClearReceivedCalls();
		}

		foreach (IInternalWorkspace workspace in workspaces.Cast<IInternalWorkspace>())
		{
			workspace.ClearReceivedCalls();
		}
	}

	/// <summary>
	/// Set up the monitors for the <see cref="IMonitorManager"/> to have the specified monitors.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="monitors"></param>
	/// <param name="activeMonitorIndex"></param>
	private static void SetupMonitors(IContext ctx, IMonitor[] monitors, int activeMonitorIndex = 0)
	{
		ctx.MonitorManager.GetEnumerator().Returns((_) => ((IEnumerable<IMonitor>)monitors).GetEnumerator());
		ctx.MonitorManager.Length.Returns(monitors.Length);
		if (monitors.Length > 0)
		{
			ctx.MonitorManager.ActiveMonitor.Returns(monitors[activeMonitorIndex]);

			ctx.MonitorManager.GetPreviousMonitor(monitors[activeMonitorIndex])
				.Returns(monitors[(activeMonitorIndex - 1).Mod(monitors.Length)]);
			ctx.MonitorManager.GetNextMonitor(monitors[activeMonitorIndex]);
		}
	}

	private static ButlerChores CreateSut(IContext ctx, IInternalContext internalCtx, params IWorkspace[] workspaces)
	{
		ButlerChores sut = new(ctx, internalCtx);

		if (workspaces.Length > 0)
		{
			ctx.WorkspaceManager.ActiveWorkspace.Returns(workspaces[0]);
		}
		ctx.WorkspaceManager.GetEnumerator().Returns(_ => workspaces.ToList().GetEnumerator());

		foreach (IWorkspace w in workspaces)
		{
			ctx.WorkspaceManager.Contains(w).Returns(true);
		}

		return sut;
	}
	#endregion

	#region Activate
	[Theory, AutoSubstituteData]
	internal void Activate_WorkspaceNotFound(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given the workspace does not exist
		ButlerChores sut = new(ctx, internalCtx);

		// When Activate is called
		sut.Activate(workspace);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData]
	internal void Activate_MonitorNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the monitor does not exist
		ButlerChores sut = new(ctx, internalCtx);
		ctx.WorkspaceManager.Contains(workspace).Returns(true);

		// When Activate is called
		sut.Activate(workspace, monitor);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData]
	internal void Activate_WorkspaceAlreadyActive(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the workspace is already activated
		ButlerChores sut = new(ctx, internalCtx);

		ctx.WorkspaceManager.Contains(workspace).Returns(true);
		ctx.MonitorManager.GetEnumerator().Returns(new List<IMonitor> { monitor }.GetEnumerator());
		ctx.Butler.Pantry.GetMonitorForWorkspace(workspace).Returns(monitor);

		// When Activate is called
		sut.Activate(workspace, monitor);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void Activate_NoPreviousWorkspace(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspace);

		// When a workspace is activated when there are no other workspaces activated, then it is
		// focused on the active monitor and raises an event,
		butlerChores.Activate(workspace);

		// Layout is done, and the first window is focused.
		internalCtx
			.Butler.Received(1)
			.TriggerMonitorWorkspaceChanged(
				new MonitorWorkspaceChangedEventArgs()
				{
					CurrentWorkspace = workspace,
					Monitor = ctx.MonitorManager.ActiveMonitor
				}
			);
		workspace.Received(1).DoLayout();
		workspace.Received(1).FocusLastFocusedWindow();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void Activate_WorkspaceDoesNotExist(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		ButlerChores butlerChores = CreateSut(ctx, internalCtx);

		// When an invalid workspace is activated
		butlerChores.Activate(Substitute.For<IWorkspace>());

		// Then no event is raised
		internalCtx.Butler.DidNotReceive().TriggerMonitorWorkspaceChanged(Arg.Any<MonitorWorkspaceChangedEventArgs>());
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void Activate_WorkspaceAlreadyActivated(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the workspace is already assigned to the monitor
		ButlerChores butlerChores = CreateSut(ctx, internalCtx);
		ctx.Butler.Pantry.SetMonitorWorkspace(monitor, workspace);

		// When we activate the workspace
		butlerChores.Activate(workspace);

		// Then nothing happens
		internalCtx.Butler.DidNotReceive().TriggerMonitorWorkspaceChanged(Arg.Any<MonitorWorkspaceChangedEventArgs>());
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void Activate_WithPreviousWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace previousWorkspace,
		IWorkspace currentWorkspace
	)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, previousWorkspace, currentWorkspace);

		butlerChores.Activate(previousWorkspace);

		// Reset wrapper
		previousWorkspace.ClearReceivedCalls();
		currentWorkspace.ClearReceivedCalls();

		// When a workspace is activated when there is another workspace activated
		butlerChores.Activate(currentWorkspace);

		// Then the old workspace is deactivated, the new workspace is activated, an event is raised
		// and the first window is focused.
		internalCtx.Butler.TriggerMonitorWorkspaceChanged(
			new MonitorWorkspaceChangedEventArgs()
			{
				CurrentWorkspace = currentWorkspace,
				PreviousWorkspace = previousWorkspace,
				Monitor = ctx.MonitorManager.ActiveMonitor
			}
		);
		previousWorkspace.Received(1).Deactivate();
		previousWorkspace.DidNotReceive().DoLayout();
		currentWorkspace.Received(1).DoLayout();
		currentWorkspace.Received(1).FocusLastFocusedWindow();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void Activate_MultipleMonitors(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given there are two workspaces and monitors
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		// Reset wrapper
		ClearWorkspaceReceivedCalls(workspaces);

		// When a workspace is activated on a monitor which already has a workspace activated, then
		// an event is raised
		butlerChores.Activate(workspaces[1], monitors[0]);

		// Then an event is raised.
		internalCtx.Butler.TriggerMonitorWorkspaceChanged(
			new MonitorWorkspaceChangedEventArgs()
			{
				CurrentWorkspace = workspaces[1],
				PreviousWorkspace = workspaces[0],
				Monitor = monitors[0]
			}
		);

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].Received(1).DoLayout();
		workspaces[0].DidNotReceive().FocusLastFocusedWindow();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].Received(1).DoLayout();
		workspaces[1].Received(1).FocusLastFocusedWindow();
	}
	#endregion

	[Theory, AutoSubstituteData]
	internal void FocusMonitorDesktop(IContext ctx, IInternalContext internalCtx, IMonitor monitor)
	{
		// Given
		ButlerChores sut = new(ctx, internalCtx);

		// When
		sut.FocusMonitorDesktop(monitor);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetDesktopWindow();
		internalCtx.CoreNativeManager.Received(1).SetForegroundWindow(Arg.Any<HWND>());
		internalCtx.WindowManager.Received(1).OnWindowFocused(null);
		internalCtx.MonitorManager.Received(1).ActivateEmptyMonitor(monitor);
	}

	#region ActivateAdjacent
	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void ActivateAdjacent_UseActiveMonitor(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given no arguments are provided
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised.
		butlerChores.ActivateAdjacent();

		// Then ActiveMonitor is called.
		_ = ctx.MonitorManager.Received().ActiveMonitor;
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void ActivateAdjacent_UseProvidedMonitor(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given a monitor is provided
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised
		butlerChores.ActivateAdjacent(monitors[0]);

		// Then ActiveMonitor is not called.
		_ = ctx.MonitorManager.DidNotReceive().ActiveMonitor;
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void ActivateAdjacent_SingleWorkspace(IContext ctx, IInternalContext internalCtx)
	{
		// Given there is only a single monitor and workspace...
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(1);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => butlerChores.ActivateAdjacent()
		);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void ActivateAdjacent_CannotFindWorkspaceForMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given a fake monitor is provided
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => butlerChores.ActivateAdjacent(Substitute.For<IMonitor>())
		);
	}

	[Theory]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, false, false, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, false, false, 0)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, false, true, 2)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, false, true, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, true, false, 2)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(1, 2, true, false, 0)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 2, true, true, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, true, true, 0)]
	internal void ActivateAdjacent_GetNextWorkspace(
		int firstActivatedIdx,
		int secondActivatedIdx,
		bool skipActive,
		bool reverse,
		int activatedWorkspaceIdx,
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given we have activated the first and second workspace
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		butlerChores.Activate(workspaces[firstActivatedIdx], monitors[0]);
		butlerChores.Activate(workspaces[secondActivatedIdx], monitors[1]);
		IWorkspace activatedWorkspace = workspaces[activatedWorkspaceIdx];

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called
		butlerChores.ActivateAdjacent(monitors[0], reverse, skipActive);

		// Then the raised event will match the expected
		internalCtx.Butler.TriggerMonitorWorkspaceChanged(
			new MonitorWorkspaceChangedEventArgs() { CurrentWorkspace = activatedWorkspace, Monitor = monitors[0] }
		);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void ActivateAdjacent_GetNextWorkspace_SkipActive_NoActive(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given there are no free workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called with skipActive, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => butlerChores.ActivateAdjacent(skipActive: true)
		);
	}
	#endregion

	#region MoveWindowToAdjacentWorkspace
	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToAdjacentWorkspace_WindowIsNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given the provided window is null, and workspaces' last focused window are null
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.LastFocusedWindow.Returns((IWindow?)null);
		}
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		// When MoveWindowToAdjacentWorkspace is called
		butlerChores.MoveWindowToAdjacentWorkspace(null);

		// Then the workspaces do not receive any calls
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().AddWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().DoLayout();
		}
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToAdjacentWorkspace_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the provided window is not contained in any workspace
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		// When MoveWindowToAdjacentWorkspace is called
		butlerChores.MoveWindowToAdjacentWorkspace(window);

		// Then the workspaces do not receive any calls
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().AddWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().DoLayout();
		}
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToAdjacentWorkspace_NoAdjacentWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the provided window is contained in a workspace, but there is no adjacent workspace
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);

		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(1);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);

		workspaces[0].ClearReceivedCalls();

		// When MoveWindowToAdjacentWorkspace is called
		butlerChores.MoveWindowToAdjacentWorkspace(window);

		// Then the workspace does not receive any calls
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[0].DidNotReceive().AddWindow(Arg.Any<IWindow>());
		workspaces[0].DidNotReceive().DoLayout();
	}

	[Theory]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, false, false, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, false, false, 0)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, false, true, 2)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, false, true, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 1, true, false, 2)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(1, 2, true, false, 0)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(0, 2, true, true, 1)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(2, 1, true, true, 0)]
	internal void MoveWindowToAdjacentWorkspace_Success(
		int firstActivatedIdx,
		int secondActivatedIdx,
		bool skipActive,
		bool reverse,
		int activatedWorkspaceIdx,
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given we have activated the first and second workspace, and the window is added to the first workspace
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		butlerChores.Activate(workspaces[firstActivatedIdx], monitors[0]);
		butlerChores.Activate(workspaces[secondActivatedIdx], monitors[1]);

		ctx.WorkspaceManager.ActiveWorkspace.Returns(workspaces[firstActivatedIdx]);

		WindowAdded(internalCtx, window);

		ClearWorkspaceReceivedCalls(workspaces);
		window.ClearReceivedCalls();

		// When MoveWindowToAdjacentWorkspace is called
		butlerChores.MoveWindowToAdjacentWorkspace(window, reverse, skipActive);

		// Then the window is removed from the first workspace and added to the activated workspace
		workspaces[firstActivatedIdx].Received(1).RemoveWindow(window);
		workspaces[activatedWorkspaceIdx].Received(1).AddWindow(window);
		workspaces[activatedWorkspaceIdx].Received(1).DoLayout();
		window.Received(1).Focus();
		Assert.Equal(workspaces[activatedWorkspaceIdx], ctx.Butler.Pantry.GetWorkspaceForWindow(window));
	}
	#endregion

	#region SwapWorkspaceWithAdjacentMonitor
	[Theory]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(false)]
	[InlineAutoSubstituteData<ButlerChoresCustomization>(true)]
	internal void SwapWorkspaceWithAdjacentMonitor_NoAdjacentMonitor(
		bool reverse,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given there is only a single monitor and workspace...
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(1);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When SwapWorkspaceWithAdjacentMonitor is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => butlerChores.SwapWorkspaceWithAdjacentMonitor(reverse: reverse)
		);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void SwapWorkspaceWithAdjacentMonitor_CouldNotFindWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given a fake monitor is provided
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetNextMonitor(Arg.Any<IMonitor>()).Returns(Substitute.For<IMonitor>());

		// When SwapWorkspaceWithAdjacentMonitor is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => butlerChores.SwapWorkspaceWithAdjacentMonitor(reverse: false)
		);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void SwapWorkspaceWithAdjacentMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given there are two workspaces and monitors
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetNextMonitor(Arg.Any<IMonitor>()).Returns(monitors[1]);

		// When SwapWorkspaceWithAdjacentMonitor is called, then an event is raised
		butlerChores.SwapWorkspaceWithAdjacentMonitor(reverse: false);

		// Then the raised event will match the expected
		internalCtx.Butler.TriggerMonitorWorkspaceChanged(
			new MonitorWorkspaceChangedEventArgs()
			{
				CurrentWorkspace = workspaces[1],
				PreviousWorkspace = workspaces[0],
				Monitor = monitors[0]
			}
		);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
	}

	#endregion

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void LayoutAllActiveWorkspaces(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When we layout all active workspaces
		butlerChores.LayoutAllActiveWorkspaces();

		// Then all active workspaces are laid out
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
	}

	#region MoveWindowToWorkspace
	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToWorkspace_NoWindow(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is not in a workspace is moved to a workspace
		butlerChores.MoveWindowToWorkspace(workspaces[0]);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[1].DidNotReceive().AddWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToWorkspace_CannotFindWindow(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();

		// When a window not in any workspace is moved to a workspace
		butlerChores.MoveWindowToWorkspace(workspaces[0], window);

		// Then the window is not removed or added to any workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToWorkspace_SameWorkspace(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		// and the window is added
		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		IWorkspace workspace = workspaces[0];
		workspace.ClearReceivedCalls();

		// When a window in a workspace is moved to the same workspace
		butlerChores.MoveWindowToWorkspace(workspace, window);

		// Then the window is not removed or added to any workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToWorkspace_Success_WindowNotHidden(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		butlerChores.Activate(workspaces[0], monitors[0]);
		butlerChores.Activate(workspaces[1], monitors[1]);

		// and the window is added
		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();
		window.ClearReceivedCalls();

		// When a window in a workspace is moved to another workspace
		butlerChores.MoveWindowToWorkspace(workspaces[1], window);

		// Then the window is removed from the first workspace and added to the second
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
		window.Received(1).Focus();
		window.DidNotReceive().Hide();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToWorkspace_Success_ActivateSingleWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(3);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		butlerChores.Activate(workspaces[0], monitors[0]);

		// and the window is added
		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();
		window.ClearReceivedCalls();

		// When a window in a workspace is moved to another workspace
		butlerChores.MoveWindowToWorkspace(workspaces[1], window);

		// Then the window is removed from the first workspace and added to the second
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
		workspaces[0].Received(1).Deactivate();
		workspaces[1].Received(1).DoLayout();
		window.Received(1).Focus();
		window.DidNotReceive().Hide();
	}
	#endregion

	#region MoveWindowToMonitor
	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToMonitor_NoWindow(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given there are 2 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When there is no focused window
		butlerChores.MoveWindowToMonitor(monitors[0]);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[1].DidNotReceive().AddWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToMonitor_NoPreviousMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is not in a workspace is moved to a monitor
		butlerChores.MoveWindowToMonitor(monitors[0], window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToMonitor_PreviousMonitorIsNewMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces, and the window has been added
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the same monitor
		butlerChores.MoveWindowToMonitor(monitors[0], window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToMonitor_WorkspaceForMonitorNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces, and the window has been added
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to a monitor which isn't registered
		butlerChores.MoveWindowToMonitor(Substitute.For<IMonitor>(), window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to a monitor
		butlerChores.MoveWindowToMonitor(monitors[1], window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}
	#endregion

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToPreviousMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		ctx.MonitorManager.GetPreviousMonitor(monitors[0]).Returns(monitors[1]);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the previous monitor
		butlerChores.MoveWindowToPreviousMonitor(window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToNextMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		ctx.MonitorManager.GetNextMonitor(monitors[0]).Returns(monitors[1]);

		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the next monitor
		butlerChores.MoveWindowToNextMonitor(window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}

	#region MoveWindowToPoint
	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToPoint_TargetWorkspaceNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, new[] { workspace });
		butlerChores.Activate(workspace, monitors[0]);

		WindowAdded(internalCtx, window);
		workspace.ClearReceivedCalls();

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		butlerChores.MoveWindowToPoint(window, new Point<int>());

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToPoint_OldWorkspaceNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, new[] { workspace });
		butlerChores.Activate(workspace, monitors[0]);

		workspace.ClearReceivedCalls();
		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[0]);

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		butlerChores.MoveWindowToPoint(window, new Point<int>());

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToPoint_Success_DifferentWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		IWorkspace activeWorkspace = workspaces[0];
		IWorkspace targetWorkspace = workspaces[1];
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		ActivateWorkspacesOnMonitors(butlerChores, workspaces, monitors);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[1]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		Point<int> givenPoint = new() { X = 2460, Y = 720 };
		Point<double> expectedPoint = new() { X = 0.5, Y = 0.5 };

		window.ClearReceivedCalls();

		// When a window is moved to a point
		butlerChores.MoveWindowToPoint(window, givenPoint);

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.Received(1).RemoveWindow(window);
		activeWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());

		targetWorkspace.Received(1).MoveWindowToPoint(window, expectedPoint);
		Assert.Equal(targetWorkspace, ctx.Butler.Pantry.GetWorkspaceForWindow(window));

		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowToPoint_Success_SameWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace activeWorkspace,
		IWorkspace anotherWorkspace,
		IWindow window
	)
	{
		// Given
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, new[] { activeWorkspace, anotherWorkspace });

		butlerChores.Activate(activeWorkspace, monitors[0]);

		WindowAdded(internalCtx, window);
		activeWorkspace.ClearReceivedCalls();

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[0]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		Point<int> givenPoint = new() { X = 960, Y = 540 };
		Point<double> expectedPoint = new() { X = 0.5, Y = 0.5 };
		window.ClearReceivedCalls();

		// When a window is moved to a point
		butlerChores.MoveWindowToPoint(window, givenPoint);

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.DidNotReceive().RemoveWindow(window);
		activeWorkspace.Received(1).MoveWindowToPoint(window, expectedPoint);
		anotherWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());

		Assert.Equal(activeWorkspace, ctx.Butler.Pantry.GetWorkspaceForWindow(window));

		window.Received(1).Focus();
	}
	#endregion


	#region MoveWindowEdgesInDirection
	private static void Workspaces_DidNotMoveWindowEdgesInDirection(IWorkspace[] workspaces)
	{
		for (int i = 0; i < workspaces.Length; i++)
		{
			workspaces[i]
				.DidNotReceive()
				.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
			workspaces[i].DidNotReceive().DoLayout();
		}
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowEdgesInDirection_NoWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		butlerChores.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowEdgesInDirection_NoContainingWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		workspaces[1].LastFocusedWindow.Returns(window);

		ctx.MonitorManager.ActiveMonitor.Returns(monitors[1]);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		butlerChores.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowEdgesInDirection_NoContainingMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

		ctx.RouterManager.RouteWindow(window).Returns(workspaces[1]);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);
		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		butlerChores.MoveWindowEdgesInDirection(Direction.Left, new Point<int>(), window);

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
	}

	[Theory, AutoSubstituteData<ButlerChoresCustomization>]
	internal void MoveWindowEdgesInDirection_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = WorkspaceUtils.CreateWorkspaces(2);
		ButlerChores butlerChores = CreateSut(ctx, internalCtx, workspaces);

		ctx.Butler.Pantry.SetMonitorWorkspace(monitors[0], workspaces[0]);
		ctx.WorkspaceManager.ActiveWorkspace.LastFocusedWindow.Returns(window);

		WindowAdded(internalCtx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		butlerChores.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then the window edges are moved
		workspaces[0].Received(1).MoveWindowEdgesInDirection(Direction.Left, Arg.Any<IPoint<double>>(), window, false);
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(
				Arg.Any<Direction>(),
				Arg.Any<IPoint<double>>(),
				Arg.Any<IWindow?>(),
				Arg.Any<bool>()
			);
	}
	#endregion
}
