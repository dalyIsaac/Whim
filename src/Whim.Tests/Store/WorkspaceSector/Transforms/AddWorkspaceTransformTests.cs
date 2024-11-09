using FluentAssertions;

namespace Whim.Tests;

public class AddWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoEngineCreators(IContext ctx, MutableRootSector root)
	{
		root.WorkspaceSector.HasInitialized = true;
		root.WorkspaceSector.CreateLayoutEngines = System.Array.Empty<CreateLeafLayoutEngine>;

		string name = "Test";

		AddWorkspaceTransform sut = new(name);

		Result<Guid> result = ctx.Store.Dispatch(sut);
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Fails_Defaults(IContext ctx, MutableRootSector root)
	{
		root.WorkspaceSector.HasInitialized = true;

		AddWorkspaceTransform sut = new();
		Result<Guid>? result = null;
		CustomAssert.DoesNotRaise<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
	}
}

public class AddWorkspaceTransformTests_NotInitialized
{
	private static void AssertWorkspaceToCreate(
		MutableRootSector root,
		Result<Guid> result,
		Guid? workspaceId,
		string? name,
		IReadOnlyList<CreateLeafLayoutEngine>? createLeafLayoutEngines,
		IReadOnlyList<int>? monitorIndices
	)
	{
		Assert.True(result.IsSuccessful);

		Assert.Single(root.WorkspaceSector.WorkspacesToCreate);

		WorkspaceToCreate workspaceToCreate = root.WorkspaceSector.WorkspacesToCreate[0];

		if (workspaceId != null)
		{
			Assert.Equal(workspaceId, result.Value);
			Assert.Equal(workspaceId, workspaceToCreate.WorkspaceId);
		}

		Assert.Equal(name, workspaceToCreate.Name);
		Assert.Equal(createLeafLayoutEngines, workspaceToCreate.CreateLeafLayoutEngines);
		workspaceToCreate.MonitorIndices.Should().BeEquivalentTo(monitorIndices);
	}

