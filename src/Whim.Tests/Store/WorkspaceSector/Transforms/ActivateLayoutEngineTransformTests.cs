using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SetLayoutEngineFromIndexTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_IndexMatches_ReturnsTrue(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is a layout engine at index 1
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = [engine, engine2],
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		SetLayoutEngineFromIndexTransform transform = new(workspace.Id, 1);

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then the transform succeeded
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_IndexDoesNotMatch_ReturnsFalse(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is no layout engine at index 2
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = [engine, engine2],
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		SetLayoutEngineFromIndexTransform transform = new(workspace.Id, 2);

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then the transform failed
		Assert.False(result.IsSuccessful);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class ActivatePreviouslyActiveLayoutEngineTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_ActiveEngineMatches_ReturnsTrue(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is a previously active layout engine
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = [engine, engine2],
			PreviousLayoutEngineIndex = 1,
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);
		
		ActivatePreviouslyActiveLayoutEngineTransform sut = new (workspace.Id);

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(sut);

		// Then the transform succeeded
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_ActiveEngineDoesNotMatch_ReturnsFalse(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is no previously active layout engine with a matching index
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = [engine, engine2],
			PreviousLayoutEngineIndex = 10
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		ActivatePreviouslyActiveLayoutEngineTransform transform = new(workspace.Id);

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then the transform failed
		Assert.False(result.IsSuccessful);
	}
}

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class SetLayoutEngineFromNameTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_NameMatches_ReturnsTrue(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is a layout engine with the specified name
		engine.Name.Returns("Engine 1");
		engine2.Name.Returns("Engine 2");

		Workspace workspace = CreateWorkspace(ctx) with { LayoutEngines = [engine, engine2] };

		AddWorkspaceToManager(ctx, rootSector, workspace);

		SetLayoutEngineFromNameTransform transform = new(workspace.Id, "Engine 1");

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then the transform succeeded
		Assert.True(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Predicate_NameDoesNotMatch_ReturnsFalse(
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine,
		ILayoutEngine engine2
	)
	{
		// Given there is no layout engine with the specified name
		engine.Name.Returns("Engine 1");
		engine2.Name.Returns("Engine 2");

		Workspace workspace = CreateWorkspace(ctx) with { LayoutEngines = [engine, engine2] };

		AddWorkspaceToManager(ctx, rootSector, workspace);

		SetLayoutEngineFromNameTransform transform = new(workspace.Id, "Engine 3");

		// When the transform is dispatched
		Result<bool> result = ctx.Store.Dispatch(transform);

		// Then the transform failed
		Assert.False(result.IsSuccessful);
	}
}
