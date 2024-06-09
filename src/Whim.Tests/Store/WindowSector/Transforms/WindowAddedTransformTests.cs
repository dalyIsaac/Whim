using System.Collections.Generic;
using System.ComponentModel;
using DotNext;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WindowAddedTransformTests
{
	private static Result<IWindow> AssertDoesNotRaise(
		IContext ctx,
		MutableRootSector rootSector,
		WindowAddedTransform sut
	)
	{
		Result<IWindow>? result = null;
		CustomAssert.DoesNotRaise<RouteEventArgs>(
			h => rootSector.MapSector.WindowRouted += h,
			h => rootSector.MapSector.WindowRouted -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return result!.Value;
	}

	private static (Result<IWindow>, List<RouteEventArgs>) AssertRaises(
		IContext ctx,
		MutableRootSector rootSector,
		WindowAddedTransform sut
	)
	{
		Result<IWindow>? result = null;
		List<RouteEventArgs> evs = new();

		Assert.Raises<RouteEventArgs>(
			h =>
				rootSector.MapSector.WindowRouted += (sender, args) =>
				{
					evs.Add(args);
					h.Invoke(sender, args);
				},
			h => rootSector.MapSector.WindowRouted -= h,
			() =>
			{
				CustomAssert.DoesLayout(rootSector, () => result = ctx.Store.Dispatch(sut));
			}
		);

		return (result!.Value, evs);
	}

	private static void Setup(
		IContext ctx,
		IInternalContext internalCtx,
		HWND hwnd,
		bool isSplashScreen = false,
		bool isCloakedWindow = false,
		bool isNotStandardWindow = false,
		bool hasVisibleWindow = false,
		bool cannotCreateWindow = false,
		bool shouldBeIgnored = false
	)
	{
		internalCtx.CoreNativeManager.IsSplashScreen(hwnd).Returns(isSplashScreen);
		internalCtx.CoreNativeManager.IsCloakedWindow(hwnd).Returns(isCloakedWindow);
		internalCtx.CoreNativeManager.IsStandardWindow(hwnd).Returns(!isNotStandardWindow);
		internalCtx.CoreNativeManager.HasNoVisibleOwner(hwnd).Returns(!hasVisibleWindow);

		if (cannotCreateWindow)
		{
			internalCtx.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>()).Throws(new Win32Exception());
		}
		else
		{
			internalCtx
				.CoreNativeManager.GetProcessNameAndPath(Arg.Any<int>())
				.Returns(("processName", "processFileName"));
		}

		ctx.FilterManager.ShouldBeIgnored(Arg.Any<IWindow>()).Returns(shouldBeIgnored);
	}

	[InlineAutoSubstituteData<StoreCustomization>("IsSplashScreen", true, false, false, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("IsCloakedWindow", false, true, false, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("IsStandardWindow", false, false, true, false, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("HasNoVisibleWindow", false, false, false, true, false, false)]
	[InlineAutoSubstituteData<StoreCustomization>("CannotCreateWindow", false, false, false, false, true, false)]
	[InlineAutoSubstituteData<StoreCustomization>("ShouldBeIgnored", false, false, false, false, false, true)]
	[Theory]
	internal void Failure(
		string _,
		bool isSplashScreen,
		bool isCloakedWindow,
		bool isNotStandardWindow,
		bool hasNoVisibleWindow,
		bool cannotCreateWindow,
		bool shouldBeIgnored,
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given the handle fails
		HWND hwnd = (HWND)1;
		Setup(
			ctx,
			internalCtx,
			hwnd,
			isSplashScreen,
			isCloakedWindow,
			isNotStandardWindow,
			hasNoVisibleWindow,
			cannotCreateWindow,
			shouldBeIgnored
		);
		WindowAddedTransform sut = new(hwnd);

		// When we dispatch the transform
		var result = AssertDoesNotRaise(ctx, rootSector, sut);

		// Then we received an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_RoutedToWorkspace(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window is routed to a workspace
		HWND hwnd = (HWND)1;
		HMONITOR monitorHandle = (HMONITOR)1;
		Workspace workspace = CreateWorkspace(ctx);

		Setup(ctx, internalCtx, hwnd);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor(monitorHandle), workspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		WindowAddedTransform sut = new(hwnd);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_RoutedToActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given the window is routed to an active workspace
		HWND hwnd = (HWND)1;
		HMONITOR monitorHandle = (HMONITOR)1;
		Workspace workspace = CreateWorkspace(ctx);

		Setup(ctx, internalCtx, hwnd);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor(monitorHandle), workspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).ReturnsNull();

		WindowAddedTransform sut = new(hwnd, RouterOptions.RouteToActiveWorkspace);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_RoutedToLastTrackedActiveWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector
	)
	{
		// Given the window is routed to last tracked active workspace
		HWND hwnd = (HWND)1;
		HMONITOR monitorHandle = (HMONITOR)1;
		Workspace workspace = CreateWorkspace(ctx);

		Setup(ctx, internalCtx, hwnd);
		ctx.RouterManager.RouterOptions.Returns(RouterOptions.RouteToLastTrackedActiveWorkspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).ReturnsNull();

		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor(monitorHandle), workspace);
		rootSector.MonitorSector.LastWhimActiveMonitorHandle = monitorHandle;

		WindowAddedTransform sut = new(hwnd);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_WorkspaceFromWindow(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window has a workspace
		HWND hwnd = (HWND)1;
		HMONITOR monitorHandle = (HMONITOR)1;
		Workspace workspace = CreateWorkspace(ctx);

		Setup(ctx, internalCtx, hwnd);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).ReturnsNull();

		internalCtx
			.CoreNativeManager.MonitorFromWindow(hwnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			.Returns(monitorHandle);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor(monitorHandle), workspace);

		WindowAddedTransform sut = new(hwnd, RouterOptions.RouteToLaunchedWorkspace);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_ActiveWorkspace(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window is not routed, so we use the active workspace
		HWND hwnd = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx);

		Setup(ctx, internalCtx, hwnd);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).ReturnsNull();

		WindowAddedTransform sut = new(hwnd, RouterOptions.RouteToLaunchedWorkspace);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_WindowMinimized(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window is minimized
		HWND hwnd = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)true);

		Setup(ctx, internalCtx, hwnd);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		WindowAddedTransform sut = new(hwnd);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);

		Assert.Contains(((StoreWrapper)ctx.Store).Transforms, t => t is MinimizeWindowStartTransform);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_WindowAdded(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given the window is not minimized
		HWND hwnd = (HWND)1;
		Workspace workspace = CreateWorkspace(ctx);

		internalCtx.CoreNativeManager.IsWindowMinimized(hwnd).Returns((BOOL)false);

		Setup(ctx, internalCtx, hwnd);
		PopulateMonitorWorkspaceMap(ctx, rootSector, CreateMonitor((HMONITOR)1), workspace);
		ctx.RouterManager.RouteWindow(Arg.Any<IWindow>()).Returns(workspace);

		WindowAddedTransform sut = new(hwnd);

		// When
		var (result, evs) = AssertRaises(ctx, rootSector, sut);

		// Then
		Assert.True(result.IsSuccessful);

		Assert.Equal(workspace.Id, rootSector.MapSector.WindowWorkspaceMap[hwnd]);
		Assert.Single(evs);

		Assert.Contains(((StoreWrapper)ctx.Store).Transforms, t => t is AddWindowToWorkspaceTransform);
	}
}
