using System.Collections.Generic;
using System.Collections.Immutable;
using DotNext;
using NSubstitute;
using Whim.TestUtils;
using Xunit;

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

		AddWorkspaceTransform sut = new(name, createLeafLayoutEngines);

		// When we execute the transform
		Result<IWorkspace?> result = ctx.Store.Dispatch(sut);

		// Then we get null, and the workspace is added to the workspaces to create
		Assert.True(result.IsSuccessful);
		Assert.Null(result.Value);

		Assert.Single(root.WorkspaceSector.WorkspacesToCreate);
		Assert.Equal(name, root.WorkspaceSector.WorkspacesToCreate[0].Name);
		Assert.Equal(createLeafLayoutEngines, root.WorkspaceSector.WorkspacesToCreate[0].CreateLeafLayoutEngines); //./
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
		Result<IWorkspace?> result = ctx.Store.Dispatch(sut);

		// Then we get an error
		Assert.False(result.IsSuccessful);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success(IContext ctx, MutableRootSector root, ILayoutEngine engine1, ILayoutEngine engine2)
	{
		// Given the workspace sector is initialized
		root.WorkspaceSector.HasInitialized = true;
		string name = "Test";
		List<CreateLeafLayoutEngine> createLeafLayoutEngines = new() { (id) => engine1, (id) => engine2 };

		root.WorkspaceSector.ProxyLayoutEngineCreators = ImmutableList.Create<ProxyLayoutEngineCreator>(
			(engine) => Substitute.For<TestProxyLayoutEngine>(engine),
			(engine) => Substitute.For<TestProxyLayoutEngine>(engine)
		);

		AddWorkspaceTransform sut = new(name, createLeafLayoutEngines);

		// When we execute the transform
		Result<IWorkspace?>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then we get the created workspace
		Assert.True(result!.Value.IsSuccessful);

		IWorkspace? workspace = result!.Value.Value;
		Assert.NotNull(workspace);
		Assert.Single(root.WorkspaceSector.Workspaces);
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);

		Assert.Empty(root.WorkspaceSector.WorkspacesToCreate);

		Assert.Equal(name, raisedEvent.Arguments.Workspace.Name);
		Assert.Same(raisedEvent.Arguments.Workspace, workspace);

		Assert.Equal(2, workspace.LayoutEngines.Count);

		// We actually create the proxy layout engines.
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[0]);
		Assert.IsAssignableFrom<TestProxyLayoutEngine>(workspace.LayoutEngines[1]);

		Assert.Same(engine1, ((TestProxyLayoutEngine)workspace.LayoutEngines[0]).InnerLayoutEngine);
		Assert.Same(engine2, ((TestProxyLayoutEngine)workspace.LayoutEngines[1]).InnerLayoutEngine);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void Success_UseDefaults(IContext ctx, MutableRootSector root)
	{
		// Given the workspace sector is initialized
		root.WorkspaceSector.HasInitialized = true;

		AddWorkspaceTransform sut = new();

		// When we execute the transform
		Result<IWorkspace?>? result = null;
		var raisedEvent = Assert.Raises<WorkspaceAddedEventArgs>(
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded += h,
			h => ctx.Store.WorkspaceEvents.WorkspaceAdded -= h,
			() => result = ctx.Store.Dispatch(sut)
		);

		// Then we get the created workspace
		Assert.True(result!.Value.IsSuccessful);

		IWorkspace? workspace = result!.Value.Value;
		Assert.NotNull(workspace);
		Assert.Single(root.WorkspaceSector.Workspaces);
		Assert.Single(root.WorkspaceSector.WorkspaceOrder);

		Assert.Empty(root.WorkspaceSector.WorkspacesToCreate);

		Assert.Equal("Workspace 1", raisedEvent.Arguments.Workspace.Name);
		Assert.Same(raisedEvent.Arguments.Workspace, workspace);

		Assert.Single(workspace.LayoutEngines);
	}
}
