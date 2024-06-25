namespace Whim.Tests;

public class LayoutEngineTests
{
	#region GetLayoutEngine
	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_IsT(TestLayoutEngine engine)
	{
		// When
		ILayoutEngine? newEngine = ((ILayoutEngine)engine).GetLayoutEngine<ILayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_IsProxy(TestLayoutEngine engine)
	{
		// Given
		TestProxyLayoutEngine proxyInner = Substitute.For<TestProxyLayoutEngine>(engine);
		TestProxyLayoutEngine proxyOuter = Substitute.For<TestProxyLayoutEngine>(proxyInner);

		// When
		TestLayoutEngine? newEngine = proxyOuter.GetLayoutEngine<TestLayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Theory, AutoSubstituteData]
	public void GetLayoutEngine_Null(TestLayoutEngine engine)
	{
		// When
		ILayoutEngine? newEngine = ((ILayoutEngine)engine).GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region
	[Theory, AutoSubstituteData]
	public void ContainsEqual_IsT(TestLayoutEngine engine)
	{
		// When
		bool contains = ((ILayoutEngine)engine).ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void ContainsEqual_IsProxy(TestLayoutEngine engine)
	{
		// Given
		TestProxyLayoutEngine proxyInner = Substitute.For<TestProxyLayoutEngine>(engine);
		TestProxyLayoutEngine proxyOuter = Substitute.For<TestProxyLayoutEngine>(proxyInner);

		// When
		bool contains = proxyOuter.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Theory, AutoSubstituteData]
	public void ContainsEqual_Null(TestLayoutEngine engine)
	{
		// When
		bool contains = ((ILayoutEngine)engine).ContainsEqual(Substitute.For<ILayoutEngine>());

		// Then
		Assert.False(contains);
	}
	#endregion
}
