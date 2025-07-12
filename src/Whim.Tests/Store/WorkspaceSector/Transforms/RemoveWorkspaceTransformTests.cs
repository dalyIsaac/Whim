namespace Whim.Tests;

public class RemoveWorkspaceTransformTests
{
	private static void NotEnoughWorkspaces(
		BaseRemoveWorkspaceTransform sut,
		Workspace providedWorkspace,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given there are less workspaces than monitors
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), CreateWorkspace());
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)2), providedWorkspace);

		// When we execute the transform
		Result<Unit> result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	private static void NoMatchingWorkspace(
		BaseRemoveWorkspaceTransform sut,
		Workspace providedWorkspace,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given there are no matching workspaces
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), CreateWorkspace() with { Name = "Test" });
		AddWorkspacesToStore(root, providedWorkspace);

		// When we execute the transform
		Result<Unit> result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	internal static void Success(
		BaseRemoveWorkspaceTransform sut,
		Workspace providedWorkspace,
		IContext ctx,
		MutableRootSector root
	)
	{
		// Given there is a matching workspace
		PopulateMonitorWorkspaceMap(root, CreateMonitor((HMONITOR)1), CreateWorkspace() with { Name = "Test" });
		AddWorkspacesToStore(root, providedWorkspace);

		// When we execute the transform
		Result<Unit>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceRemovedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceRemoved += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceRemoved -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then the workspace is removed
		Assert.True(result!.Value.IsSuccessful);
		Assert.False(root.WorkspaceSector.Workspaces.ContainsKey(providedWorkspace.Id));
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);
		Assert.Equal(providedWorkspace.Id, raisedEvent.Arguments.Workspace.Id);
	}

	#region ById
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ById_NotEnoughWorkspaces(IContext ctx, MutableRootSector root)
	{
		Guid workspaceId = Guid.NewGuid();
		Workspace workspace = CreateWorkspace(workspaceId);
		NotEnoughWorkspaces(new RemoveWorkspaceByIdTransform(workspaceId), workspace, ctx, root);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ById_NoMatchingWorkspace(IContext ctx, MutableRootSector root)
	{
		Guid workspaceId = Guid.NewGuid();
		Guid searchId = Guid.NewGuid();
		Workspace workspace = CreateWorkspace(workspaceId);
		NoMatchingWorkspace(new RemoveWorkspaceByIdTransform(searchId), workspace, ctx, root);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ById_Success(IContext ctx, MutableRootSector root)
	{
		Guid workspaceId = Guid.NewGuid();
		Workspace workspace = CreateWorkspace(workspaceId);
		Success(new RemoveWorkspaceByIdTransform(workspaceId), workspace, ctx, root);
	}
	#endregion

	#region ByName
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ByName_NotEnoughWorkspaces(IContext ctx, MutableRootSector root)
	{
		NotEnoughWorkspaces(
			new RemoveWorkspaceByNameTransform("Different test workspace"),
			CreateWorkspace(),
			ctx,
			root
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ByName_NoMatchingWorkspace(IContext ctx, MutableRootSector root)
	{
		NoMatchingWorkspace(
			new RemoveWorkspaceByNameTransform("Different test workspace"),
			CreateWorkspace(),
			ctx,
			root
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void ByName_Success(IContext ctx, MutableRootSector root)
	{
		string name = "Test";
		Success(new RemoveWorkspaceByNameTransform(name), CreateWorkspace() with { Name = name }, ctx, root);
	}
	#endregion
}
