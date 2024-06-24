using System;
using DotNext;

namespace Whim;

/// <summary>
/// Set the <see cref="IWorkspaceSector.CreateLayoutEngines"/>.
/// </summary>
/// <param name="CreateLayoutEnginesFn"></param>
public record SetCreateLayoutEnginesTransform(Func<CreateLeafLayoutEngine[]> CreateLayoutEnginesFn) : Transform<Unit>
{
	internal override Result<Unit> Execute(IContext ctx, IInternalContext internalCtx, MutableRootSector rootSector)
	{
		rootSector.WorkspaceSector.CreateLayoutEngines = CreateLayoutEnginesFn;
		return Unit.Result;
	}
}
