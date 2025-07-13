namespace Whim.Tests;

public class WindowFocusedTransformTests
{
	private static readonly HMONITOR HMONITOR_1 = (HMONITOR)1;
	private static readonly HMONITOR HMONITOR_2 = (HMONITOR)2;
	private static readonly HMONITOR HMONITOR_3 = (HMONITOR)3;

	private static ImmutableArray<IMonitor> Setup_Monitors(MutableRootSector rootSector)
	{
		IMonitor monitor1 = CreateMonitor(HMONITOR_1);
		IMonitor monitor2 = CreateMonitor(HMONITOR_2);
		IMonitor monitor3 = CreateMonitor(HMONITOR_3);

		ImmutableArray<IMonitor> monitors = [monitor1, monitor2, monitor3];
		rootSector.MonitorSector.Monitors = monitors;
		return monitors;
	}

	private static void Setup_MonitorFromWindow(IInternalContext internalCtx, HWND hwnd, HMONITOR hmonitor)
	{
		internalCtx
			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(hmonitor);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotNull_WindowIsTrackedByMapSector(IContext ctx, MutableRootSector rootSector, IWindow window)
	{
		// Given the window is tracked by the map sector
		ImmutableArray<IMonitor> monitors = Setup_Monitors(rootSector);

		window.Handle.Returns((HWND)2);
		Workspace workspace = CreateWorkspace();

		PopulateThreeWayMap(rootSector, monitors[1], workspace, window);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor indices are updated.
		Assert.Equal(HMONITOR_2, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal(HMONITOR_2, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowIsNotNull_WindowIsNotTrackedByMapSector(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given the window is not tracked by the map sector
		IWindow window = CreateWindow((HWND)1);
		Setup_Monitors(rootSector);

		Setup_MonitorFromWindow(internalCtx, window.Handle, HMONITOR_3);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is updated based on MonitorFromWindow
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void HandleIsNull(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there is no window and GetForegroundWindow returns null
		Setup_Monitors(rootSector);

		internalCtx.CoreNativeManager.GetForegroundWindow().Returns((HWND)0);

		// Mock out the next call to make sure it doesn't matter
		internalCtx
			.CoreNativeManager.MonitorFromWindow(Arg.Any<HWND>(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(HMONITOR_1);

		WindowFocusedTransform sut = new(null);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is not updated, and stays the same uninitialized value.
		Assert.Equal((HMONITOR)0, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)0, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ForegroundWindowIsNotNull(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given there is no window and GetForegroundWindow returns a handle
		Setup_Monitors(rootSector);

		HWND handle = (HWND)2;
		internalCtx.CoreNativeManager.GetForegroundWindow().Returns(handle);
		Setup_MonitorFromWindow(internalCtx, handle, HMONITOR_3);

		WindowFocusedTransform sut = new(null);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is updated, but the LastWhimActiveMonitorHandle doesn't' update because
		// the window isn't tracked.
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal((HMONITOR)0, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window is not tracked by the map sector
		IWindow window = CreateWindow((HWND)1);

		Setup_Monitors(rootSector);

		internalCtx
			.CoreNativeManager.MonitorFromWindow(window.Handle, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(HMONITOR_3);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		ctx.Store.Dispatch(sut);

		// Then the active monitor index is updated based on MonitorFromWindow
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceLaidOut(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window's workspace is tracked by the map sector
		IWindow window = CreateWindow((HWND)1);
		Workspace workspace1 = CreateWorkspace();
		Workspace workspace2 = CreateWorkspace();
		Workspace workspace3 = CreateWorkspace();
		Workspace workspace4 = CreateWorkspace();

		Setup_Monitors(rootSector);

		var monitors = rootSector.MonitorSector.Monitors;

		PopulateMonitorWorkspaceMap(rootSector, monitors[0], workspace1);
		PopulateMonitorWorkspaceMap(rootSector, monitors[1], workspace2);
		PopulateMonitorWorkspaceMap(rootSector, monitors[2], workspace3);
		PopulateWindowWorkspaceMap(rootSector, window, workspace4);

		Setup_MonitorFromWindow(internalCtx, window.Handle, HMONITOR_3);

		WindowFocusedTransform sut = new(window);

		// When we dispatch the transform
		CustomAssert.Layout(
			rootSector,
			() => ctx.Store.Dispatch(sut),
			layoutWorkspaceIds: [workspace4.Id],
			noLayoutWorkspaceIds: [workspace1.Id, workspace2.Id, workspace3.Id]
		);

		// Then the active monitor index is updated based on MonitorFromWindow		Assert.Equal(HMONITOR_1, rootSector.MonitorSector.ActiveMonitorHandle);
		Assert.Equal(HMONITOR_3, rootSector.MonitorSector.LastWhimActiveMonitorHandle);
	}
}
