namespace Whim.Tests;

public class WorkspaceTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Name_Get(MutableRootSector root)
	{
		// Given
		Workspace workspace = CreateWorkspace() with
		{
			Name = "Bob",
		};
		AddWorkspaceToStore(root, workspace);

		// When
		string name = workspace.Name;

		// Then
		Assert.Equal("Bob", name);
	}
}
