using Whim.TestUtils;

namespace Whim.Bar.Tests;

public class BaseBarLayoutEngineTests : ProxyLayoutEngineBaseTests
{
	public override Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine =>
		(inner) =>
		{
			BarConfig config =
				new(
					leftComponents: new List<BarComponent>(),
					centerComponents: new List<BarComponent>(),
					rightComponents: new List<BarComponent>()
				)
				{
					Height = 30
				};

			return new BarLayoutEngine(config, inner);
		};
}
