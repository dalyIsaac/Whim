using System;
using System.Collections.Generic;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MoveWindowToWorkspaceTransformTests
{
	private static Result<Unit> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		MoveWindowToWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		CustomAssert.DoesNotRaise<MonitorWorkspaceChangedEventArgs>(
			h => rootSector.MapSector.MonitorWorkspaceChanged += h,
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static (Result<Unit>, List<MonitorWorkspaceChangedEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector rootSector,
		MoveWindowToWorkspaceTransform sut
	)
	{
		Result<Unit>? result = null;
		List<MonitorWorkspaceChangedEventArgs> evs = new();

		Assert.Raises<MonitorWorkspaceChangedEventArgs>(
			h =>
				rootSector.MapSector.MonitorWorkspaceChanged += (sender, args) =>
				{
					evs.Add(args);
					h.Invoke(sender, args);
				},
			h => rootSector.MapSector.MonitorWorkspaceChanged -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		return (result!.Value, evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random workspace id
		Guid workspaceId = Guid.NewGuid();

		MoveWindowToWorkspaceTransform sut = new(workspaceId);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound_SpecifiedHandle(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random window id
		IWorkspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		AddWorkspaceToManager(ctx, workspace);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowNotFound_Default(IContext ctx, MutableRootSector rootSector)
	{
		// Given a random window id
		IWorkspace workspace = CreateWorkspace();

		AddWorkspaceToManager(ctx, workspace);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceForWindowNotFound(IContext ctx, MutableRootSector rootSector)
	{
		// Given a window which isn't in a workspace
		IWorkspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		AddWorkspaceToManager(ctx, workspace);
		rootSector.WindowSector.Windows = rootSector.WindowSector.Windows.Add(window.Handle, window);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WindowAlreadyOnWorkspace(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window is already on the workspace
		IWorkspace workspace = CreateWorkspace();
		IWindow window = CreateWindow((HWND)10);

		PopulateThreeWayMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace, window);

		MoveWindowToWorkspaceTransform sut = new(workspace.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we get an error
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_BothWorkspacesLayout(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window switches workspaces, and both workspaces are visible
		IWorkspace workspace1 = CreateWorkspace();
		IWorkspace workspace2 = CreateWorkspace();

		IWindow window = CreateWindow((HWND)10);

		PopulateThreeWayMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace1, window);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor((HMONITOR)2), workspace2);

		MoveWindowToWorkspaceTransform sut = new(workspace2.Id, window.Handle);

		// When we move the window to the workspace
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);
		workspace1.Received().RemoveWindow(window);
		workspace2.Received().AddWindow(window);

		workspace1.Received().DoLayout();
		workspace2.Received().DoLayout();

		window.Received().Focus();
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_SingleWorkspaceLayout(IContext ctx, MutableRootSector rootSector)
	{
		// Given the window gets activated on a hidden workspace
		IWorkspace workspace1 = CreateWorkspace();
		IWorkspace workspace2 = CreateWorkspace();

		IWindow window = CreateWindow((HWND)10);

		IMonitor monitor = CreateMonitor((HMONITOR)1);

		PopulateThreeWayMap(ctx, rootSector, monitor, workspace1, window);
		AddWorkspaceToManager(ctx, workspace2);
		rootSector.MonitorSector.ActiveMonitorHandle = monitor.Handle;

		MoveWindowToWorkspaceTransform sut = new(workspace2.Id, window.Handle);

		// When we move the window to the workspace
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);

		workspace1.Received().RemoveWindow(window);
		workspace2.Received().AddWindow(window);

		Assert.Single(evs);

		window.Received().Focus();
	}
}
