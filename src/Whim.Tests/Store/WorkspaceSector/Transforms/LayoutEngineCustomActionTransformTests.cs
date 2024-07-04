using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class LayoutEngineCustomActionTransformTests
{
	private static readonly Guid WorkspaceId = Guid.NewGuid();

	private static ILayoutEngine CreateLayoutEngineNotSupportingAction<T>()
	{
		ILayoutEngine engine = Substitute.For<ILayoutEngine>();
		engine.PerformCustomAction(Arg.Any<LayoutEngineCustomAction<T>>()).Returns(engine);
		return engine;
	}

	private static ILayoutEngine CreateLayoutEngineSupportingAction<T>()
	{
		ILayoutEngine engine = Substitute.For<ILayoutEngine>();
		engine.PerformCustomAction(Arg.Any<LayoutEngineCustomAction<T>>()).Returns(Substitute.For<ILayoutEngine>());
		return engine;
	}

	private static void NoChanges(
		LayoutEngineCustomActionWithPayloadTransform<IWindow?> sut,
		IContext ctx,
		MutableRootSector rootSector,
		IWindow window
	)
	{
		// Given none of the layout engines support the action
		Workspace workspace = CreateWorkspace(ctx, WorkspaceId) with
		{
			LayoutEngines = ImmutableList.Create(
				CreateLayoutEngineNotSupportingAction<IWindow?>(),
				CreateLayoutEngineNotSupportingAction<IWindow?>()
			)
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		// When the action is performed
		var result = ctx.Store.Dispatch(sut);

		// Then the action is not performed
		Assert.True(result.IsSuccessful);
		Assert.False(result.Value);
	}

	private static void Changes(
		LayoutEngineCustomActionWithPayloadTransform<IWindow?> sut,
		IContext ctx,
		MutableRootSector root,
		IWindow window
	)
	{
		// Given the first and third layout engines support the action
		Workspace workspace = CreateWorkspace(ctx, WorkspaceId) with
		{
			LayoutEngines = ImmutableList.Create(
				CreateLayoutEngineSupportingAction<IWindow?>(),
				CreateLayoutEngineNotSupportingAction<IWindow?>(),
				CreateLayoutEngineSupportingAction<IWindow?>()
			)
		};
		AddWorkspaceToManager(ctx, root, workspace);

		// When the action is performed
		var result = ctx.Store.Dispatch(sut);

		// Then the action is performed
		Assert.True(result.IsSuccessful);
		Assert.True(result.Value);

		Workspace resultingWorkspace = root.WorkspaceSector.Workspaces[workspace.Id];
		Assert.Equal(3, resultingWorkspace.LayoutEngines.Count);
		Assert.NotSame(resultingWorkspace.LayoutEngines[0], workspace.LayoutEngines[0]);
		Assert.Same(resultingWorkspace.LayoutEngines[1], workspace.LayoutEngines[1]);
		Assert.NotSame(resultingWorkspace.LayoutEngines[2], workspace.LayoutEngines[2]);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PayloadAction_NoChanges(IContext ctx, MutableRootSector root, IWindow window)
	{
		LayoutEngineCustomActionWithPayloadTransform<IWindow?> sut =
			new(
				WorkspaceId,
				new LayoutEngineCustomAction<IWindow?>
				{
					Name = "Action",
					Payload = window,
					Window = window
				}
			);

		NoChanges(sut, ctx, root, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoPayload_NoChanges(IContext ctx, MutableRootSector root, IWindow window)
	{
		LayoutEngineCustomActionTransform sut =
			new(WorkspaceId, new LayoutEngineCustomAction { Name = "Action", Window = window });

		NoChanges(sut, ctx, root, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void PayloadAction_Changes(IContext ctx, MutableRootSector root, IWindow window)
	{
		LayoutEngineCustomActionWithPayloadTransform<IWindow?> sut =
			new(
				WorkspaceId,
				new LayoutEngineCustomAction<IWindow?>
				{
					Name = "Action",
					Payload = window,
					Window = window
				}
			);

		Changes(sut, ctx, root, window);
	}

	[Theory, AutoSubstituteData<StoreCustomization>]
	internal void NoPayload_Changes(IContext ctx, MutableRootSector root, IWindow window)
	{
		LayoutEngineCustomActionTransform sut =
			new(WorkspaceId, new LayoutEngineCustomAction { Name = "Action", Window = window });

		Changes(sut, ctx, root, window);
	}
}
