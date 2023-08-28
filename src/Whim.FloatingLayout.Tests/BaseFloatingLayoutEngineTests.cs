using Whim.TestUtils;

namespace Whim.FloatingLayout.Tests;

public class BaseFloatingLayoutEngineTests : ProxyLayoutEngineBaseTests
{
	public override Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine =>
		(inner) =>
		{
			Wrapper wrapper = new();
			return new FloatingLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, inner);
		};
}
