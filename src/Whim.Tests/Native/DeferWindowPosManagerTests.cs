using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class DeferWindowPosManagerCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		fixture.Register(
			() =>
				new WindowPosState(
					new WindowState()
					{
						Rectangle = new Rectangle<int>(),
						Window = Substitute.For<IWindow>(),
						WindowSize = WindowSize.Normal
					}
				)
		);

		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();
		internalCtx.DeferWindowPosManager.ParallelOptions.Returns(new ParallelOptions { MaxDegreeOfParallelism = 1 });

		IContext ctx = fixture.Freeze<IContext>();
		Store store = new(ctx, internalCtx);
		ctx.Store.Returns(store);

		fixture.Inject(store._root);
		fixture.Inject(store._root.MutableRootSector);
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
		bool didPerformLayout = manager.RecoverLayout();

		// Then no layout is performed
		Assert.False(didPerformLayout);
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DeferWorkspaceLayout(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState,
		WindowPosState minimizedWindowPosState,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given a window is provided, and a workspace is found for it
		minimizedWindowPosState.WindowState.WindowSize = WindowSize.Minimized;
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState }, new() { minimizedWindowPosState });

		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector.Maps.WindowWorkspaceMap.Add(
			windowPosState.WindowState.Window,
			workspace
		);

		// When RecoverLayout is called
		bool didPerformLayout = manager.RecoverLayout();

		// Then DoLayout is called on the workspace
		workspace.Received(1).DoLayout();
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
		Assert.True(didPerformLayout);
	}

	[Theory, AutoSubstituteData<DeferWindowPosManagerCustomization>]
	internal void RecoverLayout_DeferWorkspaceLayout_WorkspaceAlreadyDeferred(
		IContext ctx,
		IInternalContext internalCtx,
		WindowPosState windowPosState1,
		WindowPosState windowPosState2,
		WindowPosState minimizedWindowPosState,
		IWorkspace workspace,
		MutableRootSector mutableRootSector
	)
	{
		// Given two windows are provided for the same workspace
		DeferWindowPosManager manager = CreateSut(ctx, internalCtx);
		manager.DeferLayout(new() { windowPosState1, windowPosState2 }, new() { minimizedWindowPosState });

		mutableRootSector.Maps.WindowWorkspaceMap = mutableRootSector
			.Maps.WindowWorkspaceMap.Add(windowPosState1.WindowState.Window, workspace)
			.Add(windowPosState2.WindowState.Window, workspace)
			.Add(minimizedWindowPosState.WindowState.Window, workspace);

		// When RecoverLayout is called
		bool didPerformLayout = manager.RecoverLayout();

		// Then DoLayout is called on the workspace once
		workspace.Received(1).DoLayout();
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
		Assert.True(didPerformLayout);
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
		manager.DeferLayout(new() { windowPosState }, new());

		ctx.FilterManager.ShouldBeIgnored(windowPosState.WindowState.Window).Returns(true);

		// When RecoverLayout is called
		bool didPerformLayout = manager.RecoverLayout();

		// Then the DeferWindowPosHandle is called
		ctx.MonitorManager.Received(1).GetEnumerator();
		Assert.True(didPerformLayout);
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
		manager.DeferLayout(new() { windowPosState }, new());

		ctx.FilterManager.ShouldBeIgnored(windowPosState.WindowState.Window).Returns(false);

		// When RecoverLayout is called
		bool didPerformLayout = manager.RecoverLayout();

		// Then the DeferWindowPosHandle is not called
		ctx.MonitorManager.DidNotReceive().GetEnumerator();
		Assert.True(didPerformLayout);
	}
}
