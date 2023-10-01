using Whim.TestUtils;

namespace Whim.TreeLayout.Tests;

public class BaseTests : LayoutEngineBaseTests
{
	public override Func<ILayoutEngine> CreateLayoutEngine =>
		() =>
		{
			LayoutEngineWrapper wrapper = new();
			return new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);
		};
}
