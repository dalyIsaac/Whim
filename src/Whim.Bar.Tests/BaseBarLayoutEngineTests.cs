using Whim.TestUtils;

namespace Whim.Bar.Tests;

public class BaseBarLayoutEngineTests : ProxyLayoutEngineBaseTests
{
	public override Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine =>
		(inner) =>
		{
			BarConfig config = new(leftComponents: [], centerComponents: [], rightComponents: []) { Height = 30 };

			return new BarLayoutEngine(config, inner);
		};
}
