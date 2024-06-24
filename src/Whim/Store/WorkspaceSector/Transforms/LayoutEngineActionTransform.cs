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
/// <typeparam name="T">
/// The type of <paramref name="PayloadAction" />'s payload.
/// </typeparam>
/// <param name="WorkspaceId"></param>
/// <param name="PayloadAction">
/// Metadata about the action to perform, and the payload to perform it with.
/// </param>
public record LayoutEngineActionWithPayloadTransform<T>(WorkspaceId WorkspaceId, LayoutEngineAction<T> PayloadAction)
	: BaseWorkspaceTransform(WorkspaceId)
{
	private protected override Result<Workspace> WorkspaceOperation(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector rootSector,
		Workspace workspace
	)
	{
		bool hasChanged = false;
		ImmutableList<ILayoutEngine>.Builder newLayoutEngines = ImmutableList.CreateBuilder<ILayoutEngine>();

		foreach (ILayoutEngine layoutEngine in workspace.LayoutEngines)
		{
			ILayoutEngine newLayoutEngine = layoutEngine.PerformCustomAction(PayloadAction);

			if (newLayoutEngine.Equals(layoutEngine))
			{
				Logger.Debug($"Layout engine {layoutEngine} could not perform action {PayloadAction.Name}");
			}
			else
			{
				hasChanged = true;
			}

			newLayoutEngines.Add(newLayoutEngine);
		}

		return hasChanged ? workspace with { LayoutEngines = newLayoutEngines.ToImmutableList() } : workspace;
	}
}

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
public record LayoutEngineActionTransform(WorkspaceId WorkspaceId, LayoutEngineAction Action)
	: LayoutEngineActionWithPayloadTransform<IWindow?>(
		WorkspaceId,
		new LayoutEngineAction<IWindow?>()
		{
			Name = Action.Name,
			Payload = Action.Window,
			Window = Action.Window
		}
	);
