using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class FocusWindowTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_WindowHandleDefined(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given a handle is defined for the transform, and we're not on the main thread
		HWND handle = (HWND)1;
		internalCtx.CoreNativeManager.IsStaThread().Returns(false);

		// When
		var result = ctx.Store.Dispatch(new FocusWindowTransform(handle));

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(handle, root.WorkspaceSector.WindowHandleToFocus);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_NoWindowHandleDefined(IContext ctx, MutableRootSector root, List<object> transforms)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		IMonitor monitor = CreateMonitor((HMONITOR)123);

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);

		// When
		var result = ctx.Store.Dispatch(new FocusWindowTransform());

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Contains(new FocusMonitorDesktopTransform(monitor.Handle), transforms);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_PickLastFocusedWindowHandle(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector root
	)
	{
		// Given a last focused window handle is defined for the workspace, and we're not on the main thread
		HWND handle = (HWND)123;
		Workspace workspace = CreateWorkspace(ctx) with { LastFocusedWindowHandle = handle };
		IMonitor monitor = CreateMonitor((HMONITOR)123);

		PopulateMonitorWorkspaceMap(ctx, root, monitor, workspace);

		internalCtx.CoreNativeManager.IsStaThread().Returns(false);

		// When
		var result = ctx.Store.Dispatch(new FocusWindowTransform());

		// Then
		Assert.True(result.IsSuccessful);
	}
}
