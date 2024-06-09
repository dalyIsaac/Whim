using System;
using NSubstitute;
using Whim.TestUtils;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class MutableRootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given
		MutableRootSector sut = new(ctx, internalCtx);
		var capture = CaptureWinEventProc.Create(internalCtx);

		// Create a workspace for the monitor created. This will avoid a KeyNotFoundException during Dispose.
		AddWorkspaceToManager(ctx, rootSector, CreateWorkspace(ctx));

		// When we initialize and dispose the root sector
		sut.Initialize();
		sut.Dispose();

		// Then
		internalCtx.WindowMessageMonitor.DisplayChanged += Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
		internalCtx.WindowMessageMonitor.DisplayChanged -= Arg.Any<EventHandler<WindowMessageMonitorEventArgs>>();
		Assert.Equal(6, capture.Handles.Count);
	}
}
