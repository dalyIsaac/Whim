namespace Whim.Tests;

public class BaseProxyLayoutEngineTests
{
	#region GetLayoutEngine
	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_IsT(ILayoutEngine innerLayoutEngine)
	{
		// Given
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		TestProxyLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<TestProxyLayoutEngine>();

		// Then
		Assert.Same(proxyLayoutEngine, newEngine);
	}

	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_IsInner(ITestLayoutEngine innerLayoutEngine)
	{
		// Given
		innerLayoutEngine.GetLayoutEngine<ITestLayoutEngine>().Returns(innerLayoutEngine);
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Same(innerLayoutEngine, newEngine);
		innerLayoutEngine.Received(1).GetLayoutEngine<ITestLayoutEngine>();
	}

	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_Null(ILayoutEngine innerLayoutEngine)
	{
		// Given
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);
		innerLayoutEngine.GetLayoutEngine<ITestLayoutEngine>().Returns((ILayoutEngine?)null);

		// When
		ITestLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region ContainsEqual
	[Theory, AutoSubstituteData]
	public void ContainsEqual_IsT(ILayoutEngine innerLayoutEngine)
	{
		// Given
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(proxyLayoutEngine);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void ContainsEqual_IsInner(ITestLayoutEngine innerLayoutEngine)
	{
		// Given
		innerLayoutEngine.ContainsEqual(innerLayoutEngine).Returns(true);
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(innerLayoutEngine);

		// Then
		Assert.True(contains);
		innerLayoutEngine.Received(1).ContainsEqual(innerLayoutEngine);
	}

	[Theory, AutoSubstituteData]
	public void ContainsEqual_False(ILayoutEngine innerLayoutEngine)
	{
		// Given
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(Substitute.For<ILayoutEngine>());

		// Then
		Assert.False(contains);
	}
	#endregion

	[Theory, AutoSubstituteData]
	public void Identity(ILayoutEngine innerLayoutEngine)
	{
		// Given
		TestProxyLayoutEngine proxyLayoutEngine = Substitute.For<TestProxyLayoutEngine>(innerLayoutEngine);

		// When
		LayoutEngineIdentity proxyIdentity = proxyLayoutEngine.Identity;

		// Then
		Assert.Equal(innerLayoutEngine.Identity, proxyIdentity);
	}
}
