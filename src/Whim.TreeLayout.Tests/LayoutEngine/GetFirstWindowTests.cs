using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class GetFirstWindowTests
{
	[Fact]
	public void GetFirstWindow_RootIsNull()
	{
		// Given
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetFirstWindow_RootIsWindow()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(
			wrapper.Context.Object,
			wrapper.Plugin.Object,
			wrapper.Identity
		).AddWindow(window.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window.Object, result);
	}

	[Fact]
	public void GetFirstWindow_RootIsSplitNode()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object, wrapper.Identity)
			.AddWindow(window1.Object)
			.AddWindow(window2.Object)
			.AddWindow(window3.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window1.Object, result);
	}
}
