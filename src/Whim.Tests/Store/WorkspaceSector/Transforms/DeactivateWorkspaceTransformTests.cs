namespace Whim.Tests;

public class DeactivateWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector rootSector, IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		Workspace workspace = CreateWorkspace();

		workspace = PopulateWindowWorkspaceMap(rootSector, window1, workspace);
		workspace = PopulateWindowWorkspaceMap(rootSector, window2, workspace);
		workspace = PopulateWindowWorkspaceMap(rootSector, window3, workspace);

		DeactivateWorkspaceTransform sut = new(workspace.Id);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Workspace resultWorkspace = rootSector.WorkspaceSector.Workspaces[workspace.Id];
		Assert.True(result.IsSuccessful);

		ctx.NativeManager.Received(1).HideWindow(window1.Handle);
		ctx.NativeManager.Received(1).HideWindow(window2.Handle);
		ctx.NativeManager.Received(1).HideWindow(window3.Handle);

		Assert.Equal(
			new WindowPosition() with
			{
				WindowSize = WindowSize.Minimized,
			},
			resultWorkspace.WindowPositions[window1.Handle]
		);
		Assert.Equal(
			new WindowPosition() with
			{
				WindowSize = WindowSize.Minimized,
			},
			resultWorkspace.WindowPositions[window2.Handle]
		);
		Assert.Equal(
			new WindowPosition() with
			{
				WindowSize = WindowSize.Minimized,
			},
			resultWorkspace.WindowPositions[window3.Handle]
		);
	}
}
