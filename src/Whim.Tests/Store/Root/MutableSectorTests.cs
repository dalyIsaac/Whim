using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class MutableRootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given
		MutableRootSector sut = new(ctx, internalCtx);
		var capture = CaptureWinEventProc.Create(internalCtx);

		ctx.Store.Dispatch(new AddWorkspaceTransform());

		// When we initialize and dispose the root sector
		sut.Initialize();
		sut.Dispose();

		// Then the monitor sector subscribes to the window message monitor
		internalCtx.WindowMessageMonitor.DisplayChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
		internalCtx.WindowMessageMonitor.DisplayChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();

		// and the window sector subscribes to SetWinEventHook
		Assert.Equal(6, capture.Handles.Count);

		// and a workspace is created
		Assert.Single(rootSector.WorkspaceSector.Workspaces);
		Assert.Single(rootSector.WorkspaceSector.WorkspaceOrder);
	}
}
