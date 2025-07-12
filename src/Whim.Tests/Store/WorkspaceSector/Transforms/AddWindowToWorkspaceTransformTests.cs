namespace Whim.Tests;

public class AddWindowToWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AddWindowToWorkspaceTransform_Success(IContext ctx, MutableRootSector root, ILayoutEngine engine)
	{
		// Given
		Workspace workspace = CreateWorkspace();
		workspace = workspace with { LayoutEngines = [engine] };

		AddWorkspaceToStore(root, workspace);

		AddWindowToWorkspaceTransform transform = new(workspace.Id, CreateWindow((HWND)1));

		// When
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Single(resultWorkspace.WindowPositions);
		Assert.Equal(new WindowPosition(), resultWorkspace.WindowPositions[transform.Window.Handle]);
	}
}
