<<<<<<< HEAD
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerCustomization : ICustomization
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

		MonitorManagerUtils.SetupMonitors(ctx, monitors);

		Butler butler = new(ctx, internalCtx);
		ctx.Butler.Returns(butler);
		internalCtx.ButlerEventHandlers.Returns(butler.EventHandlers);

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

		return sut;
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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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

		ctx.WindowManager.CreateWindow((HWND)1).Returns(Result.FromValue(window1));
		ctx.WindowManager.CreateWindow((HWND)2).Returns(Result.FromException<IWindow>(new WhimException("welp")));
		ctx.WindowManager.CreateWindow((HWND)3).Returns(Result.FromValue(window3));
		ctx.WindowManager.CreateWindow((HWND)4).Returns(Result.FromValue(window4));

		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		MonitorManagerUtils.SetupMonitors(ctx, new[] { Substitute.For<IMonitor>(), ctx.MonitorManager.ActiveMonitor });

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
		MonitorManagerUtils.SetupMonitors(ctx, Array.Empty<IMonitor>());
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
		MonitorManagerUtils.SetupMonitors(ctx, Array.Empty<IMonitor>());
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove("not found"));
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_String_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(3);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Initialize();
		ctx.MonitorManager.ActiveMonitor.Returns(monitors[1]);

		// When the active monitor can be found inside the WorkspaceManager
		IWorkspace activeWorkspace = workspaceManager.ActiveWorkspace;

		// Then the workspace is returned
		Assert.Equal(workspaces[1], activeWorkspace);
	}
	#endregion
}
=======
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class WorkspaceManagerCustomization : ICustomization
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

		MonitorManagerUtils.SetupMonitors(ctx, monitors);

		Butler butler = new(ctx);
		ctx.Butler.Returns(butler);

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

		return sut;
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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });

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

		ctx.WindowManager.CreateWindow((HWND)1).Returns(Result.FromValue(window1));
		ctx.WindowManager.CreateWindow((HWND)2).Returns(Result.FromException<IWindow>(new WhimException("welp")));
		ctx.WindowManager.CreateWindow((HWND)3).Returns(Result.FromValue(window3));
		ctx.WindowManager.CreateWindow((HWND)4).Returns(Result.FromValue(window4));

		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx);
		MonitorManagerUtils.SetupMonitors(ctx, new[] { Substitute.For<IMonitor>(), ctx.MonitorManager.ActiveMonitor });

		// When two workspaces are created, only one of which shares a name with a saved workspace
		workspaceManager.Add("john");
		workspaceManager.Add("jane");
		workspaceManager.Initialize();

		// Then we will have two workspaces
		IWorkspace[] workspaces = workspaceManager.ToArray();
		Assert.Equal(2, workspaces.Length);

		// The first workspace will have the same name as the saved workspace, and the first window.
		// The second window was no longer valid, so it was not added.
		IWorkspace johnWorkspace = workspaces[0];
		Assert.Equal("john", johnWorkspace.Name);
		Assert.Single(johnWorkspace.Windows);
		Assert.Equal((HWND)1, johnWorkspace.Windows.ElementAt(0).Handle);

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
		MonitorManagerUtils.SetupMonitors(ctx, Array.Empty<IMonitor>());
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
		MonitorManagerUtils.SetupMonitors(ctx, Array.Empty<IMonitor>());
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		MonitorManagerUtils.SetupMonitors(ctx, new[] { ctx.MonitorManager.ActiveMonitor });
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		// When a workspace is removed, it returns false, as the workspace was not found
		Assert.False(workspaceManager.Remove("not found"));
	}

	[Theory, AutoSubstituteData<WorkspaceManagerCustomization>]
	internal void Remove_String_Success(IContext ctx, IInternalContext internalCtx)
	{
		// Given
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(3);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
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
		IWorkspace[] workspaces = TestUtils.WorkspaceUtils.CreateWorkspaces(2);
		WorkspaceManagerTestWrapper workspaceManager = CreateSut(ctx, internalCtx, workspaces);

		workspaceManager.Initialize();
		ctx.MonitorManager.ActiveMonitor.Returns(monitors[1]);

		// When the active monitor can be found inside the WorkspaceManager
		IWorkspace activeWorkspace = workspaceManager.ActiveWorkspace;

		// Then the workspace is returned
		Assert.Equal(workspaces[1], activeWorkspace);
	}
	#endregion
}
>>>>>>> 305c778 (Remove Butler tests)
