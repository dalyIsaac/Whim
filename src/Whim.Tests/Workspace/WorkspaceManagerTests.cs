using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;
using static Xunit.Assert;

namespace Whim.Tests;

public class WorkspaceManagerCustomization : ICustomization
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
	public void Customize(IFixture fixture)
	{
		// By default, create two monitors.
		IMonitor monitor1 = Substitute.For<IMonitor>();
		monitor1
			.WorkingArea
			.Returns(
				new Location<int>()
				{
					X = 0,
					Y = 0,
					Width = 1920,
					Height = 1080
				}
			);
		IMonitor monitor2 = Substitute.For<IMonitor>();
		monitor2
			.WorkingArea
			.Returns(
				new Location<int>()
				{
					X = 1920,
					Y = 360,
					Width = 1080,
					Height = 720
				}
			);

		IMonitor[] monitors = new[] { monitor1, monitor2 };
		fixture.Inject(monitors);

		IContext ctx = fixture.Freeze<IContext>();
		WorkspaceManagerTests.SetupMonitors(ctx, monitors);

		// Don't route things.
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
public class WorkspaceManagerTests
{
	private class WorkspaceManagerTestWrapper : WorkspaceManager
	{
		// Yes, I know it's bad to have `_triggers` be `internal` in `WorkspaceManager`.
		public WorkspaceManagerTriggers InternalTriggers => _triggers;

		public WorkspaceManagerTestWrapper(IContext ctx, IInternalContext internalCtx)
			: base(ctx, internalCtx) { }

		public void Add(IWorkspace workspace) => _workspaces.Add(workspace);
	}

	/// <summary>
	/// Set up the monitors for the <see cref="IMonitorManager"/> to have the specified monitors.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="monitors"></param>
	/// <param name="activeMonitorIndex"></param>
	internal static void SetupMonitors(IContext ctx, IMonitor[] monitors, int activeMonitorIndex = 0)
	{
		ctx.MonitorManager.GetEnumerator().Returns((_) => ((IEnumerable<IMonitor>)monitors).GetEnumerator());
		ctx.MonitorManager.Length.Returns(monitors.Length);
		if (monitors.Length > 0)
		{
			ctx.MonitorManager.ActiveMonitor.Returns(monitors[activeMonitorIndex]);
		}
	}

	private static WorkspaceManagerTestWrapper CreateSut(
		IContext ctx,
		IInternalContext internalCtx,
		params IWorkspace[] workspaces
	)
	{
		WorkspaceManagerTestWrapper sut = new(ctx, internalCtx);
		foreach (IWorkspace workspace in workspaces)
		{
			sut.Add(workspace);
		}
		return sut;
	}

	private static IWorkspace[] CreateWorkspaces(int count)
	{
		IWorkspace[] workspaces = new IWorkspace[count];
		for (int i = 0; i < count; i++)
		{
			workspaces[i] = Substitute.For<IWorkspace, IInternalWorkspace>();
		}
		return workspaces;
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

	private static void ActivateWorkspacesOnMonitors(
		WorkspaceManagerTestWrapper workspaceManager,
		IWorkspace[] workspaces,
		IMonitor[] monitors
	)
	{
		for (int i = 0; i < workspaces.Length; i++)
		{
			workspaceManager.Activate(workspaces[i], monitors[i]);
		}
	}

	#region Add
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_BeforeInitialization(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When a workspace is added before initialization
		workspaceManager.Add();

		// Then the workspace is not added
		Assert.Empty(workspaceManager);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_BeforeInitialization_DefaultName(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

		// When a workspace is added
		workspaceManager.Add();
		workspaceManager.Initialize();

		// Then the workspace is created with default name.
		Assert.Equal("Workspace 1", workspaceManager.Single().Name);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_BeforeInitialization_CustomName(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

		// When a workspace is added
		workspaceManager.Add("workspace");
		workspaceManager.Initialize();

		// Then the workspace is created with default name.
		Assert.Equal("workspace", workspaceManager.Single().Name);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_SpecifyName(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, Array.Empty<IMonitor>());
		workspaceManager.Initialize();

		// When a workspace is added with a name
		workspaceManager.Add("named workspace");

		// Then the workspace is created with the specified name.
		Assert.Equal("named workspace", workspaceManager.Single().Name);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_SpecifyLayoutEngine(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, Array.Empty<IMonitor>());
		workspaceManager.Initialize();

		// When a workspace is added with a layout engine
		workspaceManager.Add("workspace", new CreateLeafLayoutEngine[] { (id) => new TestLayoutEngine() });

		// Then the workspace is created with the specified layout engine.
		Assert.IsType<TestLayoutEngine>(workspaceManager.Single().ActiveLayoutEngine);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_BeforeInitialization_DefaultLayoutEngine(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

		// When a workspace is added
		workspaceManager.Add();
		workspaceManager.Initialize();

		// Then the workspace is created with default layout engine.
		Assert.IsType<ColumnLayoutEngine>(workspaceManager.Single().ActiveLayoutEngine);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_ThrowsWhenNoLayoutEngines(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When the workspace manager is initialized after a workspace is added
		workspaceManager.CreateLayoutEngines = Array.Empty<CreateLeafLayoutEngine>;
		workspaceManager.Add();

		// Then an exception is thrown
		Assert.Throws<InvalidOperationException>(workspaceManager.Initialize);
	}
	#endregion

	#region Remove
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_Workspace_RequireAtLeastNWorkspace(IContext ctx, IInternalContext internalCtx)
	{
		// Given the workspace manager has two workspaces and there are two monitors
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When a workspace is removed
		bool result = workspaceManager.Remove(workspaces[0]);

		// Then it returns false, as there must be at least two workspaces, since there are two monitors
		Assert.False(result);
		workspaces[1].DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_Workspace_NotFound(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(
			ctx,
			internalCtx,
			new[] { Substitute.For<IWorkspace>(), Substitute.For<IWorkspace>(), Substitute.For<IWorkspace>() }
		);

		// When a workspace is removed
		bool result = workspaceManager.Remove(Substitute.For<IWorkspace>());

		// Then it returns false, as the workspace was not found
		Assert.False(result);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_Workspace_Success(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		workspaces[0].Windows.Returns(new[] { window });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When a workspace is removed, it returns true, WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceRemoved += h,
			h => workspaceManager.WorkspaceRemoved -= h,
			() => Assert.True(workspaceManager.Remove(workspaces[0]))
		);
		Assert.Equal(workspaces[0], result.Arguments.Workspace);

		// and the window is added to the last remaining workspace
		workspaces[1].Received(1).AddWindow(window);
		workspaces[1].Received(1).DoLayout();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_String_NotFound(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove("not found"));
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_String_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaces[0].Name.Returns("workspace");

		// When a workspace is removed, it returns true, and WorkspaceRemoved is raised
		var result = Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceRemoved += h,
			h => workspaceManager.WorkspaceRemoved -= h,
			() => workspaceManager.Remove("workspace")
		);
		Assert.Equal(workspaces[0], result.Arguments.Workspace);
	}
	#endregion

	#region TryGet
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void TryGet_Null(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When getting a workspace which does not exist, then null is returned
		Assert.Null(workspaceManager.TryGet("not found"));
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void TryGet_Success(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		workspace.Name.Returns("workspace");
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace, workspaceManager.TryGet("workspace"));
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void SquareBracket_Get(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		workspace.Name.Returns("workspace");
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });

		// When getting a workspace which does exist, then the workspace is returned
		Assert.Equal(workspace, workspaceManager["workspace"]);
	}
	#endregion

	#region GetEnumerator
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When enumerating the workspaces, then the workspaces are returned
		IEnumerator<IWorkspace> enumerator = workspaceManager.GetEnumerator();
		List<IWorkspace> enumeratedWorkspaces = new();

		while (enumerator.MoveNext())
		{
			enumeratedWorkspaces.Add(enumerator.Current);
		}

		// Then
		Assert.Equal(workspaces, enumeratedWorkspaces);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void IEnumerable_GetEnumerator(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When enumerating the workspaces, then the workspaces are returned
		IEnumerator enumerator = ((IEnumerable)workspaceManager).GetEnumerator();
		List<IWorkspace> enumeratedWorkspaces = new();

		while (enumerator.MoveNext())
		{
			enumeratedWorkspaces.Add((IWorkspace)enumerator.Current);
		}

		// Then
		Assert.Equal(workspaces, enumeratedWorkspaces);
	}
	#endregion

	#region Activate
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Activate_NoPreviousWorkspace(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspace);

		// When a workspace is activated when there are no other workspaces activated, then it is
		// focused on the active monitor and raises an event,
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspace)
		);
		Assert.Equal(workspace, result.Arguments.CurrentWorkspace);
		Assert.Null(result.Arguments.PreviousWorkspace);

		// Layout is done, and the first window is focused.
		workspace.Received(1).DoLayout();
		workspace.Received(1).FocusFirstWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Activate_WithPreviousWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace previousWorkspace,
		IWorkspace currentWorkspace
	)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, previousWorkspace, currentWorkspace);

		workspaceManager.Activate(previousWorkspace);

		// Reset wrapper
		previousWorkspace.ClearReceivedCalls();
		currentWorkspace.ClearReceivedCalls();

		// When a workspace is activated when there is another workspace activated, then the old
		// workspace is deactivated, the new workspace is activated, and an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(currentWorkspace)
		);
		Assert.Equal(currentWorkspace, result.Arguments.CurrentWorkspace);
		Assert.Equal(previousWorkspace, result.Arguments.PreviousWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		previousWorkspace.Received(1).Deactivate();
		previousWorkspace.DidNotReceive().DoLayout();
		currentWorkspace.Received(1).DoLayout();
		currentWorkspace.Received(1).FocusFirstWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Activate_MultipleMonitors(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given there are two workspaces and monitors
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		// Reset wrapper
		ClearWorkspaceReceivedCalls(workspaces);

		// When a workspace is activated on a monitor which already has a workspace activated, then
		// an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => workspaceManager.MonitorWorkspaceChanged += h,
			h => workspaceManager.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspaces[1], monitors[0])
		);

		Assert.Equal(monitors[0], result.Arguments.Monitor);
		Assert.Equal(workspaces[1], result.Arguments.CurrentWorkspace);
		Assert.Equal(workspaces[0], result.Arguments.PreviousWorkspace);

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].Received(1).DoLayout();
		workspaces[0].DidNotReceive().FocusFirstWindow();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].Received(1).DoLayout();
		workspaces[1].Received(1).FocusFirstWindow();
	}

	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 2)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1)]
	[Theory]
	internal void ActivatePrevious(int currentIdx, int prevIdx, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[currentIdx]);

		// Reset wrapper
		workspaces[currentIdx].ClearReceivedCalls();

		// When the previous workspace is activated, then the previous workspace is activated
		workspaceManager.ActivatePrevious();

		workspaces[currentIdx].Received(1).Deactivate();
		workspaces[currentIdx].DidNotReceive().DoLayout();
		workspaces[currentIdx].DidNotReceive().FocusFirstWindow();

		workspaces[prevIdx].DidNotReceive().Deactivate();
		workspaces[prevIdx].Received(1).DoLayout();
		workspaces[prevIdx].Received(1).FocusFirstWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivePrevious_CannotFindActiveMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[0]);

		// Reset wrapper
		workspaces[0].ClearReceivedCalls();
		ctx.MonitorManager.ActiveMonitor.Returns(Substitute.For<IMonitor>());

		// When the previous workspace is activated, then the previous workspace is activated
		workspaceManager.ActivatePrevious();

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].DidNotReceive().DoLayout();
		workspaces[0].DidNotReceive().FocusFirstWindow();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].DidNotReceive().DoLayout();
		workspaces[1].DidNotReceive().FocusFirstWindow();
	}

	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 0)]
	[Theory]
	internal void ActivateNext(int currentIdx, int nextIdx, IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(3);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[currentIdx]);

		// Reset wrapper
		workspaces[currentIdx].ClearReceivedCalls();

		// When the next workspace is activated, then the next workspace is activated
		workspaceManager.ActivateNext();

		workspaces[currentIdx].Received(1).Deactivate();
		workspaces[currentIdx].DidNotReceive().DoLayout();
		workspaces[currentIdx].DidNotReceive().FocusFirstWindow();

		workspaces[nextIdx].DidNotReceive().Deactivate();
		workspaces[nextIdx].Received(1).DoLayout();
		workspaces[nextIdx].Received(1).FocusFirstWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateNext_CannotFindActiveMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[0]);

		// Reset wrapper
		workspaces[0].ClearReceivedCalls();
		ctx.MonitorManager.ActiveMonitor.Returns(Substitute.For<IMonitor>());

		// When the next workspace is activated, then the next workspace is activated
		workspaceManager.ActivateNext();

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].DidNotReceive().DoLayout();
		workspaces[0].DidNotReceive().FocusFirstWindow();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].DidNotReceive().DoLayout();
		workspaces[1].DidNotReceive().FocusFirstWindow();
	}
	#endregion

	#region GetMonitorForWorkspace
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void GetMonitorForWorkspace_NoWorkspace(IContext ctx, IInternalContext internalCtx, IWorkspace workspace)
	{
		// Given
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspace);

		workspaceManager.Activate(workspace);

		// When we get the monitor for a workspace which isn't in the workspace manager
		IMonitor? monitor = workspaceManager.GetMonitorForWorkspace(Substitute.For<IWorkspace>());

		// Then null is returned
		Assert.Null(monitor);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void GetMonitorForWorkspace_Success(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[0]);
		workspaceManager.Activate(workspaces[1], monitors[1]);

		// When we get the monitor for a workspace which is in the workspace manager
		IMonitor? monitor = workspaceManager.GetMonitorForWorkspace(workspaces[0]);

		// Then the monitor is returned
		Assert.Equal(monitors[0], monitor);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void LayoutAllActiveWorkspaces(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When we layout all active workspaces
		workspaceManager.LayoutAllActiveWorkspaces();

		// Then all active workspaces are laid out
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
	}

	#region WindowAdded
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowAdded_NoRouter(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors, IWindow window)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window is added
		RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() => workspaceManager.WindowAdded(window)
		);

		// Then the window is added to the active workspace
		Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowAdded_RouterReturnsInvalidWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// There is a router which routes the window to a different workspace
		ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);

		// When a window is added
		RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() => workspaceManager.WindowAdded(window)
		);

		// Then the window is added to the active workspace
		Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowAdded_NoRouter_GetMonitorAtWindowCenter(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window,
		IWorkspace imposterWorkspace
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ctx.RouterManager.RouteWindow(window).Returns(imposterWorkspace);
		ctx.MonitorManager.GetMonitorAtPoint(point: Arg.Any<IPoint<int>>()).Returns(monitors[0]);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window is added
		RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() => workspaceManager.WindowAdded(window)
		);

		// Then the window is added to the active workspace
		Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
		imposterWorkspace.DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowAdded_Router(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors, IWindow window)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// There is a router which routes the window to a different workspace
		ctx.RouterManager.RouteWindow(window).Returns(workspaces[1]);

		// When a window is added
		RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() => workspaceManager.WindowAdded(window)
		);

		// Then the window is added to the workspace returned by the router
		workspaces[0].DidNotReceive().AddWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowAdded_RouterToActive(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// There is a router which routes the window to the active workspace
		ctx.RouterManager.RouteWindow(window).Returns((IWorkspace?)null);
		ctx.RouterManager.RouteToActiveWorkspace.Returns(true);

		// When a window is added
		RaisedEvent<RouteEventArgs> routeEvent = Assert.Raises<RouteEventArgs>(
			h => workspaceManager.WindowRouted += h,
			h => workspaceManager.WindowRouted -= h,
			() => workspaceManager.WindowAdded(window)
		);

		// Then the window is added to the active workspace
		Assert_WindowAddedToWorkspace1(workspaces, window, routeEvent);
	}

	private static void Assert_WindowAddedToWorkspace1(
		IWorkspace[] workspaces,
		IWindow window,
		RaisedEvent<RouteEventArgs> routeEvent
	)
	{
		workspaces[0].Received(1).AddWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);

		Assert.Equal(workspaces[0], routeEvent.Arguments.CurrentWorkspace);
		Assert.Equal(window, routeEvent.Arguments.Window);
	}
	#endregion

	#region WindowRemoved
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowRemoved_NotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window is removed
		workspaceManager.WindowRemoved(window);

		// Then the window is removed from all workspaces
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowRemoved_Found(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors, IWindow window)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is removed
		workspaceManager.WindowRemoved(window);

		// Then the window is removed from the workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].DidNotReceive().RemoveWindow(window);
	}
	#endregion

	#region MoveWindowToWorkspace
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToWorkspace_NoWindow(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is not in a workspace is moved to a workspace
		workspaceManager.MoveWindowToWorkspace(workspaces[0]);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[1].DidNotReceive().AddWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToWorkspace_CannotFindWindow(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();

		// When a window not in any workspace is moved to a workspace
		workspaceManager.MoveWindowToWorkspace(workspaces[0], window);

		// Then the window is not removed or added to any workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToWorkspace_Success_WindowNotHidden(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[0], monitors[0]);
		workspaceManager.Activate(workspaces[1], monitors[1]);

		// and the window is added
		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();
		window.ClearReceivedCalls();

		// When a window in a workspace is moved to another workspace
		workspaceManager.MoveWindowToWorkspace(workspaces[1], window);

		// Then the window is removed from the first workspace and added to the second
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
		window.DidNotReceive().Hide();
	}
	#endregion

	#region MoveWindowToMonitor
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToMonitor_NoWindow(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given there are 2 workspaces
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When there is no focused window
		workspaceManager.MoveWindowToMonitor(monitors[0]);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[1].DidNotReceive().AddWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToMonitor_NoPreviousMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is not in a workspace is moved to a monitor
		workspaceManager.MoveWindowToMonitor(monitors[0], window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToMonitor_PreviousMonitorIsNewMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces, and the window has been added
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the same monitor
		workspaceManager.MoveWindowToMonitor(monitors[0], window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToMonitor_WorkspaceForMonitorNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given there are 2 workspaces, and the window has been added
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to a monitor which isn't registered
		workspaceManager.MoveWindowToMonitor(Substitute.For<IMonitor>(), window);

		// Then the window is not added to the workspace
		workspaces[0].DidNotReceive().RemoveWindow(window);
		workspaces[1].DidNotReceive().AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to a monitor
		workspaceManager.MoveWindowToMonitor(monitors[1], window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToPreviousMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		ctx.MonitorManager.GetPreviousMonitor(monitors[0]).Returns(monitors[1]);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the previous monitor
		workspaceManager.MoveWindowToPreviousMonitor(window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToNextMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		ctx.MonitorManager.GetNextMonitor(monitors[0]).Returns(monitors[1]);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When a window which is in a workspace is moved to the next monitor
		workspaceManager.MoveWindowToNextMonitor(window);

		// Then the window is removed from the old workspace and added to the new workspace
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
	}

	#region MoveWindowToPoint
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToPoint_TargetWorkspaceNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		workspaceManager.Activate(workspace, monitors[0]);

		workspaceManager.WindowAdded(window);
		workspace.ClearReceivedCalls();

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		workspaceManager.MoveWindowToPoint(window, new Point<int>());

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToPoint_OldWorkspaceNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		workspaceManager.Activate(workspace, monitors[0]);

		workspace.ClearReceivedCalls();
		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[0]);

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		workspaceManager.MoveWindowToPoint(window, new Point<int>());

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToPoint_Success_DifferentWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		IWorkspace activeWorkspace = workspaces[0];
		IWorkspace targetWorkspace = workspaces[1];
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		workspaceManager.WindowAdded(window);
		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[1]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		// When a window is moved to a point
		workspaceManager.MoveWindowToPoint(window, new Point<int>());

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.Received(1).RemoveWindow(window);
		activeWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());

		Assert.Equal(targetWorkspace, workspaceManager.GetWorkspaceForWindow(window));

		window.Received(1).Focus();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
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
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(
			ctx,
			internalCtx,
			new[] { activeWorkspace, anotherWorkspace }
		);

		workspaceManager.Activate(activeWorkspace, monitors[0]);

		workspaceManager.WindowAdded(window);
		activeWorkspace.ClearReceivedCalls();

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[0]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		// When a window is moved to a point
		workspaceManager.MoveWindowToPoint(window, new Point<int>());

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.DidNotReceive().RemoveWindow(window);
		activeWorkspace.Received(1).MoveWindowToPoint(window, Arg.Any<Point<double>>());
		anotherWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());

		Assert.Equal(activeWorkspace, workspaceManager.GetWorkspaceForWindow(window));

		window.Received(1).Focus();
	}
	#endregion

	#region WindowFocused
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowFocused_WindowIsNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given the window is null
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		IWindow? window = null;

		// When WindowFocused is called
		workspaceManager.WindowFocused(window);

		// Then each workspace is notified, but no workspace is laid out
		WindowFocused_Notification_NoLayout(workspaces, window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowFocused_WindowNotInWorkspace(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window is defined, but not in any workspace
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When WindowFocused is called
		workspaceManager.WindowFocused(window);

		// Then each workspace is notified, but no workspace is laid out
		WindowFocused_Notification_NoLayout(workspaces, window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowFocused_WindowWorkspaceNotShown(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window is defined, and in a workspace, but the workspace is not shown
		ctx.MonitorManager.GetEnumerator().Returns(Array.Empty<IMonitor>().AsEnumerable().GetEnumerator());
		IWorkspace[] workspaces = CreateWorkspaces(2);
		IInternalWorkspace[] internalWorkspaces = workspaces.Cast<IInternalWorkspace>().ToArray();
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaceManager.WindowAdded(window);

		ClearWorkspaceReceivedCalls(workspaces);

		// When WindowFocused is called
		workspaceManager.WindowFocused(window);

		// Then each workspace is notified, and the workspace is laid out
		internalWorkspaces[0].Received(1).WindowFocused(window);
		internalWorkspaces[1].Received(1).WindowFocused(window);

		workspaces[0].Received(1).DoLayout();
		workspaces[1].DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowFocused_WindowWorkspaceIsShown(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given the window is defined, in a workspace, and the workspace is shown
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaceManager.WindowAdded(window);
		workspaceManager.WindowFocused(window);

		ClearWorkspaceReceivedCalls(workspaces);

		// When WindowFocused is called
		workspaceManager.WindowFocused(window);

		// Then each workspace is notified, but no workspace is laid out
		WindowFocused_Notification_NoLayout(workspaces, window);
	}

	private static void WindowFocused_Notification_NoLayout(IWorkspace[] workspaces, IWindow? window)
	{
		IInternalWorkspace[] internalWorkspaces = workspaces.Cast<IInternalWorkspace>().ToArray();
		internalWorkspaces[0].Received(1).WindowFocused(window);
		internalWorkspaces[1].Received(1).WindowFocused(window);

		workspaces[0].DidNotReceive().DoLayout();
		workspaces[1].DidNotReceive().DoLayout();
	}
	#endregion

	#region WindowMinimizeStart
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowMinimizeStart_CouldNotFindWindow(
		IContext ctx,
		IInternalContext internalCtx,
		IWorkspace workspace,
		IWindow window
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });

		// When a window is minimized, but the window is not found in any workspace
		workspaceManager.WindowMinimizeStart(window);

		// Then nothing happens
		workspace.DidNotReceive().RemoveWindow(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowMinimizeStart(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		// A window is added to the workspace
		workspaceManager.WindowAdded(window);

		// When a window is minimized
		workspaceManager.WindowMinimizeStart(window);

		// Then the workspace is notified
		((IInternalWorkspace)workspace)
			.Received(1)
			.WindowMinimizeStart(window);
	}
	#endregion

	#region WindowMinimizeEnd
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowMinimizeEnd(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		// A window is added to the workspace
		workspaceManager.WindowAdded(window);

		// When a window is restored
		workspaceManager.WindowMinimizeEnd(window);

		// Then the window is routed to the workspace
		((IInternalWorkspace)workspace)
			.Received(1)
			.WindowMinimizeEnd(window);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WindowMinizeEnd_Fail(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given
		IWorkspace workspace = Substitute.For<IWorkspace, IInternalWorkspace>();
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		// When a window which isn't tracked is restored
		workspaceManager.WindowMinimizeEnd(window);

		// Then the workspace is not notified
		((IInternalWorkspace)workspace)
			.DidNotReceive()
			.WindowMinimizeEnd(window);
	}
	#endregion

	#region MonitorManager_MonitorsChanged
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MonitorManager_MonitorsChanged_Removed(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		// When a monitor is removed, and a monitor not tracked in the WorkspaceManager is removed
		workspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = Array.Empty<IMonitor>(),
				RemovedMonitors = new IMonitor[] { monitors[0], Substitute.For<IMonitor>() },
				UnchangedMonitors = new IMonitor[] { monitors[1] }
			}
		);

		// Then the workspace is deactivated
		workspaces[0].Received(1).Deactivate();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MonitorManager_MonitorsChanged_Added_CreateWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		int calls = 0;
		CreateLeafLayoutEngine[] createLayoutEngines()
		{
			calls++;
			return new CreateLeafLayoutEngine[] { (identity) => Substitute.For<ILayoutEngine>() };
		}
		workspaceManager.CreateLayoutEngines = createLayoutEngines;

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		// When a monitor is added
		workspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { Substitute.For<IMonitor>() },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { monitors[0], monitors[1] }
			}
		);

		// Then a new workspace is created
		Assert.Equal(1, calls);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MonitorManager_MonitorsChanged_Added_UseExistingWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[0], monitors[0]);
		workspaceManager.Activate(workspaces[1], monitors[1]);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();

		// When a monitor is added
		workspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { Substitute.For<IMonitor>() },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { monitors[0], monitors[1] }
			}
		);

		// Then the workspace is activated
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
		workspaces[2].Received(2).DoLayout();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MonitorManager_MonitorsChanged_CannotAddWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWorkspace workspace
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, new[] { workspace });
		workspaceManager.Activate(workspace, monitors[0]);

		// Reset the wrapper
		workspace.ClearReceivedCalls();
		workspaceManager.CreateLayoutEngines = Array.Empty<CreateLeafLayoutEngine>;

		// When a monitor is added
		workspaceManager.MonitorManager_MonitorsChanged(
			this,
			new MonitorsChangedEventArgs()
			{
				AddedMonitors = new IMonitor[] { monitors[0], monitors[1] },
				RemovedMonitors = Array.Empty<IMonitor>(),
				UnchangedMonitors = new IMonitor[] { monitors[0] }
			}
		);

		// Then the workspace is not activated
		Assert.Single(workspaceManager);
	}
	#endregion

	#region AddProxyLayoutEngine
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void NoProxyLayoutEngines(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When
		workspaceManager.Initialize();

		// Then
		Assert.IsNotAssignableFrom<BaseProxyLayoutEngine>(workspaceManager.ActiveWorkspace.ActiveLayoutEngine);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void AddProxyLayoutEngine(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When
		workspaceManager.AddProxyLayoutEngine((engine) => Substitute.For<TestProxyLayoutEngine>(engine));
		workspaceManager.Initialize();

		// Then
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspaceManager.ActiveWorkspace.ActiveLayoutEngine);
	}
	#endregion

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void DoesDispose(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When the workspace manager is disposed
		workspaceManager.Dispose();

		// Then the workspaces are disposed
		workspaces[0].Received(1).Dispose();
		workspaces[1].Received(1).Dispose();
	}

	#region WorkspaceManagerTriggers
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_ActiveLayoutEngineChanged(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();

		// Then changing the layout engine should trigger the event
		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => workspaceManager.ActiveLayoutEngineChanged += h,
			h => workspaceManager.ActiveLayoutEngineChanged -= h,
			workspaceManager.ActiveWorkspace.NextLayoutEngine
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_ActiveLayoutEngineChanged_DoesNotThrowWhenNoListener(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();

		// When creating a workspace
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;

		// Then changing the layout engine should trigger the event
		try
		{
			workspace.NextLayoutEngine();
		}
		catch
		{
			Assert.True(false);
		}
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceRenamed(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();

		// When creating a workspace
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;

		// Then renaming the workspace should trigger the event
		Assert.Raises<WorkspaceRenamedEventArgs>(
			h => workspaceManager.WorkspaceRenamed += h,
			h => workspaceManager.WorkspaceRenamed -= h,
			() => workspace.Name = "new name"
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceRenamed_DoesNotThrowWhenNoListener(
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();

		// When creating a workspace
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;

		// Then renaming the workspace should trigger the event
		try
		{
			workspace.Name = "new name";
		}
		catch
		{
			Assert.True(false);
		}
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceLayoutStarted(
		IContext ctx,
		IInternalContext internalCtx,
		Func<CreateLeafLayoutEngine[]> createLayoutEngines
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();
		workspaceManager.CreateLayoutEngines = createLayoutEngines;

		// When creating a workspace
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;
		workspaceManager.Activate(workspace);

		// Then starting the layout should trigger the event
		Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceLayoutStarted += h,
			h => workspaceManager.WorkspaceLayoutStarted -= h,
			workspace.DoLayout
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceLayoutStarted_DoesNotThrowWhenNoListener(
		IContext ctx,
		IInternalContext internalCtx,
		Func<CreateLeafLayoutEngine[]> createLayoutEngines
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();
		workspaceManager.CreateLayoutEngines = createLayoutEngines;

		// When creating a workspace
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;
		workspaceManager.Activate(workspace);

		// Then starting the layout should trigger the event
		try
		{
			workspace.DoLayout();
		}
		catch
		{
			Assert.True(false);
		}
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceLayoutCompleted(
		IContext ctx,
		IInternalContext internalCtx,
		Func<CreateLeafLayoutEngine[]> createLayoutEngines
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();
		workspaceManager.CreateLayoutEngines = createLayoutEngines;

		// When
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;
		workspaceManager.Activate(workspace);

		// Then completing the layout should trigger the event
		Assert.Raises<WorkspaceEventArgs>(
			h => workspaceManager.WorkspaceLayoutCompleted += h,
			h => workspaceManager.WorkspaceLayoutCompleted -= h,
			workspace.DoLayout
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void WorkspaceManagerTriggers_WorkspaceLayoutCompleted_DoesNotThrowWhenNoListener(
		IContext ctx,
		IInternalContext internalCtx,
		Func<CreateLeafLayoutEngine[]> createLayoutEngines
	)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		workspaceManager.Initialize();
		workspaceManager.CreateLayoutEngines = createLayoutEngines;

		// When
		workspaceManager.Add("workspace");
		IWorkspace workspace = workspaceManager["workspace"]!;
		workspaceManager.Activate(workspace);

		// Then completing the layout should trigger the event
		try
		{
			workspace.DoLayout();
		}
		catch
		{
			Assert.True(false);
		}
	}
	#endregion

	#region ActiveWorkspace
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActiveWorkspace_CannotFindMonitor(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ctx.MonitorManager.ActiveMonitor.Returns(Substitute.For<IMonitor>());

		// When the active monitor can't be found inside the WorkspaceManager
		IWorkspace activeWorkspace = workspaceManager.ActiveWorkspace;

		// Then the first workspace is returned
		Assert.Equal(workspaces[0], activeWorkspace);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActiveWorkspace_CanFindMonitor(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Initialize();
		ctx.MonitorManager.ActiveMonitor.Returns(monitors[1]);

		// When the active monitor can be found inside the WorkspaceManager
		IWorkspace activeWorkspace = workspaceManager.ActiveWorkspace;

		// Then the workspace is returned
		Assert.Equal(workspaces[1], activeWorkspace);
	}
	#endregion

	#region MoveWindowEdgesInDirection
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowEdgesInDirection_NoWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaceManager.Initialize();

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		workspaces[0]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowEdgesInDirection_NoContainingWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaces[1].Windows.Returns(new[] { window });
		workspaces[1].LastFocusedWindow.Returns(window);

		workspaceManager.Initialize();
		ctx.MonitorManager.ActiveMonitor.Returns(monitors[1]);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		workspaces[0]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowEdgesInDirection_NoContainingMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

		workspaces[1].Windows.Returns(new[] { window });
		ctx.RouterManager.RouteWindow(window).Returns(workspaces[1]);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaceManager.Initialize();
		workspaceManager.WindowAdded(window);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>(), window);

		// Then nothing happens
		workspaces[0]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowEdgesInDirection_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors,
		IWindow window
	)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaces[0].Windows.Returns(new[] { window });
		workspaces[0].LastFocusedWindow.Returns(window);
		ctx.MonitorManager.ActiveMonitor.Returns(monitors[0]);

		workspaceManager.Initialize();
		workspaceManager.WindowAdded(window);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then the window edges are moved
		workspaces[0].Received(1).MoveWindowEdgesInDirection(Direction.Left, Arg.Any<IPoint<double>>(), window);
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
	}
	#endregion
}
