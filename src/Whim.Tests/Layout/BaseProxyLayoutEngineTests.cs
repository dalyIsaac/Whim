using Moq;
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
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		TestUtils.TestProxyLayoutEngine? newEngine =
			proxyLayoutEngine.Object.GetLayoutEngine<TestUtils.TestProxyLayoutEngine>();

		// Then
		Assert.Same(proxyLayoutEngine.Object, newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsInner()
	{
		// Given
		Mock<TestUtils.ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(x => x.GetLayoutEngine<TestUtils.ITestLayoutEngine>())
			.Returns(innerLayoutEngine.Object);
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		TestUtils.ITestLayoutEngine? newEngine =
			proxyLayoutEngine.Object.GetLayoutEngine<TestUtils.ITestLayoutEngine>();

		// Then
		Assert.Same(innerLayoutEngine.Object, newEngine);
		innerLayoutEngine.Verify(x => x.GetLayoutEngine<TestUtils.ITestLayoutEngine>(), Times.Once);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		TestUtils.ITestLayoutEngine? newEngine =
			proxyLayoutEngine.Object.GetLayoutEngine<TestUtils.ITestLayoutEngine>();

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
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);
		proxyLayoutEngine.Setup(p => p.Equals(proxyLayoutEngine.Object)).Returns(true);

		// When
		bool contains = proxyLayoutEngine.Object.ContainsEqual(proxyLayoutEngine.Object);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_IsInner()
	{
		// Given
		Mock<TestUtils.ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.ContainsEqual(innerLayoutEngine.Object)).Returns(true);
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

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
		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

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

		Mock<TestUtils.TestProxyLayoutEngine> proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		LayoutEngineIdentity proxyIdentity = proxyLayoutEngine.Object.Identity;

		// Then
		Assert.Equal(innerLayoutEngine.Object.Identity, proxyIdentity);
	}
}
