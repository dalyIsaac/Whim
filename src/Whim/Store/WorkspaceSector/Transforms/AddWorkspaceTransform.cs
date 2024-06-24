using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotNext;

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
public record AddWorkspaceTransform(
	string? Name = null,
	IEnumerable<CreateLeafLayoutEngine>? CreateLeafLayoutEngines = null
) : Transform<IWorkspace?>
{
	internal override Result<IWorkspace?> Execute(
		IContext ctx,
		IInternalContext internalCtx,
		MutableRootSector mutableRootSector
	)
	{
		WorkspaceSector sector = mutableRootSector.WorkspaceSector;

		if (!sector.HasInitialized)
		{
			sector.WorkspacesToCreate = sector.WorkspacesToCreate.Add(new(Name, CreateLeafLayoutEngines));
			return null;
		}

		CreateLeafLayoutEngine[] engineCreators = CreateLeafLayoutEngines?.ToArray() ?? sector.CreateLayoutEngines();

		if (engineCreators.Length == 0)
		{
			return Result.FromException<IWorkspace?>(new WhimException("No engine creators were provided"));
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

		Workspace workspace =
			new(ctx, WorkspaceId.NewGuid())
			{
				BackingName = Name ?? $"Workspace {sector.Workspaces.Count + 1}",
				LayoutEngines = layoutEnginesBuilder.ToImmutable()
			};
		sector.Workspaces = sector.Workspaces.Add(workspace.Id, workspace);
		sector.WorkspaceOrder = sector.WorkspaceOrder.Add(workspace.Id);
		sector.QueueEvent(new WorkspaceAddedEventArgs() { Workspace = workspace });

		return workspace;
	}
}
