namespace Whim.Tests;

public class SetWorkspaceNameTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoChanges(IContext ctx, MutableRootSector root)
	{
		// Given the new name is the same as the old name
		Workspace workspace = CreateWorkspace() with
		{
			Name = "test",
		};
		AddWorkspaceToStore(root, workspace);

		SetWorkspaceNameTransform sut = new(workspace.Id, "test");

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get the same workspace
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);
		Assert.Same(workspace, root.WorkspaceSector.Workspaces[workspace.Id]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ChangeName(IContext ctx, MutableRootSector root)
	{
		// Given the new name is different from the old name
		Workspace workspace = CreateWorkspace() with
		{
			Name = "test",
		};
		AddWorkspaceToStore(root, workspace);

		SetWorkspaceNameTransform sut = new(workspace.Id, "test2");

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get a new workspace
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);
		Assert.NotSame(workspace, root.WorkspaceSector.Workspaces[workspace.Id]);
		Assert.Equal("test2", root.WorkspaceSector.Workspaces[workspace.Id].Name);
	}
}
