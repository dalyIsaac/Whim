using System.Collections.Generic;
using System.Linq;
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
		Dictionary<HWND, IWindowState> windowStates = new();
		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(false);
		workspace.Windows.Returns(new List<IWindow>() { Substitute.For<IWindow>() });

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers, windowStates);

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
		Dictionary<HWND, IWindowState> windowStates = new();
		internalCtx.CoreNativeManager.IsWindow(Arg.Any<HWND>()).Returns(true);
		internalCtx.WindowManager.HandleWindowMap.ContainsKey(Arg.Any<HWND>()).Returns(false);
		workspace.Windows.Returns(new List<IWindow>() { Substitute.For<IWindow>() });

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers, windowStates);

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
		Dictionary<HWND, IWindowState> windowStates = new();
		ctx.Butler.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(null as IMonitor);

		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers, windowStates);

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
		Dictionary<HWND, IWindowState> windowStatesDict = new() { { (HWND)3, Substitute.For<IWindowState>() }, };

		ctx.Butler.GetMonitorForWorkspace(Arg.Any<IWorkspace>()).Returns(monitor);

		window.Handle.Returns((HWND)1);
		window2.Handle.Returns((HWND)2);

		workspace.Windows.Returns(new List<IWindow>() { window, window2 });
		IWindowState[] windowStates = new IWindowState[]
		{
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = window,
				WindowSize = WindowSize.Normal
			},
			new WindowState()
			{
				Rectangle = new Rectangle<int>(),
				Window = window2,
				WindowSize = WindowSize.Normal
			},
		};
		workspace.ActiveLayoutEngine.DoLayout(Arg.Any<IRectangle<int>>(), Arg.Any<IMonitor>()).Returns(windowStates);
		DeferWorkspacePosManager sut = new(ctx, internalCtx);

		// When
		sut.DoLayout(workspace, triggers, windowStatesDict);

		// Then
		triggers.WorkspaceLayoutStarted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
		triggers.WorkspaceLayoutCompleted.Received(1).Invoke(Arg.Any<WorkspaceEventArgs>());
		ctx.NativeManager.Received(1).DeferWindowPos(Arg.Is<IEnumerable<WindowPosState>>(x => x.Count() == 2));

		Assert.DoesNotContain((HWND)3, windowStatesDict.Keys);
		Assert.Equal(2, windowStatesDict.Count);
		Assert.Contains((HWND)1, windowStatesDict.Keys);
		Assert.Contains((HWND)2, windowStatesDict.Keys);
	}
}
