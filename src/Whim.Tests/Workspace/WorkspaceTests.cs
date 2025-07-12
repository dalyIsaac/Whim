namespace Whim.Tests;

public class WorkspaceTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Name_Get(IContext ctx, MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			Name = "Bob",
		};
		AddWorkspaceToManager(ctx, root, workspace);

		// When
		string name = workspace.Name;

		// Then
		Assert.Equal("Bob", name);
	}
}
