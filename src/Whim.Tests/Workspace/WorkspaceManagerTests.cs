using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerCustomization : ICustomization
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
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

		IMonitor[] monitors = new[] { monitor1, monitor2 };
		fixture.Inject(monitors);

		IContext ctx = fixture.Freeze<IContext>();
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		WorkspaceManagerTests.SetupMonitors(ctx, monitors);

		ctx.Butler.Returns(new Butler(ctx, internalCtx));

		// Don't route things.
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns((IWorkspace?)null);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Unnecessary for tests")]
public class WorkspaceManagerTests
{
	internal class WorkspaceManagerTestWrapper : WorkspaceManager
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

			ctx.MonitorManager.GetPreviousMonitor(monitors[activeMonitorIndex])
				.Returns(monitors[(activeMonitorIndex - 1).Mod(monitors.Length)]);
			ctx.MonitorManager.GetNextMonitor(monitors[activeMonitorIndex]);
		}
	}

	private static void WindowAdded(IContext ctx, IWindow window)
	{
		// Raise the WindowAdded event.
		ctx.WindowManager.WindowAdded += Raise.Event<EventHandler<WindowEventArgs>>(
			ctx.WindowManager,
			new WindowEventArgs() { Window = window }
		);
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

		ctx.WorkspaceManager.Returns(sut);

		ctx.Butler.PreInitialize();
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

	#region Initialize
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
	internal void Add_MergeWorkspacesWithSaved(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window1,
		IWindow window3,
		IWindow window4
	)
	{
		// Given:
		// - a saved state with two workspaces, each with two handles
		// - workspace 1 "john"'s last handle not being valid
		// - workspace 2 "james" not being saved
		CoreSavedState savedState =
			new(
				new List<SavedWorkspace>()
				{
					new(
						"john",
						new() { new SavedWindow(1, new(0, 0, 0.1, 0.1)), new SavedWindow(2, new(0.1, 0.1, 0.1, 0.1)), }
					),
					new(
						"james",
						new()
						{
							new SavedWindow(3, new(0.2, 0.2, 0.1, 0.1)),
							new SavedWindow(4, new(0.3, 0.3, 0.1, 0.1)),
						}
					)
				}
			);
		internalCtx.CoreSavedStateManager.SavedState.Returns(savedState);

		internalCtx.CoreNativeManager.GetAllWindows().Returns(new[] { (HWND)1, (HWND)2, (HWND)3, (HWND)5 });

		window1.Handle.Returns((HWND)1);
		window3.Handle.Returns((HWND)3);
		window4.Handle.Returns((HWND)4);

		ctx.WindowManager.CreateWindow((HWND)1).Returns(window1);
		ctx.WindowManager.CreateWindow((HWND)2).Returns((IWindow?)null);
		ctx.WindowManager.CreateWindow((HWND)3).Returns(window3);
		ctx.WindowManager.CreateWindow((HWND)4).Returns(window4);

		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		SetupMonitors(ctx, new[] { Substitute.For<IMonitor>(), ctx.MonitorManager.ActiveMonitor });

		// When two workspaces are created, only one of which shares a name with a saved workspace
		workspaceManager.Add("john");
		workspaceManager.Add("jane");
		workspaceManager.Initialize();
		ctx.Butler.Initialize();

		// Then we will have two workspaces
		IWorkspace[] workspaces = workspaceManager.ToArray();
		Assert.Equal(2, workspaces.Length);

		// The first workspace will have the same name as the saved workspace, and the first window.
		// The second window was no longer valid, so it was not added.
		IWorkspace johnWorkspace = workspaces[0];
		Assert.Equal("john", johnWorkspace.Name);
		Assert.Single(johnWorkspace.Windows);
		Assert.Equal((HWND)1, johnWorkspace.Windows.ElementAt(0).Handle);
		internalCtx.WindowManager.Received(1).OnWindowAdded(window1);

		// The new workspace "jane" will have no windows already added, but will have two windows
		// added by the WindowManager.
		IWorkspace janeWorkspace = workspaces[1];
		Assert.Equal("jane", janeWorkspace.Name);
		Assert.Empty(janeWorkspace.Windows);
		internalCtx.WindowManager.Received(1).AddWindow((HWND)3);
		internalCtx.WindowManager.Received(1).AddWindow((HWND)5);

		internalCtx.WindowManager.DidNotReceive().AddWindow((HWND)4);
	}

	#endregion


	#region Add after initialization
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Add_AfterInitialization_SpecifyName(IContext ctx, IInternalContext internalCtx)
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
	internal void Add_AfterInitialization_SpecifyLayoutEngine(IContext ctx, IInternalContext internalCtx)
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
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspace)
		);
		Assert.Equal(workspace, result.Arguments.CurrentWorkspace);
		Assert.Null(result.Arguments.PreviousWorkspace);

		// Layout is done, and the first window is focused.
		workspace.Received(1).DoLayout();
		workspace.Received(1).FocusLastFocusedWindow();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Activate_WorkspaceDoesNotExist(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);

		// When an invalid workspace is activated, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(Substitute.For<IWorkspace>())
		);
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
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(currentWorkspace)
		);
		Assert.Equal(currentWorkspace, result.Arguments.CurrentWorkspace);
		Assert.Equal(previousWorkspace, result.Arguments.PreviousWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		previousWorkspace.Received(1).Deactivate();
		previousWorkspace.DidNotReceive().DoLayout();
		currentWorkspace.Received(1).DoLayout();
		currentWorkspace.Received(1).FocusLastFocusedWindow();
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
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.Activate(workspaces[1], monitors[0])
		);

		Assert.Equal(monitors[0], result.Arguments.Monitor);
		Assert.Equal(workspaces[1], result.Arguments.CurrentWorkspace);
		Assert.Equal(workspaces[0], result.Arguments.PreviousWorkspace);

		workspaces[0].DidNotReceive().Deactivate();
		workspaces[0].Received(1).DoLayout();
		workspaces[0].DidNotReceive().FocusLastFocusedWindow();

		workspaces[1].DidNotReceive().Deactivate();
		workspaces[1].Received(1).DoLayout();
		workspaces[1].Received(1).FocusLastFocusedWindow();
	}
	#endregion

	#region ActivateAdjacent
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateAdjacent_UseActiveMonitor(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given no arguments are provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised.
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent()
		);

		// Then ActiveMonitor is called.
		_ = ctx.MonitorManager.Received().ActiveMonitor;
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateAdjacent_UseProvidedMonitor(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given a monitor is provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent(monitors[0])
		);

		// Then ActiveMonitor is not called.
		_ = ctx.MonitorManager.DidNotReceive().ActiveMonitor;
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateAdjacent_SingleWorkspace(IContext ctx, IInternalContext internalCtx)
	{
		// Given there is only a single monitor and workspace...
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);
		IWorkspace[] workspaces = CreateWorkspaces(1);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent()
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateAdjacent_CannotFindWorkspaceForMonitor(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given a fake monitor is provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent(Substitute.For<IMonitor>())
		);
	}

	[Theory]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, false, false, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, false, false, 0)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, false, true, 2)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, false, true, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, true, false, 2)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(1, 2, true, false, 0)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 2, true, true, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, true, true, 0)]
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
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[firstActivatedIdx], monitors[0]);
		workspaceManager.Activate(workspaces[secondActivatedIdx], monitors[1]);
		IWorkspace activatedWorkspace = workspaces[activatedWorkspaceIdx];

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent(monitors[0], reverse, skipActive)
		);

		// Then the raised event will match the expected
		Assert.Equal(activatedWorkspace, result.Arguments.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateAdjacent_GetNextWorkspace_SkipActive_NoActive(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given there are no free workspaces
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called with skipActive, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateAdjacent(skipActive: true)
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivateNext(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given no arguments are provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised.
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivateNext()
		);

		// Then ActiveMonitor is called.
		_ = ctx.MonitorManager.Received().ActiveMonitor;
		Assert.Equal(workspaces[1], result.Arguments.CurrentWorkspace);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void ActivatePrevious(IContext ctx, IInternalContext internalCtx, IMonitor[] monitors)
	{
		// Given no arguments are provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When ActivateAdjacent is called, then an event is raised.
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.ActivatePrevious()
		);

		// Then ActiveMonitor is called.
		_ = ctx.MonitorManager.Received().ActiveMonitor;
		Assert.Equal(workspaces[1], result.Arguments.CurrentWorkspace);
	}
	#endregion

	#region MoveWindowToAdjacentWorkspace
	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToAdjacentWorkspace_WindowIsNull(IContext ctx, IInternalContext internalCtx)
	{
		// Given the provided window is null, and workspaces' last focused window are null
		IWorkspace[] workspaces = CreateWorkspaces(2);
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.LastFocusedWindow.Returns((IWindow?)null);
		}
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When MoveWindowToAdjacentWorkspace is called
		workspaceManager.MoveWindowToAdjacentWorkspace(null);

		// Then the workspaces do not receive any calls
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().AddWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().DoLayout();
		}
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToAdjacentWorkspace_DoesNotContainWindow(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the provided window is not contained in any workspace
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When MoveWindowToAdjacentWorkspace is called
		workspaceManager.MoveWindowToAdjacentWorkspace(window);

		// Then the workspaces do not receive any calls
		foreach (IWorkspace workspace in workspaces)
		{
			workspace.DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().AddWindow(Arg.Any<IWindow>());
			workspace.DidNotReceive().DoLayout();
		}
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToAdjacentWorkspace_NoAdjacentWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IWindow window
	)
	{
		// Given the provided window is contained in a workspace, but there is no adjacent workspace
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);

		IWorkspace[] workspaces = CreateWorkspaces(1);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		WindowAdded(ctx, window);

		workspaces[0].ClearReceivedCalls();

		// When MoveWindowToAdjacentWorkspace is called
		workspaceManager.MoveWindowToAdjacentWorkspace(window);

		// Then the workspace does not receive any calls
		workspaces[0].DidNotReceive().RemoveWindow(Arg.Any<IWindow>());
		workspaces[0].DidNotReceive().AddWindow(Arg.Any<IWindow>());
		workspaces[0].DidNotReceive().DoLayout();
	}

	[Theory]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, false, false, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, false, false, 0)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, false, true, 2)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, false, true, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 1, true, false, 2)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(1, 2, true, false, 0)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(0, 2, true, true, 1)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(2, 1, true, true, 0)]
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
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Activate(workspaces[firstActivatedIdx], monitors[0]);
		workspaceManager.Activate(workspaces[secondActivatedIdx], monitors[1]);
		IWorkspace activatedWorkspace = workspaces[activatedWorkspaceIdx];

		WindowAdded(ctx, window);

		ClearWorkspaceReceivedCalls(workspaces);
		window.ClearReceivedCalls();

		// When MoveWindowToAdjacentWorkspace is called
		workspaceManager.MoveWindowToAdjacentWorkspace(window, reverse, skipActive);

		// Then the window is removed from the first workspace and added to the activated workspace
		workspaces[firstActivatedIdx].Received(1).RemoveWindow(window);
		activatedWorkspace.Received(1).AddWindow(window);
		activatedWorkspace.Received(1).DoLayout();
		window.Received(1).Focus();

		Assert.Equal(workspaces[activatedWorkspaceIdx], workspaceManager.GetWorkspaceForWindow(window));
	}
	#endregion

	#region SwapActiveWorkspaceWithAdjacentMonitor
	[Theory]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(false)]
	[InlineAutoSubstituteData<WorkspaceManagerCustomization>(true)]
	internal void SwapActiveWorkspaceWithAdjacentMonitor_NoAdjacentMonitor(
		bool reverse,
		IContext ctx,
		IInternalContext internalCtx
	)
	{
		// Given there is only a single monitor and workspace...
		IMonitor[] monitors = new[] { ctx.MonitorManager.ActiveMonitor };
		SetupMonitors(ctx, monitors);
		IWorkspace[] workspaces = CreateWorkspaces(1);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		// When SwapActiveWorkspaceWithAdjacentMonitor is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.SwapActiveWorkspaceWithAdjacentMonitor(reverse)
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void SwapActiveWorkspaceWithAdjacentMonitor_CouldNotFindWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given a fake monitor is provided
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetNextMonitor(Arg.Any<IMonitor>()).Returns(Substitute.For<IMonitor>());

		// When SwapActiveWorkspaceWithAdjacentMonitor is called, then no event is raised
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.SwapActiveWorkspaceWithAdjacentMonitor(reverse: false)
		);
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void SwapActiveWorkspaceWithAdjacentMonitor_Success(
		IContext ctx,
		IInternalContext internalCtx,
		IMonitor[] monitors
	)
	{
		// Given there are two workspaces and monitors
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		ActivateWorkspacesOnMonitors(workspaceManager, workspaces, monitors);

		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetNextMonitor(Arg.Any<IMonitor>()).Returns(monitors[1]);

		// When SwapActiveWorkspaceWithAdjacentMonitor is called, then an event is raised
		var result = Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h => ctx.Butler.MonitorWorkspaceChanged += h,
			h => ctx.Butler.MonitorWorkspaceChanged -= h,
			() => workspaceManager.SwapActiveWorkspaceWithAdjacentMonitor(reverse: false)
		);

		// Then the raised event will match the expected
		Assert.Equal(monitors[0], result.Arguments.Monitor);
		Assert.Equal(workspaces[1], result.Arguments.CurrentWorkspace);
		Assert.Equal(workspaces[0], result.Arguments.PreviousWorkspace);

		// The old workspace is deactivated, the new workspace is laid out, and the first window is
		// focused.
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
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
	internal void MoveWindowToWorkspace_SameWorkspace(IContext ctx, IInternalContext internalCtx, IWindow window)
	{
		// Given there are 3 workspaces
		IWorkspace[] workspaces = CreateWorkspaces(3);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// and the window is added
		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		IWorkspace workspace = workspaces[0];
		workspace.ClearReceivedCalls();

		// When a window in a workspace is moved to the same workspace
		workspaceManager.MoveWindowToWorkspace(workspace, window);

		// Then the window is not removed or added to any workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().AddWindow(window);
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
		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();
		window.ClearReceivedCalls();

		// When a window in a workspace is moved to another workspace
		workspaceManager.MoveWindowToWorkspace(workspaces[1], window);

		// Then the window is removed from the first workspace and added to the second
		workspaces[0].Received(1).RemoveWindow(window);
		workspaces[1].Received(1).AddWindow(window);
		workspaces[0].Received(1).DoLayout();
		workspaces[1].Received(1).DoLayout();
		window.Received(1).Focus();
		window.DidNotReceive().Hide();
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowToWorkspace_Success_ActivateSingleWorkspace(
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

		// and the window is added
		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);
		workspaces[2].ClearReceivedCalls();
		window.ClearReceivedCalls();

		// When a window in a workspace is moved to another workspace
		workspaceManager.MoveWindowToWorkspace(workspaces[1], window);

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

		WindowAdded(ctx, window);
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

		WindowAdded(ctx, window);
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

		WindowAdded(ctx, window);
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

		WindowAdded(ctx, window);
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

		WindowAdded(ctx, window);
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

		WindowAdded(ctx, window);
		workspace.ClearReceivedCalls();

		// When a window which is in a workspace is moved to a point which doesn't correspond to any workspaces
		workspaceManager.MoveWindowToPoint(window, new Point<int>());

		// Then the window is not removed from the old workspace and not added to the new workspace
		workspace.DidNotReceive().RemoveWindow(window);
		workspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
		workspace.DidNotReceive().DoLayout();
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
		workspace.DidNotReceive().DoLayout();
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

		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[1]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		Point<int> givenPoint = new() { X = 2460, Y = 720 };
		Point<double> expectedPoint = new() { X = 0.5, Y = 0.5 };

		window.ClearReceivedCalls();

		// When a window is moved to a point
		workspaceManager.MoveWindowToPoint(window, givenPoint);

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.Received(1).RemoveWindow(window);
		activeWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());
		activeWorkspace.Received(1).DoLayout();

		targetWorkspace.Received(1).MoveWindowToPoint(window, expectedPoint);
		targetWorkspace.Received(1).DoLayout();

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

		WindowAdded(ctx, window);
		activeWorkspace.ClearReceivedCalls();

		ctx.MonitorManager.GetMonitorAtPoint(Arg.Any<IPoint<int>>()).Returns(monitors[0]);
		activeWorkspace.RemoveWindow(window).Returns(true);

		Point<int> givenPoint = new() { X = 960, Y = 540 };
		Point<double> expectedPoint = new() { X = 0.5, Y = 0.5 };
		window.ClearReceivedCalls();

		// When a window is moved to a point
		workspaceManager.MoveWindowToPoint(window, givenPoint);

		// Then the window is removed from the old workspace and added to the new workspace
		activeWorkspace.DidNotReceive().RemoveWindow(window);
		activeWorkspace.Received(1).MoveWindowToPoint(window, expectedPoint);
		anotherWorkspace.DidNotReceive().MoveWindowToPoint(window, Arg.Any<Point<double>>());

		Assert.Equal(activeWorkspace, workspaceManager.GetWorkspaceForWindow(window));

		window.Received(1).Focus();
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
		workspaceManager.CreateLayoutEngines = () =>
			new CreateLeafLayoutEngine[]
			{
				(id) => Substitute.For<ILayoutEngine>(),
				(id) => Substitute.For<ILayoutEngine>()
			};
		workspaceManager.Initialize();

		// Then changing the layout engine should trigger the event
		Assert.Raises<ActiveLayoutEngineChangedEventArgs>(
			h => workspaceManager.ActiveLayoutEngineChanged += h,
			h => workspaceManager.ActiveLayoutEngineChanged -= h,
			() => workspaceManager.ActiveWorkspace.CycleLayoutEngine(false)
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
			workspace.CycleLayoutEngine(false);
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

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void MoveWindowEdgesInDirection_NoWindow(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);
		workspaceManager.Initialize();

		workspaces[0].LastFocusedWindow.Returns((IWindow?)null);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
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
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
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
		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>(), window);

		// Then nothing happens
		Workspaces_DidNotMoveWindowEdgesInDirection(workspaces);
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
		WindowAdded(ctx, window);
		ClearWorkspaceReceivedCalls(workspaces);

		// When moving the window edges in a direction
		workspaceManager.MoveWindowEdgesInDirection(Direction.Left, new Point<int>());

		// Then the window edges are moved
		workspaces[0].Received(1).MoveWindowEdgesInDirection(Direction.Left, Arg.Any<IPoint<double>>(), window);
		workspaces[0].Received(1).DoLayout();
		workspaces[1]
			.DidNotReceive()
			.MoveWindowEdgesInDirection(Arg.Any<Direction>(), Arg.Any<IPoint<double>>(), Arg.Any<IWindow?>());
		workspaces[1].DidNotReceive().DoLayout();
	}
	#endregion
}
