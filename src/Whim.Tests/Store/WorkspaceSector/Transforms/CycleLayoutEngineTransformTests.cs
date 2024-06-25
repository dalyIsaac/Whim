using System.Diagnostics.CodeAnalysis;

namespace Whim.Tests;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope")]
public class CycleLayoutEngineTransformTests
{
	[Theory]
	[InlineAutoSubstituteData<StoreCustomization>(false, 0, 1)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 2, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(false, 1, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 0, 2)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 1, 0)]
	[InlineAutoSubstituteData<StoreCustomization>(true, 2, 1)]
	internal void Success(
		bool reverse,
		int startIdx,
		int expectedIdx,
		IContext ctx,
		MutableRootSector rootSector,
		ILayoutEngine engine1,
		ILayoutEngine engine2,
		ILayoutEngine engine3
	)
	{
		// Given
		Workspace workspace = CreateWorkspace(ctx) with
		{
			LayoutEngines = ImmutableList.Create(engine1, engine2, engine3),
			ActiveLayoutEngineIndex = startIdx
		};
		AddWorkspaceToManager(ctx, rootSector, workspace);

		CycleLayoutEngineTransform sut = new(workspace.Id, reverse);

		// When
		var result = ctx.Store.Dispatch(sut);

		// Then
		Workspace resultWorkspace = rootSector.WorkspaceSector.Workspaces[workspace.Id];

		Assert.True(result.IsSuccessful);
		Assert.Equal(expectedIdx, resultWorkspace.ActiveLayoutEngineIndex);
	}
}
