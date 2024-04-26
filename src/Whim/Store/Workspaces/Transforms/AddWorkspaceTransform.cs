using System.Collections.Generic;
using System.Linq;
using DotNext;

namespace Whim;

/// <summary>
/// Create a workspace with the given <paramref name="Name"/> and layout engines.
/// </summary>
/// <param name="Name">
/// The name of the workspace. Defaults to <see langword="null"/>, which will generate the name
/// <c>Workspace {n}</c>.
/// </param>
/// <param name="CreateLeafLayoutEngines">
/// The layout engines to add to the workspace. Defaults to <see langword="null"/>, which will
/// use the <see cref="WorkspaceSlice.CreateLayoutEngines"/> function.
/// </param>
public record AddWorkspaceTransform(
	string? Name = null,
	IEnumerable<CreateLeafLayoutEngine>? CreateLeafLayoutEngines = null
) : Transform<ImmutableWorkspace>
{
	internal override Result<ImmutableWorkspace> Execute(IContext ctx, IInternalContext internalCtx)
	{
		WorkspaceSlice slice = ctx.Store.WorkspaceSlice;
		CreateLeafLayoutEngine[] engineCreators = CreateLeafLayoutEngines?.ToArray() ?? slice.CreateLayoutEngines();

		if (engineCreators.Length == 0)
		{
			return Result.FromException<ImmutableWorkspace>(new WhimException("No engine creators were provided"));
		}

		// Create the layout engines.
		ILayoutEngine[] layoutEngines = new ILayoutEngine[engineCreators.Length];
		for (int i = 0; i < engineCreators.Length; i++)
		{
			layoutEngines[i] = engineCreators[i](new LayoutEngineIdentity());
		}

		// Set up the proxies.
		for (int engineIdx = 0; engineIdx < engineCreators.Length; engineIdx++)
		{
			ILayoutEngine currentEngine = layoutEngines[engineIdx];
			foreach (CreateProxyLayoutEngine createProxyLayoutEngineFn in slice.ProxyLayoutEngines)
			{
				ILayoutEngine proxy = createProxyLayoutEngineFn(currentEngine);
				layoutEngines[engineIdx] = proxy;
				currentEngine = proxy;
			}
		}

		ImmutableWorkspace workspace = new(slice.IdCount, Name ?? $"Workspace {slice.Workspaces.Count + 1}");

		slice.IdCount++;
		slice.Workspaces = slice.Workspaces.Add(workspace);
		slice.MutableWorkspaces = slice.MutableWorkspaces.Add(new Workspace(ctx, internalCtx, workspace.Id));

		slice.QueueEvent(new WorkspaceAddedEventArgs() { Workspace = workspace });

		return Result.FromValue(workspace);
	}
}
