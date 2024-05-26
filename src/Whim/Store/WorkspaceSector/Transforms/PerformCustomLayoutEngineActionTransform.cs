using System.Collections.Immutable;
using DotNext;

namespace Whim;

/// <summary>
/// Performs a custom action in a layout engine.
/// </summary>
/// <remarks>
/// Layout engines need to handle the custom action in <see cref="ILayoutEngine.PerformCustomAction{T}" />.
/// For more, see <see cref="ILayoutEngine.PerformCustomAction{T}" />.
/// </remarks>
/// <param name="WorkspaceId"></param>
/// <param name="Action">
/// Metadata about the action to perform, and the payload to perform it with.
/// </param>
public record PerformCustomLayoutEngineActionTransform<T>(WorkspaceId WorkspaceId, LayoutEngineCustomAction<T> Action)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<ImmutableWorkspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		ImmutableWorkspace workspace
	)
	{
		bool hasChanged = false;
		ImmutableList<ILayoutEngine>.Builder newLayoutEngines = ImmutableList.CreateBuilder<ILayoutEngine>();

		foreach (ILayoutEngine layoutEngine in workspace.LayoutEngines)
		{
			ILayoutEngine newLayoutEngine = layoutEngine.PerformCustomAction(Action);

			if (newLayoutEngine.Equals(layoutEngine))
			{
				Logger.Debug($"Layout engine {layoutEngine} could not perform action {Action.Name}");
			}
			else
			{
				hasChanged = true;
			}

			newLayoutEngines.Add(layoutEngine);
		}

		return hasChanged ? workspace with { LayoutEngines = newLayoutEngines.ToImmutableList() } : workspace;
	}
}
