using Moq;
using Xunit;

namespace Whim.ImmutableTreeLayout.Tests;

public class AddTests
{
	// private class Wrapper
	// {
	// 	public Mock<IContext> Context { get; } = new();
	// 	public Mock<IImmutableInternalTreePlugin> Plugin { get; } = new();

	// 	public Wrapper SetAsPhantom(Mock<IWindow> window)
	// 	{
	// 		Plugin.Setup(x => x.PhantomWindows).Returns(new HashSet<IWindow> { window.Object });
	// 		return this;
	// 	}
	// }

	// [Fact]
	// public void AddWindow_RootIsNull()
	// {
	// 	// Given
	// 	Wrapper wrapper = new();
	// 	Mock<IWindow> window = new();
	// 	TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

	// 	// When
	// 	IImmutableLayoutEngine result = engine.Add(window.Object);

	// 	// Then
	// 	Assert.NotSame(engine, result);
	// 	Assert.True(result.Contains(window.Object));
	// 	Assert.Equal(1, result.Count);
	// }

	// [Fact]
	// public void AddPhantom_RootIsNull()
	// {
	// 	// Given
	// 	Mock<IWindow> window = new();
	// 	Wrapper wrapper = new Wrapper().SetAsPhantom(window);
	// 	TreeLayoutEngine engine = new(wrapper.Context.Object, wrapper.Plugin.Object);

	// 	// When
	// 	IImmutableLayoutEngine result = engine.Add(window.Object);

	// 	// Then
	// 	Assert.NotSame(engine, result);
	// 	Assert.True(result.Contains(window.Object));
	// 	Assert.Equal(1, result.Count);
	// }

	// [Fact]
	// public void AddWindow_RootIsPhantom()
	// {
	// 	// Given
	// 	Mock<IWindow> phantomWindow = new();
	// 	Wrapper wrapper = new Wrapper().SetAsPhantom(phantomWindow);
	// 	IImmutableLayoutEngine engine = new TreeLayoutEngine(wrapper.Context.Object, wrapper.Plugin.Object).Add(
	// 		phantomWindow.Object
	// 	);

	// 	Mock<IWindow> window = new();

	// 	// When
	// 	IImmutableLayoutEngine result = engine.Add(window.Object);

	// 	// Then
	// 	Assert.NotSame(engine, result);
	// 	Assert.False(result.Contains(phantomWindow.Object));
	// 	Assert.True(result.Contains(window.Object));
	// 	Assert.Equal(1, result.Count);
	// }
}
