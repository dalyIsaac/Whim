using System.Collections.Generic;
using FluentAssertions;

namespace Whim.Tests;

public class AddWorkspaceTransformTests
{
	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SectorNotInitialized(IContext ctx, MutableRootSector root)
	{
		// Given the workspace sector is not initialized
		root.WorkspaceSector.HasInitialized = false;
		string name = "Test";
		List<CreateLeafLayoutEngine> createLeafLayoutEngines = new() { (id) => Substitute.For<ILayoutEngine>() };

		AddWorkspaceTransform sut = new(name, createLeafLayoutEngines, MonitorIndices: [1, 2]);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		Assert.True(result.IsSuccessful);

		Assert.Single(root.WorkspaceSector.WorkspacesToCreate);

		WorkspaceToCreate workspaceToCreate = root.WorkspaceSector.WorkspacesToCreate[0];
		Assert.Equal(name, workspaceToCreate.Name);
		Assert.Equal(createLeafLayoutEngines, workspaceToCreate.CreateLeafLayoutEngines);
		workspaceToCreate.MonitorIndices.Should().BeEquivalentTo(new[] { 1, 2 });
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoEngineCreators(IContext ctx, MutableRootSector root)
	{
		// Given the workspace sector is initialized
		root.WorkspaceSector.HasInitialized = true;
		root.WorkspaceSector.CreateLayoutEngines = System.Array.Empty<CreateLeafLayoutEngine>;

		string name = "Test";

		AddWorkspaceTransform sut = new(name);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SectorInitialized_NoMonitorIndices(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the workspace sector is initialized
		root.WorkspaceSector.HasInitialized = true;
		string name = "Test";
		List<CreateLeafLayoutEngine> createLeafLayoutEngines = new() { (id) => engine1, (id) => engine2 };

		root.WorkspaceSector.ProxyLayoutEngineCreators =
		[
			(ILayoutEngine engine) => Substitute.For<TestProxyLayoutEngine>(engine),
			(engine) => Substitute.For<TestProxyLayoutEngine>(engine),
		];

		AddWorkspaceTransform sut = new(name, createLeafLayoutEngines);

		// When we execute the transform
		Result<Guid>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then we get the created workspace
		Assert.True(result!.Value.IsSuccessful);

		IWorkspace workspace = root.WorkspaceSector.Workspaces[result!.Value.Value];

		Assert.NotNull(workspace);
		Assert.Single(root.WorkspaceSector.Workspaces);
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);

		Assert.Empty(root.WorkspaceSector.WorkspacesToCreate);

		Assert.Equal(name, raisedEvent.Arguments.Workspace.BackingName);
		Assert.Same(raisedEvent.Arguments.Workspace, workspace);

		Assert.Equal(2, workspace.LayoutEngines.Count);

		// We actually create the proxy layout engines.
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[0]);
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[1]);

		Assert.Same(engine1, ((TestProxyLayoutEngine)workspace.LayoutEngines[0]).InnerLayoutEngine);
		Assert.Same(engine2, ((TestProxyLayoutEngine)workspace.LayoutEngines[1]).InnerLayoutEngine);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void SectorInitialized_MonitorIndices(
		IContext ctx,
		MutableRootSector root,
		ILayoutEngine engine1,
		ILayoutEngine engine2
	)
	{
		// Given the workspace sector is initialized
		root.WorkspaceSector.HasInitialized = true;
		string name = "Test";
		List<CreateLeafLayoutEngine> createLeafLayoutEngines = new() { (id) => engine1, (id) => engine2 };

		root.WorkspaceSector.ProxyLayoutEngineCreators =
		[
			(ILayoutEngine engine) => Substitute.For<TestProxyLayoutEngine>(engine),
			(engine) => Substitute.For<TestProxyLayoutEngine>(engine),
		];

		AddWorkspaceTransform sut = new(name, createLeafLayoutEngines, MonitorIndices: [1, 3]);

		// When we execute the transform
		Result<Guid>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then we get the created workspace
		Assert.True(result!.Value.IsSuccessful);

		IWorkspace workspace = root.WorkspaceSector.Workspaces[result!.Value.Value];

		Assert.NotNull(workspace);
		Assert.Single(root.WorkspaceSector.Workspaces);
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);

		Assert.Empty(root.WorkspaceSector.WorkspacesToCreate);

		Assert.Equal(name, raisedEvent.Arguments.Workspace.BackingName);
		Assert.Same(raisedEvent.Arguments.Workspace, workspace);

		Assert.Equal(2, workspace.LayoutEngines.Count);

		// We actually create the proxy layout engines.
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[0]);
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[1]);

		Assert.Same(engine1, ((TestProxyLayoutEngine)workspace.LayoutEngines[0]).InnerLayoutEngine);
		Assert.Same(engine2, ((TestProxyLayoutEngine)workspace.LayoutEngines[1]).InnerLayoutEngine);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Fails_Defaults(IContext ctx, MutableRootSector root)
	{
		// Given the workspace sector is initialized and there are no layout engines in the creator
		root.WorkspaceSector.HasInitialized = true;

		AddWorkspaceTransform sut = new();

		// When we execute the transform
		// Then we don't get a workspace created
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
		IEnumerable<CreateLeafLayoutEngine>? createLeafLayoutEngines,
		IEnumerable<int>? monitorIndices
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
		// Given a workspace to create with no name
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut =
			new(CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id, MonitorIndices: [1, 2]);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		AssertWorkspaceToCreate(root, result, id, null, createLeafLayoutEngines, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoCreateLeafLayoutEngines(IContext ctx, MutableRootSector root)
	{
		// Given a workspace to create with no layout engines
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new("Test", WorkspaceId: id, MonitorIndices: [1, 2]);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		AssertWorkspaceToCreate(root, result, id, "Test", null, [1, 2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoMonitorIndices(IContext ctx, MutableRootSector root)
	{
		// Given a workspace to create with no monitor indices
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new("Test", CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		AssertWorkspaceToCreate(root, result, id, "Test", createLeafLayoutEngines, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void AllDefaults(IContext ctx, MutableRootSector root)
	{
		// Given a workspace to create with all defaults
		AddWorkspaceTransform sut = new();

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		AssertWorkspaceToCreate(root, result, null, null, null, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void WorkspaceId(IContext ctx, MutableRootSector root)
	{
		// Given a workspace to create with a workspace ID
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut = new(WorkspaceId: id);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get the workspace ID we provided
		AssertWorkspaceToCreate(root, result, id, null, null, null);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Everything(IContext ctx, MutableRootSector root)
	{
		// Given the workspace sector is not initialized
		Guid id = Guid.NewGuid();
		AddWorkspaceTransform sut =
			new("Test", CreateLeafLayoutEngines: createLeafLayoutEngines, WorkspaceId: id, MonitorIndices: [1, 2]);

		// When we execute the transform
		Result<Guid> result = ctx.Store.Dispatch(sut);

		// Then we get the workspace ID we provided
		AssertWorkspaceToCreate(root, result, id, "Test", createLeafLayoutEngines, [1, 2]);
	}
}
