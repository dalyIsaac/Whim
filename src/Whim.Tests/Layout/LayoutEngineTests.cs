using Moq;
using Xunit;

namespace Whim.Tests;

public class LayoutEngineTests
{
	#region GetLayoutEngine
	[Fact]
	public void GetLayoutEngine_IsT()
	{
		// Given
		ILayoutEngine engine = new TestUtils.TestLayoutEngine();

		// When
		ILayoutEngine? newEngine = engine.GetLayoutEngine<ILayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsProxy()
	{
		// Given
		TestUtils.TestLayoutEngine engine = new();
		Mock<TestUtils.TestProxyLayoutEngine> proxyInner = new(engine);
		Mock<TestUtils.TestProxyLayoutEngine> proxyOuter = new(proxyInner.Object);

		// When
		TestUtils.TestLayoutEngine? newEngine = proxyOuter.Object.GetLayoutEngine<TestUtils.TestLayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		ILayoutEngine engine = new TestUtils.TestLayoutEngine();

		// When
		ILayoutEngine? newEngine = engine.GetLayoutEngine<TestUtils.ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region
	[Fact]
	public void ContainsEqual_IsT()
	{
		// Given
		ILayoutEngine engine = new TestUtils.TestLayoutEngine();

		// When
		bool contains = engine.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_IsProxy()
	{
		// Given
		TestUtils.TestLayoutEngine engine = new();
		Mock<TestUtils.TestProxyLayoutEngine> proxyInner = new(engine);
		Mock<TestUtils.TestProxyLayoutEngine> proxyOuter = new(proxyInner.Object);

		// When
		bool contains = proxyOuter.Object.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_Null()
	{
		// Given
		ILayoutEngine engine = new TestUtils.TestLayoutEngine();

		// When
		bool contains = engine.ContainsEqual(new Mock<ILayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion
}
