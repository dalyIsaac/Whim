using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspacePickersTests
{
	private static void CreateNamedWorkspaces(IContext ctx, MutableRootSector root)
	{
		AddWorkspacesToManager(
			ctx,
			root,
			CreateWorkspace(ctx) with
			{
				BackingName = "Test"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test2"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test3"
			}
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Success(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		Guid workspaceId = root.WorkspaceSector.WorkspaceOrder[0];

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[workspaceId], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Failure(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		Guid workspaceId = Guid.NewGuid();

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspaces
		var result = ctx.Store.Pick(Pickers.PickAllWorkspaces()).ToArray();

		// Then we get the workspaces
		Assert.Equal(3, result.Length);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Test"));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[root.WorkspaceSector.WorkspaceOrder[0]], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Failure(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Bob the Builder"));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the workspace
		IWorkspace result = ctx.Store.Pick(Pickers.PickActiveWorkspace());

		// Then we get the workspace
		Assert.Same(activeWorkspace, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveWorkspaceId(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx);
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the workspace id
		Guid result = ctx.Store.Pick(Pickers.PickActiveWorkspaceId());

		// Then we get the workspace id
		Assert.Equal(activeWorkspace.Id, result);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickActiveLayoutEngine(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine layoutEngine1,
		ILayoutEngine layoutEngine2
	)
	{
		// Given the workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx));

		Workspace activeWorkspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(layoutEngine1, layoutEngine2),
			ActiveLayoutEngineIndex = 1
		};
		PopulateMonitorWorkspaceMap(ctx, root, CreateMonitor((HMONITOR)1), activeWorkspace);

		// When we get the layout engine
		Result<ILayoutEngine> result = ctx.Store.Pick(Pickers.PickActiveLayoutEngine(activeWorkspace.Id));

		// Then we get the layout engine
		Assert.True(result.IsSuccessful);
		Assert.Same(layoutEngine2, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllWindowsInWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		ImmutableDictionary<HWND, WindowPosition> windowPositions = ImmutableDictionary<HWND, WindowPosition>
			.Empty.Add((HWND)1, new())
			.Add((HWND)2, new())
			.Add((HWND)3, new());

		Workspace workspace = CreateWorkspace(ctx) with { WindowPositions = windowPositions };

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)2), workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		// When we get the windows
		Result<IEnumerable<IWindow>> result = ctx.Store.Pick(Pickers.PickAllWindowsInWorkspace(workspace.Id));

		// Then we get the windows
		Assert.True(result.IsSuccessful);
		Assert.Equal(3, result.Value.Count());
	}

	private static IWindow Setup_LastFocusedWindow(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		IWindow lastFocusedWindow = CreateWindow((HWND)2);

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)1), workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, lastFocusedWindow, workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		return lastFocusedWindow;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspace.Id));

		// Then we get the last focused window
		Assert.True(result.IsSuccessful);
		Assert.Same(lastFocusedWindow, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_WorkspaceNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows exist, but the workspace to search for doesn't exist
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		Guid workspaceToSearchFor = Guid.NewGuid();

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspaceToSearchFor));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindow_NoLastFocusedWindow(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows, but the last focused window isn't set
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		// When we get the last focused window
		Result<IWindow> result = ctx.Store.Pick(Pickers.PickLastFocusedWindow(workspace.Id));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindowHandle_DefaultWorkspace(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		// When we get the last focused window handle
		Result<HWND> result = ctx.Store.Pick(Pickers.PickLastFocusedWindowHandle());

		// Then we get the last focused window handle
		Assert.True(result.IsSuccessful);
		Assert.Equal(lastFocusedWindow.Handle, result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindowHandle_WorkspaceNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows exist, but the workspace to search for doesn't exist
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(
			workspace.Id,
			workspace with
			{
				LastFocusedWindowHandle = lastFocusedWindow.Handle
			}
		);

		Guid workspaceToSearchFor = Guid.NewGuid();

		// When we get the last focused window handle
		Result<HWND> result = ctx.Store.Pick(Pickers.PickLastFocusedWindowHandle(workspaceToSearchFor));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickLastFocusedWindowHandle_NoLastFocusedWindow(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows, but the last focused window isn't set
		Workspace workspace = CreateWorkspace(ctx);
		IWindow lastFocusedWindow = Setup_LastFocusedWindow(ctx, root, workspace);

		root.WorkspaceSector.Workspaces = root.WorkspaceSector.Workspaces.SetItem(workspace.Id, workspace);

		// When we get the last focused window handle
		Result<HWND> result = ctx.Store.Pick(Pickers.PickLastFocusedWindowHandle());

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	private static IWindow Setup_WindowPosition(IContext ctx, MutableRootSector root, Workspace workspace)
	{
		IMonitor monitor = CreateMonitor((HMONITOR)1);
		IWindow window = CreateWindow((HWND)2);

		workspace = workspace with
		{
			WindowPositions = workspace.WindowPositions.SetItem(
				window.Handle,
				new WindowPosition(WindowSize.Minimized, new Rectangle<int>())
			)
		};

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);
		workspace = PopulateWindowWorkspaceMap(ctx, root, window, workspace);
		PopulateWindowWorkspaceMap(ctx, root, CreateWindow((HWND)3), workspace);

		return window;
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWindowPosition_WorkspaceNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows exist, but the workspace to search for doesn't exist
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = Setup_WindowPosition(ctx, root, workspace);

		Guid workspaceToSearchFor = Guid.NewGuid();

		// When we get the window position
		Result<WindowPosition> result = ctx.Store.Pick(Pickers.PickWindowPosition(workspaceToSearchFor, window.Handle));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWindowPosition_WindowNotFound(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows exist, but the window to search for doesn't exist
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = Setup_WindowPosition(ctx, root, workspace);

		HWND hwndToSearchFor = (HWND)987;

		// When we get the window position
		Result<WindowPosition> result = ctx.Store.Pick(Pickers.PickWindowPosition(workspace.Id, hwndToSearchFor));

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWindowPosition_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		Workspace workspace = CreateWorkspace(ctx);
		IWindow window = Setup_WindowPosition(ctx, root, workspace);

		// When we get the window position
		Result<WindowPosition> result = ctx.Store.Pick(Pickers.PickWindowPosition(workspace.Id, window.Handle));

		// Then we get the window position
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickCreateLeafLayoutEngines(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces and windows
		Func<CreateLeafLayoutEngine[]> createLayoutEngines = () =>
			new CreateLeafLayoutEngine[]
			{
				(id) => Substitute.For<ILayoutEngine>(),
				(id) => Substitute.For<ILayoutEngine>(),
			};

		root.WorkspaceSector.CreateLayoutEngines = createLayoutEngines;

		// When we get the layout engines
		Result<Func<CreateLeafLayoutEngine[]>> result = ctx.Store.Pick(Pickers.PickCreateLeafLayoutEngines());

		// Then we get the layout engines
		Assert.True(result.IsSuccessful);
		Assert.Same(createLayoutEngines, result.Value);
	}
}
