using System;
using Whim.TestUtils;

namespace Whim.Tests;

public class BaseColumnLayoutEngineTests : LayoutEngineBaseTests
{
	private static readonly LayoutEngineIdentity identity = new();

	public override Func<ILayoutEngine> CreateLayoutEngine => () => new ColumnLayoutEngine(identity);
}
