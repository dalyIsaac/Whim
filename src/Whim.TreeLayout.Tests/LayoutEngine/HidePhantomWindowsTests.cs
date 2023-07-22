using Moq;
using Xunit;

namespace Whim.TreeLayout.Tests;

public class HidePhantomWindowsTests
{
	[Fact]
	public void HidePhantomWindows()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		Mock<IWindow> phantomWindow1 = new();
		Mock<IWindow> phantomWindow2 = new();

		LayoutEngineWrapper wrapper = new LayoutEngineWrapper()
			.SetAsLastFocusedWindow(null)
			.SetAsPhantom(phantomWindow1.Object, phantomWindow2.Object);

		IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object)
			.Add(window1.Object)
			.Add(window2.Object)
			.Add(phantomWindow1.Object)
			.Add(phantomWindow2.Object);

		// When
		engine.HidePhantomWindows();

		// Then
		window1.Verify(x => x.Hide(), Times.Never);
		window2.Verify(x => x.Hide(), Times.Never);
		phantomWindow1.Verify(x => x.Hide(), Times.Once);
		phantomWindow2.Verify(x => x.Hide(), Times.Once);
	}
}
