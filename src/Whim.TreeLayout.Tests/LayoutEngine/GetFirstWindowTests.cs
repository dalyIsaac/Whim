using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class GetFirstWindowTests
{
	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void GetFirstWindow_RootIsNull(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void GetFirstWindow_RootIsWindow(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity).AddWindow(window);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}

	[Theory, AutoSubstituteData<TreeCustomization>]
	internal void GetFirstWindow_RootIsSplitNode(
		IContext ctx,
		MutableRootSector root,
		ITreeLayoutPlugin plugin,
		LayoutEngineIdentity identity,
		Workspace workspace,
		IWindow window1,
		IWindow window2,
		IWindow window3
	)
	{
		// Given
		TreeCustomization.SetAsLastFocusedWindow(ctx, root, workspace, null);

		ILayoutEngine engine = new TreeLayoutEngine(ctx, plugin, identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window1, result);
	}
}
