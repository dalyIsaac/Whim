using System;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToAdjacentWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoValidWindows(IContext ctx)
	{
		// Given there is no valid windows
		MoveWindowToAdjacentWorkspaceTransform sut = new();

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWindowForHandle(IContext ctx)
	{
		// Given there is no window for the handle
		MoveWindowToAdjacentWorkspaceTransform sut = new((HWND)10);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoWorkspaceForWindow(IContext ctx, MutableRootSector rootSector)
	{
		// Given there is no workspace for the window
		IWindow window = CreateWindow((HWND)10);
		AddWindowToSector(rootSector, window);

		MoveWindowToAdjacentWorkspaceTransform sut = new(window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given the workspace doesn't exist
		IWindow window = CreateWindow((HWND)10);
		AddWindowToSector(rootSector, window);

		rootSector.MapSector.WindowWorkspaceMap = rootSector.MapSector.WindowWorkspaceMap.SetItem(
			window.Handle,
			Guid.NewGuid()
		);

		MoveWindowToAdjacentWorkspaceTransform sut = new(window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoAdjacentWorkspaces(IContext ctx, MutableRootSector rootSector)
	{
		// Given there are no adjacent workspaces
		IWindow window = CreateWindow((HWND)10);
		PopulateWindowWorkspaceMap(ctx, rootSector, window, CreateWorkspace(ctx));

		MoveWindowToAdjacentWorkspaceTransform sut = new(window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector)
	{
		// Given
		IWindow window = CreateWindow((HWND)10);

		IWorkspace workspace1 = CreateWorkspace(ctx);
		IWorkspace workspace2 = CreateWorkspace(ctx);

		PopulateThreeWayMap(ctx, rootSector, CreateMonitor((HMONITOR)10), workspace1, window);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor((HMONITOR)11), workspace2);

		MoveWindowToAdjacentWorkspaceTransform sut = new(window.Handle);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.Equal(workspace2.Id, rootSector.MapSector.WindowWorkspaceMap[window.Handle]);

		workspace1.Received(1).RemoveWindow(window);
		workspace2.Received(1).AddWindow(window);

		workspace1.Received(1).DoLayout();
		workspace2.Received(1).DoLayout();

		window.Received(1).Focus();
	}
}
