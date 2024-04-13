using System.Collections.Generic;

namespace Whim;

public record AddWorkspace(string? Name = null, IEnumerable<CreateLeafLayoutEngine>? CreateLayoutEngines = null)
	: Transform()
{
	public override void Execute(IRootSlice root)
	{
		// TODO
	}
}
