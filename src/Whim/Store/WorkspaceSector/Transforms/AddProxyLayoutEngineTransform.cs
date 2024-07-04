namespace Whim;

/// <summary>
/// Adds a proxy layout engine to the list of proxy layout engine creators.
/// This does not add the layout engine to any existing workspaces.
/// A proxy layout engine is used by plugins to add layout functionality to
/// all workspaces.
/// This should be used by <see cref="IPlugin"/>s.
/// </summary>
/// <param name="ProxyLayoutEngineCreator">The proxy layout engine creator to add.</param>
public record AddProxyLayoutEngineTransform(ProxyLayoutEngineCreator ProxyLayoutEngineCreator) : Transform<Unit>
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		rootSector.WorkspaceSector.ProxyLayoutEngineCreators = rootSector.WorkspaceSector.ProxyLayoutEngineCreators.Add(
			ProxyLayoutEngineCreator
		);
		return Unit.Result;
	}
}
