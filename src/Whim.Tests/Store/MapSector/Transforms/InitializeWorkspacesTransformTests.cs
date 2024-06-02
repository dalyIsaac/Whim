// using System;
// using System.Collections.Generic;
// using System.Linq;
// using DotNext;
// using NSubstitute;
// using Whim.TestUtils;
// using Windows.Win32.Foundation;
// using Xunit;
// using static Whim.TestUtils.StoreTestUtils;
//
// namespace Whim.Tests;
//
// [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
// public class InitializeWorkspacesTransformTests
// {
// 	private static Result<Unit> AssertDoesNotRaise(
// 		IContext ctx,
// 		MutableRootSector rootSector,
// 		InitializeWorkspacesTransform sut
// 	)
// 	{
// 		Result<Unit>? result = null;
// 		CustomAssert.DoesNotRaise<WindowAddedEventArgs>(
// 			h => rootSector.WindowSector.WindowAdded += h,
// 			h => rootSector.WindowSector.WindowAdded -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
// 		return result!.Value;
// 	}
//
// 	private static (Result<Unit>, List<WindowAddedEventArgs>) AssertRaises(
// 		IContext ctx,
// 		MutableRootSector rootSector,
// 		InitializeWorkspacesTransform sut
// 	)
// 	{
// 		Result<Unit>? result = null;
// 		List<WindowAddedEventArgs> evs = new();
// 		Assert.Raises<WindowAddedEventArgs>(
// 			h =>
// 				rootSector.WindowSector.WindowAdded += (sender, args) =>
// 				{
// 					evs.Add(args);
// 					h.Invoke(sender, args);
// 				},
// 			h => rootSector.WindowSector.WindowAdded -= h,
// 			() => result = ctx.Store.Dispatch(sut)
// 		);
// 		return (result!.Value, evs);
// 	}
//
// 	private static void AddWorkspacesToSavedState(IInternalContext internalCtx, params SavedWorkspace[] workspaces)
// 	{
// 		internalCtx.CoreSavedStateManager.SavedState.Returns(new CoreSavedState(workspaces.ToList()));
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void NoSavedWorkspaces(IContext ctx, MutableRootSector rootSector)
// 	{
// 		// Given there are no saved workspaces
// 		InitializeWorkspacesTransform sut = new();
//
// 		// When
// 		var result = AssertDoesNotRaise(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void CouldNotFindWorkspace(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
// 	{
// 		// Given there are saved workspaces which don't exist in the workspace manager
// 		SavedWorkspace workspace = new("test", new List<SavedWindow>());
// 		AddWorkspacesToSavedState(internalCtx, workspace);
//
// 		InitializeWorkspacesTransform sut = new();
//
// 		// When the map transform is dispatched
// 		var result = AssertDoesNotRaise(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void CouldNotFindWindowFromHandle(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
// 	{
// 		// Given there are saved workspaces which don't exist in the workspace manager
// 		IWindow window = CreateWindow((HWND)10);
// 		SavedWorkspace workspace =
// 			new("test", new List<SavedWindow>() { new(window.Handle, Rectangle.UnitSquare<double>()), });
// 		AddWorkspacesToSavedState(internalCtx, workspace);
//
// 		ctx.WindowManager.CreateWindow(window.Handle).Returns(Result.FromException<IWindow>(new Exception("nope")));
//
// 		InitializeWorkspacesTransform sut = new();
//
// 		// When the map transform is dispatched
// 		var result = AssertDoesNotRaise(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
// 		Assert.Empty(rootSector.MapSector.WindowWorkspaceMap);
// 	}
//
// 	[Theory, AutoSubstituteData<StoreCustomization>]
// 	internal void Success(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
// 	{
// 		// Given there are saved workspaces which exist in the workspace manager
// 		IWindow window1 = CreateWindow((HWND)10);
// 		IWindow window2 = CreateWindow((HWND)20);
// 		IWindow window3 = CreateWindow((HWND)30);
// 		IWindow notSavedWindow = CreateWindow((HWND)40);
//
// 		SavedWindow savedWindow1 = new(window1.Handle, Rectangle.UnitSquare<double>());
// 		SavedWindow savedWindow2 = new(window2.Handle, Rectangle.UnitSquare<double>());
// 		SavedWindow savedWindow3 = new(window3.Handle, Rectangle.UnitSquare<double>());
//
// 		SavedWorkspace savedWorkspace1 = new("test1", new List<SavedWindow>() { savedWindow1, savedWindow2 });
// 		SavedWorkspace savedWorkspace2 = new("test2", new List<SavedWindow>() { savedWindow3 });
//
// 		Workspace workspace1 = CreateWorkspace(ctx);
// 		workspace1.Name.Returns(savedWorkspace1.Name);
// 		Workspace workspace2 = CreateWorkspace(ctx);
// 		workspace2.Name.Returns(savedWorkspace2.Name);
//
// 		AddWorkspacesToSavedState(internalCtx, savedWorkspace1, savedWorkspace2);
// 		AddWorkspacesToManager(ctx, rootSector, workspace1, workspace2);
//
// 		ctx.WorkspaceManager.ActiveWorkspace.Returns(workspace1);
// 		ctx.WorkspaceManager.TryGet(savedWorkspace1.Name).Returns(workspace1);
// 		ctx.WorkspaceManager.TryGet(savedWorkspace2.Name).Returns(workspace2);
//
// 		ctx.WindowManager.CreateWindow(window1.Handle).Returns(Result.FromValue(window1));
// 		ctx.WindowManager.CreateWindow(window2.Handle).Returns(Result.FromValue(window2));
// 		ctx.WindowManager.CreateWindow(window3.Handle).Returns(Result.FromValue(window3));
// 		ctx.WindowManager.CreateWindow(notSavedWindow.Handle)
// 			.Returns(Result.FromException<IWindow>(new Exception("nope")));
//
// 		List<HWND> allWindows = new() { window1.Handle, window2.Handle, window3.Handle, notSavedWindow.Handle };
// 		internalCtx.CoreNativeManager.GetAllWindows().Returns(allWindows);
//
// 		internalCtx.CoreNativeManager.IsStandardWindow(Arg.Any<HWND>()).Returns(true);
// 		internalCtx.CoreNativeManager.HasNoVisibleOwner(Arg.Any<HWND>()).Returns(true);
//
// 		InitializeWorkspacesTransform sut = new();
//
// 		// When the map transform is dispatched
// 		var (result, evs) = AssertRaises(ctx, rootSector, sut);
//
// 		// Then
// 		Assert.True(result.IsSuccessful);
//
// 		Assert.Equal(4, evs.Count);
// 		Assert.Equal(4, rootSector.WindowSector.Windows.Count);
// 		Assert.Equal(4, rootSector.MapSector.WindowWorkspaceMap.Count);
//
// 		Assert.Equal(workspace1.Id, rootSector.MapSector.WindowWorkspaceMap[window1.Handle]);
// 		Assert.Equal(workspace1.Id, rootSector.MapSector.WindowWorkspaceMap[window2.Handle]);
// 		Assert.Equal(workspace1.Id, rootSector.MapSector.WindowWorkspaceMap[notSavedWindow.Handle]);
// 		Assert.Equal(workspace2.Id, rootSector.MapSector.WindowWorkspaceMap[window3.Handle]);
// 	}
// }
