using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Whim.TestUtils;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests.WorkspaceSector;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class WorkspacePickersTests
{
	private static void CreateNamedWorkspaces(IContext ctx, MutableRootSector root)
	{
		AddWorkspacesToManager(
			ctx,
			root,
			CreateWorkspace(ctx) with
			{
				BackingName = "Test"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test2"
			},
			CreateWorkspace(ctx) with
			{
				BackingName = "Test3"
			}
		);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Success(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx), CreateWorkspace(ctx));

		Guid workspaceId = root.WorkspaceSector.WorkspaceOrder[0];

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[workspaceId], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceById_Failure(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx), CreateWorkspace(ctx));

		Guid workspaceId = Guid.NewGuid();

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceById(workspaceId));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickAllWorkspaces(IContext ctx, MutableRootSector root)
	{
		// Given there are three workspaces
		AddWorkspacesToManager(ctx, root, CreateWorkspace(ctx), CreateWorkspace(ctx), CreateWorkspace(ctx));

		// When we get the workspaces
		var result = ctx.Store.Pick(Pickers.PickAllWorkspaces()).ToArray();

		// Then we get the workspaces
		Assert.Equal(3, result.Length);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Success(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Test"));

		// Then we get the workspace
		Assert.True(result.IsSuccessful);
		Assert.Same(root.WorkspaceSector.Workspaces[root.WorkspaceSector.WorkspaceOrder[0]], result.Value);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PickWorkspaceByName_Failure(IContext ctx, MutableRootSector root)
	{
		// Given the workspaces
		CreateNamedWorkspaces(ctx, root);

		// When we get the workspace
		var result = ctx.Store.Pick(Pickers.PickWorkspaceByName("Bob the Builder"));

		// Then we don't succeed
		Assert.False(result.IsSuccessful);
	}
}
