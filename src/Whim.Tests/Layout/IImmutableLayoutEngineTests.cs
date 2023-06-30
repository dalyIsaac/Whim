using Moq;
using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class IImmutableLayoutEngineTests
{
	private class ProxyLayoutEngine : ImmutableBaseProxyLayoutEngine
	{
		public ProxyLayoutEngine(IImmutableLayoutEngine innerLayoutEngine)
			: base(innerLayoutEngine) { }

		public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			InnerLayoutEngine.DoLayout(location, monitor);
	}

	private class TestLayoutEngine : IImmutableLayoutEngine
	{
		public string Name => throw new System.NotImplementedException();

		public int Count => throw new System.NotImplementedException();

		public IImmutableLayoutEngine Add(IWindow window) => throw new System.NotImplementedException();

		public IImmutableLayoutEngine AddAtPoint(IWindow window, IPoint<double> point) =>
			throw new System.NotImplementedException();

		public bool Contains(IWindow window) => throw new System.NotImplementedException();

		public IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			throw new System.NotImplementedException();

		public void FocusWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();

		public IWindow? GetFirstWindow() => throw new System.NotImplementedException();

		public IImmutableLayoutEngine HidePhantomWindows() => throw new System.NotImplementedException();

		public IImmutableLayoutEngine MoveWindowEdgesInDirection(
			Direction edges,
			IPoint<double> deltas,
			IWindow window
		) => throw new System.NotImplementedException();

		public IImmutableLayoutEngine Remove(IWindow window) => throw new System.NotImplementedException();

		public IImmutableLayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();
	}

	internal interface ITestLayoutEngine : IImmutableLayoutEngine { }

	#region GetLayoutEngine
	[Fact]
	public void GetLayoutEngine_IsT()
	{
		// Given
		IImmutableLayoutEngine engine = new TestLayoutEngine();

		// When
		IImmutableLayoutEngine? newEngine = engine.GetLayoutEngine<IImmutableLayoutEngine>();

		// Then
		Assert.Same(engine, newEngine);
		Assert.NotNull(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsProxy()
	{
		// Given
		TestLayoutEngine engine = new();
		IImmutableLayoutEngine proxyInner = new ProxyLayoutEngine(engine);
		IImmutableLayoutEngine proxyOuter = new ProxyLayoutEngine(proxyInner);

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
		IImmutableLayoutEngine engine = new TestLayoutEngine();

		// When
		IImmutableLayoutEngine? newEngine = engine.GetLayoutEngine<ITestLayoutEngine>();

		// Then
		Assert.Null(newEngine);
	}
	#endregion

	#region
	[Fact]
	public void ContainsEqual_IsT()
	{
		// Given
		IImmutableLayoutEngine engine = new TestLayoutEngine();

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
		IImmutableLayoutEngine proxyInner = new ProxyLayoutEngine(engine);
		IImmutableLayoutEngine proxyOuter = new ProxyLayoutEngine(proxyInner);

		// When
		bool contains = proxyOuter.ContainsEqual(engine);

		// Then
		Assert.True(contains);
	}

	[Fact]
	public void ContainsEqual_Null()
	{
		// Given
		IImmutableLayoutEngine engine = new TestLayoutEngine();

		// When
		bool contains = engine.ContainsEqual(new Mock<IImmutableLayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion
}
