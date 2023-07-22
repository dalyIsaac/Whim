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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object);

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

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(window.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window.Object, result);
	}

	[Fact]
	public void GetFirstWindow_RootIsPhantomWindow()
	{
		// Given
		Mock<IWindow> window = new();
		LayoutEngineWrapper wrapper = new LayoutEngineWrapper()
			.SetAsLastFocusedWindow(null)
			.SetAsPhantom(window.Object);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(window.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Null(result);
	}

	[Fact]
	public void GetFirstWindow_RootIsSplitNode()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> window3 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper().SetAsLastFocusedWindow(null);

		ILayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(window3.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Same(window1.Object, result);
	}
}
