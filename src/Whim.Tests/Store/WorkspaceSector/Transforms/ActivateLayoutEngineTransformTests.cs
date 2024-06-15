using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DotNext;
using Whim.TestUtils;
using Xunit;
using static Whim.TestUtils.StoreTestUtils;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActivateLayoutEngineTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceNotFound(IContext ctx)
	{
		// Given the workspace doesn't exist
		ActivateLayoutEngineTransform sut = new(Guid.NewGuid(), (_, _) => true);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LayoutEngineNotFound(IContext ctx, MutableRootSector rootSector, ILayoutEngine engine)
	{
		// Given the layout engine doesn't exist
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(engine)
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		ActivateLayoutEngineTransform sut = new(workspace.Id, (_, _) => true);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we succeed, but the workspace doesn't change
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);
		Assert.Same(workspace, rootSector.WorkspaceSector.Workspaces[workspace.Id]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LayoutEngineFoundByEngine(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given the layout engine exists
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(engine, engine2)
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		ActivateLayoutEngineTransform sut = new(workspace.Id, (e, _) => e == engine2);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);
		Assert.NotSame(workspace, rootSector.WorkspaceSector.Workspaces[workspace.Id]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void LayoutEngineFoundByIndex(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given the layout engine exists
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(engine, engine2)
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		ActivateLayoutEngineTransform sut = new(workspace.Id, (_, i) => i == 1);

		// When we execute the transform
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then we succeed
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);
		Assert.NotSame(workspace, rootSector.WorkspaceSector.Workspaces[workspace.Id]);
	}
}