	private static readonly List<CreateLeafLayoutEngine> createLeafLayoutEngines =
		new() { (id) => Substitute.For<ILayoutEngine>() };

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoName(IContext ctx, MutableRootSector root)
	{
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut =
			new(CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id, MonitorIndices: [1, 2]);
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, id, null, createLeafLayoutEngines, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoCreateLeafLayoutEngines(IContext ctx, MutableRootSector root)
	{
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new("Test", WorkspaceId: id, MonitorIndices: [1, 2]);
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, id, "Test", null, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorIndices(IContext ctx, MutableRootSector root)
	{
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new("Test", CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id);
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, id, "Test", createLeafLayoutEngines, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AllDefaults(IContext ctx, MutableRootSector root)
	{
		AddWorkspaceTransform sut = new();
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, null, null, null, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceId(IContext ctx, MutableRootSector root)
	{
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new(WorkspaceId: id);
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, id, null, null, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Everything(IContext ctx, MutableRootSector root)
	{
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut =
			new("Test", CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id, MonitorIndices: [1, 2]);
		Result<Guid> result = ctx.Store.Dispatch(sut);
		AssertWorkspaceToCreate(root, result, id, "Test", createLeafLayoutEngines, [1, 2]);
	}
}

public class AddWorkspaceTransformTests_Initialized
{
	private static readonly List<CreateLeafLayoutEngine> sectorCreateLeafLayoutEngines =
		new() { (id) => Substitute.For<ILayoutEngine>() };
	private static readonly List<CreateLeafLayoutEngine> transformCreateLeafLayoutEngines =
		new() { (id) => Substitute.For<ILayoutEngine>(), (id) => Substitute.For<ILayoutEngine>() };

	private static void Setup_WorkspaceSector(MutableRootSector root)
	{
		root.WorkspaceSector.HasInitialized = true;

		root.WorkspaceSector.ProxyLayoutEngineCreators =
		[
			(ILayoutEngine engine) => Substitute.For<TestProxyLayoutEngine>(engine),
			(engine) => Substitute.For<TestProxyLayoutEngine>(engine),
		];

		root.WorkspaceSector.CreateLayoutEngines = () => [.. sectorCreateLeafLayoutEngines];
	}

	private static void AssertWorkspace(
		MutableRootSector root,
		Result<Guid> result,
		Guid? workspaceId,
		string name,
		int layoutEngineCount,
		IReadOnlyList<int>? monitorIndices
	)
	{
		Assert.True(result.IsSuccessful);
		Assert.Single(root.WorkspaceSector.Workspaces);

		Workspace workspace = root.WorkspaceSector.Workspaces[workspaceId ?? result.Value];

		Assert.NotNull(workspace);
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);

		Assert.Empty(root.WorkspaceSector.WorkspacesToCreate);

		Assert.Equal(name, workspace.BackingName);
		Assert.Equal(layoutEngineCount, workspace.LayoutEngines.Count);

		// We actually create the proxy layout engines.
		for (int idx = 0; idx < layoutEngineCount; idx++)
		{
			Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[idx]);
		}

		if (monitorIndices is null)
		{
			root.MapSector.StickyWorkspaceMonitorIndexMap.Should().BeEmpty();
		}
		else
		{
			root.MapSector.StickyWorkspaceMonitorIndexMap[result.Value].Should().BeEquivalentTo(monitorIndices);
		}
	}

	private static (Result<Guid>, WorkspaceAddedEventArgs) ExecuteTransform(IContext ctx, AddWorkspaceTransform sut)
	{
		Result<Guid>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);
		return (result!.Value, raisedEvent.Arguments);
	}

	private static void AssertEvent(WorkspaceAddedEventArgs args, Guid? workspaceId, string name, int layoutEngineCount)
	{
		if (workspaceId is not null)
		{
			Assert.Equal(workspaceId, args.Workspace.Id);
		}

		Assert.Equal(name, args.Workspace.BackingName);
		Assert.Equal(layoutEngineCount, args.Workspace.LayoutEngines.Count);

		for (int idx = 0; idx < layoutEngineCount; idx++)
		{
			Assert.IsAssignableFrom<TestProxyLayoutEngine>(args.Workspace.LayoutEngines[idx]);
		}
	}

	private static void AssertExecuteTransform(
		IContext ctx,
		AddWorkspaceTransform sut,
		MutableRootSector root,
		Guid? workspaceId,
		string name,
		int layoutEngineCount,
		IReadOnlyList<int>? monitorIndices
	)
	{
		(Result<Guid> result, WorkspaceAddedEventArgs eventArgs) = ExecuteTransform(ctx, sut);
		AssertWorkspace(root, result, workspaceId, name, layoutEngineCount, monitorIndices);
		AssertEvent(eventArgs, workspaceId, name, layoutEngineCount);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorIndices(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		AddWorkspaceTransform sut = new("Test", transformCreateLeafLayoutEngines);
		AssertExecuteTransform(ctx, sut, root, null, "Test", transformCreateLeafLayoutEngines.Count, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoName(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		AddWorkspaceTransform sut =
			new(CreateLeafLayoutEngines: transformCreateLeafLayoutEngines, MonitorIndices: [1, 2]);
		AssertExecuteTransform(ctx, sut, root, null, "Workspace 1", transformCreateLeafLayoutEngines.Count, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoCreateLeafLayoutEngines(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		AddWorkspaceTransform sut = new("Test", MonitorIndices: [1, 2]);
		AssertExecuteTransform(ctx, sut, root, null, "Test", sectorCreateLeafLayoutEngines.Count, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AllDefaults(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		AddWorkspaceTransform sut = new();
		AssertExecuteTransform(ctx, sut, root, null, "Workspace 1", sectorCreateLeafLayoutEngines.Count, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceId(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new(WorkspaceId: id);
		AssertExecuteTransform(ctx, sut, root, id, "Workspace 1", sectorCreateLeafLayoutEngines.Count, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Everything(IContext ctx, MutableRootSector root)
	{
		Setup_WorkspaceSector(root);
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new("Test", transformCreateLeafLayoutEngines, id, [1, 2]);
		AssertExecuteTransform(ctx, sut, root, id, "Test", transformCreateLeafLayoutEngines.Count, [1, 2]);
	}
}
