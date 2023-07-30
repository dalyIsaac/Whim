using Moq;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class BaseProxyLayoutEngineTests
{
	#region GetLayoutEngine
	[Fact]
	public void GetLayoutEngine_IsT()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		TestProxyLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<TestProxyLayoutEngine>();

		// Then
		Assert.Same(proxyLayoutEngine, newEngine);
		Assert.IsType<TestProxyLayoutEngine>(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsInner()
	{
		// Given
		Mock<ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetLayoutEngine<ITestLayoutEngine>()).Returns(innerLayoutEngine.Object);
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Same(innerLayoutEngine.Object, newEngine);
		innerLayoutEngine.Verify(x => x.GetLayoutEngine<ITestLayoutEngine>(), Times.Once);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region ContainsEqual
	[Fact]
	public void ContainsEqual_IsT()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(proxyLayoutEngine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_IsInner()
	{
		// Given
		Mock<ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.ContainsEqual(innerLayoutEngine.Object)).Returns(true);
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(innerLayoutEngine.Object);

		// Then
		Assert.True(contains);
		innerLayoutEngine.Verify(x => x.ContainsEqual(innerLayoutEngine.Object), Times.Once);
	}

	[Fact]
	public void ContainsEqual_False()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		TestProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(new Mock<ILayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion
}
