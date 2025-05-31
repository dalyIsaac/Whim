using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MergeWorkspaceWindowsTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SourceWorkspaceNotFound(IContext ctx)
	{
		// Given
		Guid sourceWorkspaceId = Guid.NewGuid();
		Guid targetWorkspaceId = Guid.NewGuid();

		MergeWorkspaceWindowsTransform sut = new(sourceWorkspaceId, targetWorkspaceId);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void TargetWorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx);
		Guid targetWorkspaceId = Guid.NewGuid();

		AddWorkspacesToManager(ctx, rootSector, workspace);

		MergeWorkspaceWindowsTransform sut = new(workspace.Id, targetWorkspaceId);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given the source and target workspaces
		Workspace sourceWorkspace = CreateWorkspace(ctx);
		Workspace targetWorkspace = CreateWorkspace(ctx);

		HMONITOR monitorHandle = (HMONITOR)1;
		IMonitor monitor = CreateMonitor(monitorHandle);

		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);
		IWindow window3 = CreateWindow((HWND)3);

		sourceWorkspace = PopulateWindowWorkspaceMap(ctx, rootSector, window1, sourceWorkspace);
		sourceWorkspace = PopulateWindowWorkspaceMap(ctx, rootSector, window2, sourceWorkspace);
		sourceWorkspace = PopulateWindowWorkspaceMap(ctx, rootSector, window3, sourceWorkspace);

		AddWorkspaceToManager(ctx, rootSector, targetWorkspace);

		PopulateMonitorWorkspaceMap(ctx, rootSector, monitor, sourceWorkspace);

		MergeWorkspaceWindowsTransform sut = new(sourceWorkspace.Id, targetWorkspace.Id);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window1.Handle]);
		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window2.Handle]);
		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.WindowWorkspaceMap[window3.Handle]);

		Assert.Equal(targetWorkspace.Id, rootSector.MapSector.MonitorWorkspaceMap[monitor.Handle]);

		Workspace resultTarget = rootSector.WorkspaceSector.Workspaces[targetWorkspace.Id];
		Assert.Equal(3, resultTarget.WindowPositions.Count);
		Assert.Contains(window1.Handle, resultTarget.WindowPositions.Keys);
		Assert.Contains(window2.Handle, resultTarget.WindowPositions.Keys);
		Assert.Contains(window3.Handle, resultTarget.WindowPositions.Keys);
	}
}
