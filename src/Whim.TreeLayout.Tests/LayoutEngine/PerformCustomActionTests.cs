using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class PerformCustomActionTests
{
	[Theory, AutoSubstituteData]
	public void PerformCustomAction(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);
		LayoutEngineAction<string> action =
			new()
			{
				Name = "Action",
				Payload = "payload",
				Window = window
			};
		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Same(engine, newEngine);
	}
}
