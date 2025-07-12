namespace Whim.Tests;

public class RootSectorTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Initialize_Dispose(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		// Given
		RootSector sut = new(ctx, internalCtx);
		CaptureWinEventProc.Create(internalCtx);

		// Create a workspace for the monitor created. This will avoid a KeyNotFoundException during Dispose.
		AddWorkspaceToStore(rootSector, CreateWorkspace());

		// When
		sut.Initialize();
		sut.Dispose();

		// Then
		internalCtx.MouseHook.MouseLeftButtonUp += Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown += Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonUp -= Arg.Any<EventHandler<MouseEventArgs>>();
		internalCtx.MouseHook.MouseLeftButtonDown -= Arg.Any<EventHandler<MouseEventArgs>>();
	}
}
