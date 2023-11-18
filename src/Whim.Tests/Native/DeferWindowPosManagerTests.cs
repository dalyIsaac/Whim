using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class DeferWindowPosManagerCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		fixture.Register(
			() =>
				new WindowPosState(
					new WindowState()
					{
						Location = new Location<int>(),
						Window = Substitute.For<IWindow>(),
						WindowSize = WindowSize.Normal
					}
				)
		);

		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		internalCtx.DeferWindowPosManager.ParallelOptions.Returns(new ParallelOptions { MaxDegreeOfParallelism = 1 });
	}
}

public class DeferWindowPosManagerTests
{
	private static DeferWindowPosManager CreateSut(IContext ctx, IInternalContext internalCtx)
	{
		DeferWindowPosManager manager = new(ctx, internalCtx);
		internalCtx.DeferWindowPosManager.Returns(manager);
		return manager;
	}

	[Theory, AutoSubstituteData]
	internal void RecoverLayout_NoDeferredWindows(IContext ctx, IInternalContext internalCtx)
	{
		// Given no windows are provided
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		// When RecoverLayout is called
		manager.RecoverLayout();

		// Then no calls to WorkspaceManager.GetWorkspaceForWindow are made
		ctx.WorkspaceManager.DidNotReceive().GetWorkspaceForWindow(Arg.Any<IWindow>());
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DeferWorkspaceLayout(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState,
		IWorkspace workspace
	)
	{
		// Given a window is provided, and a workspace is found for it
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState });

		ctx.WorkspaceManager.GetWorkspaceForWindow(windowPosState.WindowState.Window).Returns(workspace);

		// When RecoverLayout is called
		manager.RecoverLayout();

		// Then DoLayout is called on the workspace
		workspace.Received(1).DoLayout();
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DeferWorkspaceLayout_WorkspaceAlreadyDeferred(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState1,
		WindowPosState windowPosState2,
		IWorkspace workspace
	)
	{
		// Given two windows are provided for the same workspace
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState1, windowPosState2 });

		ctx.WorkspaceManager.GetWorkspaceForWindow(windowPosState1.WindowState.Window).Returns(workspace);
		ctx.WorkspaceManager.GetWorkspaceForWindow(windowPosState2.WindowState.Window).Returns(workspace);

		// When RecoverLayout is called
		manager.RecoverLayout();

		// Then DoLayout is called on the workspace once
		workspace.Received(1).DoLayout();
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DeferWindowLayout(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState
	)
	{
		// Given a window is provided, no workspace is found for the window, and the window is
		// ignored by the FilterManager
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState });

		ctx.WorkspaceManager.GetWorkspaceForWindow(windowPosState.WindowState.Window).Returns((IWorkspace?)null);
		ctx.FilterManager.ShouldBeIgnored(windowPosState.WindowState.Window).Returns(true);

		// When RecoverLayout is called
		manager.RecoverLayout();

		// Then the DeferWindowPosHandle is called
		ctx.MonitorManager.Received(1).GetEnumerator();
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DoNotLayoutWindow(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState
	)
	{
		// Given a window is provided, no workspace is found for the window, and the window is
		// not ignored by the FilterManager
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState });

		ctx.WorkspaceManager.GetWorkspaceForWindow(windowPosState.WindowState.Window).Returns((IWorkspace?)null);
		ctx.FilterManager.ShouldBeIgnored(windowPosState.WindowState.Window).Returns(false);

		// When RecoverLayout is called
		manager.RecoverLayout();

		// Then the DeferWindowPosHandle is not called
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
	}
}
