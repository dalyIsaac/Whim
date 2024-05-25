using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MergeWorkspaceWIndowsTransformTests
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
	internal void TargetWorkspaceNotFound(IContext ctx)
	{
		// Given
		IWorkspace workspace = CreateWorkspace();
		Guid targetWorkspaceId = Guid.NewGuid();

		AddWorkspacesToManager(ctx, workspace);

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
		IWorkspace sourceWorkspace = CreateWorkspace();
		IWorkspace targetWorkspace = CreateWorkspace();

		HMONITOR monitorHandle = (HMONITOR)1;
		IMonitor monitor = CreateMonitor(monitorHandle);

		IWindow window1 = CreateWindow((HWND)1);
		IWindow window2 = CreateWindow((HWND)2);
		IWindow window3 = CreateWindow((HWND)3);

		sourceWorkspace.Windows.Returns(new List<IWindow>() { window1, window2, window3 });

		AddWorkspacesToManager(ctx, sourceWorkspace, targetWorkspace);

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

		targetWorkspace.Received(1).AddWindow(window1);
		targetWorkspace.Received(1).AddWindow(window2);
		targetWorkspace.Received(1).AddWindow(window3);
	}
}
