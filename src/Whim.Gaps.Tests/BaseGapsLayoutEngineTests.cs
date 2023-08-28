using Whim.TestUtils;

namespace Whim.Gaps.Tests;

public class BaseGapsLayoutEngineTests : ProxyLayoutEngineBaseTests
{
	public override Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine =>
		(inner) => new GapsLayoutEngine(new GapsConfig { OuterGap = 10, InnerGap = 5 }, inner);
}
