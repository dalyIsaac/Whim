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
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		TestProxyLayoutEngine? newEngine = proxyLayoutEngine.Object.GetLayoutEngine<TestProxyLayoutEngine>();

		// Then
		Assert.Same(proxyLayoutEngine.Object, newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsInner()
	{
		// Given
		Mock<ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetLayoutEngine<ITestLayoutEngine>()).Returns(innerLayoutEngine.Object);
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.Object.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Same(innerLayoutEngine.Object, newEngine);
		innerLayoutEngine.Verify(x => x.GetLayoutEngine<ITestLayoutEngine>(), Times.Once);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.Object.GetLayoutEngine<ITestLayoutEngine>();

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
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.Object.ContainsEqual(proxyLayoutEngine.Object);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_IsInner()
	{
		// Given
		Mock<ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.ContainsEqual(innerLayoutEngine.Object)).Returns(true);
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.Object.ContainsEqual(innerLayoutEngine.Object);

		// Then
		Assert.True(contains);
		innerLayoutEngine.Verify(x => x.ContainsEqual(innerLayoutEngine.Object), Times.Once);
	}

	[Fact]
	public void ContainsEqual_False()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.Object.ContainsEqual(new Mock<ILayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion

	[Fact]
	public void Identity()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Identity).Returns(new LayoutEngineIdentity());

		Mock<TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		LayoutEngineIdentity proxyIdentity = proxyLayoutEngine.Object.Identity;

		// Then
		Assert.Equal(innerLayoutEngine.Object.Identity, proxyIdentity);
	}
}
