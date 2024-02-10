using System.Collections.Generic;
using NSubstitute;
using Whim.TestUtils;
using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class ButlerChoresTests
{
	#region Activate
	// NOTE: The rest of the tests reside in WorkspaceManagerTests, until the obsolete code is removed.
	[Theory, AutoSubstituteData]
	internal void Activate_WorkspaceNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IWorkspace workspace
	)
	{
		// Given the workspace does not exist
		ButlerChores sut = new(ctx, internalCtx, triggers);

		// When Activate is called
		sut.Activate(workspace);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData]
	internal void Activate_MonitorNotFound(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the monitor does not exist
		ButlerChores sut = new(ctx, internalCtx, triggers);
		ctx.WorkspaceManager.GetEnumerator().Returns(new List<IWorkspace> { workspace }.GetEnumerator());

		// When Activate is called
		sut.Activate(workspace, monitor);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}

	[Theory, AutoSubstituteData]
	internal void Activate_WorkspaceAlreadyActive(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IWorkspace workspace,
		IMonitor monitor
	)
	{
		// Given the workspace is already activated
		ButlerChores sut = new(ctx, internalCtx, triggers);

		ctx.WorkspaceManager.GetEnumerator().Returns(new List<IWorkspace> { workspace }.GetEnumerator());
		ctx.MonitorManager.GetEnumerator().Returns(new List<IMonitor> { monitor }.GetEnumerator());
		ctx.Butler.Pantry.GetMonitorForWorkspace(workspace).Returns(monitor);

		// When Activate is called
		sut.Activate(workspace, monitor);

		// Then a layout is not called
		workspace.DidNotReceive().DoLayout();
	}
	#endregion

	[Theory, AutoSubstituteData]
	internal void FocusMonitorDesktop(
		IContext ctx,
		IInternalContext internalCtx,
		ButlerTriggers triggers,
		IMonitor monitor
	)
	{
		// Given
		ButlerChores sut = new(ctx, internalCtx, triggers);

		// When
		sut.FocusMonitorDesktop(monitor);

		// Then
		internalCtx.CoreNativeManager.Received(1).GetDesktopWindow();
		internalCtx.CoreNativeManager.Received(1).SetForegroundWindow(Arg.Any<HWND>());
		internalCtx.WindowManager.Received(1).OnWindowFocused(null);
		internalCtx.MonitorManager.Received(1).ActivateEmptyMonitor(monitor);
	}
}
