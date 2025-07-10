using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class PerformCustomActionTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void PerformCustomAction(
		IContext ctx,
		MutableRootSector root,
		Workspace workspace,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		IWindow window
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);
		LayoutEngineCustomAction<string> action = new()
		{
			Name = "Action",
			Payload = "payload",
			Window = window,
		};
		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		// When
		ILayoutEngine newEngine = engine.PerformCustomAction(action);

		// Then
		Assert.Same(engine, newEngine);
	}
}
