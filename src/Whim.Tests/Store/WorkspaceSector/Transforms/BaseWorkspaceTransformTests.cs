using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DotNext;
using Whim.TestUtils;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class BaseWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given the workspace doesn't exist
		FailedWorkspaceTransform sut = new(Guid.NewGuid());

		// When we execute the transform (outside of the store)
		var result = sut.Execute(ctx, internalCtx, root);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	private record FailedWorkspaceTransform(Guid WorkspaceId) : BaseWorkspaceTransform(WorkspaceId)
	{
		private protected override Result<Workspace> WorkspaceOperation(
			IContext ctx,
			IInternalContext internalCtx,
			MutableRootSector rootSector,
			Workspace workspace
		) => Result.FromException<Workspace>(new InvalidOperationException());
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void OperationFailed(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given the operation fails
		Workspace workspace = CreateWorkspace(ctx);
		AddWorkspaceToManager(ctx, root, workspace);

		FailedWorkspaceTransform sut = new(workspace.Id);

		// When we execute the transform (outside of the store)
		var result = sut.Execute(ctx, internalCtx, root);

		// Then we get an error
		Assert.False(result.IsSuccessful);
		Assert.Empty(root.WorkspaceSector.WorkspacesToLayout);
	}

	private record SameWorkspaceTransform(Guid WorkspaceId) : BaseWorkspaceTransform(WorkspaceId)
	{
		private protected override Result<Workspace> WorkspaceOperation(
			IContext ctx,
			IInternalContext internalCtx,
			MutableRootSector rootSector,
			Workspace workspace
		) => Result.FromValue(workspace);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SameWorkspace(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given the operation succeeds, but the returned workspace is the same
		Workspace workspace = CreateWorkspace(ctx);
		AddWorkspaceToManager(ctx, root, workspace);

		SameWorkspaceTransform sut = new(workspace.Id);

		// When we execute the transform (outside of the store)
		var result = sut.Execute(ctx, internalCtx, root);

		// Then we succeed, but the workspace hasn't changed
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);
		Assert.Empty(root.WorkspaceSector.WorkspacesToLayout);
	}

	private record SuccessfulWorkspaceTransform(Guid WorkspaceId, bool SkipDoLayout)
		: BaseWorkspaceTransform(WorkspaceId, SkipDoLayout)
	{
		private protected override Result<Workspace> WorkspaceOperation(
			IContext ctx,
			IInternalContext internalCtx,
			MutableRootSector rootSector,
			Workspace workspace
		) => workspace with { BackingName = "bob" };
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DifferentWorkspace_SkipDoLayout(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given the operation succeeds and the returned workspace is the same
		Workspace workspace = CreateWorkspace(ctx);
		AddWorkspaceToManager(ctx, root, workspace);

		SuccessfulWorkspaceTransform sut = new(workspace.Id, true);

		// When we execute the transform (outside of the store)
		var result = sut.Execute(ctx, internalCtx, root);

		// Then we succeed and the workspace has changed, but the workspaces to layout are empty
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);
		Assert.Empty(root.WorkspaceSector.WorkspacesToLayout);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void DifferentWorkspace_DoLayout(IContext ctx, IInternalContext internalCtx, MutableRootSector root)
	{
		// Given the operation succeeds and the returned workspace is the same
		Workspace workspace = CreateWorkspace(ctx);
		AddWorkspaceToManager(ctx, root, workspace);

		SuccessfulWorkspaceTransform sut = new(workspace.Id, false);

		// When we execute the transform (outside of the store)
		var result = sut.Execute(ctx, internalCtx, root);

		// Then we succeed and the workspace has changed and the workspaces to layout is not empty
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);
		Assert.Single(root.WorkspaceSector.WorkspacesToLayout);
		Assert.Equal(workspace.Id, root.WorkspaceSector.WorkspacesToLayout.First());
	}
}
