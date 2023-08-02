using Moq;
using System.Collections.Generic;
using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class LayoutEngineTests
{
	#region GetLayoutEngine
	[Fact]
	public void GetLayoutEngine_IsT()
	{
		// Given
		ILayoutEngine engine = new TestLayoutEngine();

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
		TestLayoutEngine engine = new();
		ILayoutEngine proxyInner = new TestProxyLayoutEngine(engine);
		ILayoutEngine proxyOuter = new TestProxyLayoutEngine(proxyInner);

		// When
		TestLayoutEngine? newEngine = proxyOuter.GetLayoutEngine<TestLayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		ILayoutEngine engine = new TestLayoutEngine();

		// When
		ILayoutEngine? newEngine = engine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region
	[Fact]
	public void ContainsEqual_IsT()
	{
		// Given
		ILayoutEngine engine = new TestLayoutEngine();

		// When
		bool contains = engine.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_IsProxy()
	{
		// Given
		TestLayoutEngine engine = new();
		ILayoutEngine proxyInner = new TestProxyLayoutEngine(engine);
		ILayoutEngine proxyOuter = new TestProxyLayoutEngine(proxyInner);

		// When
		bool contains = proxyOuter.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_Null()
	{
		// Given
		ILayoutEngine engine = new TestLayoutEngine();

		// When
		bool contains = engine.ContainsEqual(new Mock<ILayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion
}
