using Whim.TestUtils;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class GetFirstWindowTests
{
	[Fact]
	public void GetFirstWindow_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_RootIsWindow(IWindow window)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity).AddWindow(
			window
		);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window, result);
	}

	[Theory, AutoSubstituteData]
	public void GetFirstWindow_RootIsSplitNode(IWindow window1, IWindow window2, IWindow window3)
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context, wrapper.Plugin, wrapper.Identity)
			.AddWindow(window1)
			.AddWindow(window2)
			.AddWindow(window3);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window1, result);
	}
}
