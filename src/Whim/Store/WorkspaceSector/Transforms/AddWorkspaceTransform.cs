using System.Linq;

namespace Whim;

/// <summary>
/// Create a workspace with the given <paramref name="Name"/> and layout engines.
/// If this transform is called prior to initialization, then <see cref="IWorkspace"/> creation is deferred.
/// <see langword="null"/> is returned by the transform if the workspace creation was deferred.
/// </summary>
/// <param name="Name">
/// The name of the workspace. Defaults to <see langword="null"/>, which will generate the name
/// <c>Workspace {n}</c>.
/// </param>
/// <param name="CreateLeafLayoutEngines">
/// The layout engines to add to the workspace. Defaults to <see langword="null"/>, which will
/// use the <see cref="WorkspaceSector.CreateLayoutEngines"/> function.
/// </param>
/// <param name="WorkspaceId">
/// The ID of the workspace. Defaults to <see cref="WorkspaceId.NewGuid"/>.
/// </param>
/// <param name="MonitorIndices">
/// The indices of the monitors the workspace is allowed to be on. Defaults to <see langword="null"/>,
/// which will allow the workspace to be on all monitors.
/// </param>
public record AddWorkspaceTransform(
	string? Name = null,
	IEnumerable<CreateLeafLayoutEngine>? CreateLeafLayoutEngines = null,
	WorkspaceId WorkspaceId = default,
	IEnumerable<int>? MonitorIndices = null
) : Transform<WorkspaceId>
{
	internal override Result<WorkspaceId> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.WorkspaceSector;
		WorkspaceId id = WorkspaceId == default ? WorkspaceId.NewGuid() : WorkspaceId;

		if (!sector.HasInitialized)
		{
			sector.WorkspacesToCreate = sector.WorkspacesToCreate.Add(
				new(id, Name, CreateLeafLayoutEngines, MonitorIndices)
			);
			return id;
		}

		CreateLeafLayoutEngine[] engineCreators = CreateLeafLayoutEngines?.ToArray() ?? sector.CreateLayoutEngines();

		if (engineCreators.Length == 0)
		{
			return Result.FromError<WorkspaceId>(new WhimError("No engine creators were provided"));
		}

		// Create the layout engines.
		ImmutableList<ILayoutEngine>.Builder layoutEnginesBuilder = ImmutableList.CreateBuilder<ILayoutEngine>();
		for (int i = 0; i < engineCreators.Length; i++)
		{
			ILayoutEngine currentEngine = engineCreators[i](new LayoutEngineIdentity());

			// Set up the proxies.
			foreach (ProxyLayoutEngineCreator createProxyLayoutEngineFn in sector.ProxyLayoutEngineCreators)
			{
				ILayoutEngine proxy = createProxyLayoutEngineFn(currentEngine);
				currentEngine = proxy;
			}

			layoutEnginesBuilder.Add(currentEngine);
		}

		Workspace workspace = new(ctx, id)
		{
			BackingName = Name ?? $"Workspace {sector.Workspaces.Count + 1}",
			LayoutEngines = layoutEnginesBuilder.ToImmutable(),
		};
		sector.Workspaces = sector.Workspaces.Add(workspace.Id, workspace);
		sector.WorkspaceOrder = sector.WorkspaceOrder.Add(workspace.Id);
		sector.QueueEvent(new WorkspaceAddedEventArgs() { Workspace = workspace });

		// If the monitor indices are provided, add them to the sticky workspace monitor index map.
		if (MonitorIndices != null)
		{
			mutableRootSector.MapSector.StickyWorkspaceMonitorIndexMap =
				mutableRootSector.MapSector.StickyWorkspaceMonitorIndexMap.Add(
					workspace.Id,
					[.. MonitorIndices]
				);
		}

		return id;
	}
}
