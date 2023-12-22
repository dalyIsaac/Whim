using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class PerformCustomActionTests
{
	[Theory, AutoSubstituteData]
	public void PerformCustomAction(string actionName, object args, IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(actionName, args, window);

		// Then
		Assert.Same(engine, newEngine);
	}
}
