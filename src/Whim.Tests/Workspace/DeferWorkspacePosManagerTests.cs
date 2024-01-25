using System.Collections.Generic;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class DeferWorkspacePosManagerTests
{
	[Theory, AutoSubstituteData]
	internal void GarbageCollect_NotAWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		IWorkspace workspace
	)
	{
		// Given the window is not a valid window
		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(false);
		workspace.Windows.Returns(new List<IWindow>() { Substitute.For<IWindow>() });

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers);

		// Then
		internalCtx.WindowManager.Received(1).OnWindowRemoved(Arg.Any<IWindow>());
		ctx.Butler.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
	}

	[Theory, AutoSubstituteData]
	internal void GarbageCollect_NotTrackedWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		IWorkspace workspace
	)
	{
		// Given the window is not tracked by the WindowManager
		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);
		internalCtx.WindowManager.HandleWindowMap.ContainsKey(Arg.Any<HWND>()).Returns(false);
		workspace.Windows.Returns(new List<IWindow>() { Substitute.For<IWindow>() });

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers);

		// Then
		internalCtx.WindowManager.Received(1).OnWindowRemoved(Arg.Any<IWindow>());
		ctx.Butler.DidNotReceive().GetMonitorForWorkspace(Arg.Any<IWorkspace>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void DoLayout_CannotFindMonitorForWorkspace(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		IWorkspace workspace
	)
	{
		// Given the workspace has no monitor
		ctx.Butler.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers);

		// Then
		triggers.WorkspaceLayoutStarted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.DidNotReceive().Invoke(Arg.Any<WorkspaceEventArgs>());
	}

	[Theory, AutoSubstituteData<WorkspaceCustomization>]
	internal void DoLayout_Success(
		IContext ctx,
		IInternalContext internalCtx,
		WorkspaceManagerTriggers triggers,
		IWorkspace workspace,
		IMonitor monitor,
		IWindow window,
		IWindow window2
	)
	{
		// Given the workspace has a monitor
		ctx.Butler.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(monitor);
		workspace.Windows.Returns(new List<IWindow>() { window, window2 });

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers);

		// Then
		triggers.WorkspaceLayoutStarted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
	}
}
