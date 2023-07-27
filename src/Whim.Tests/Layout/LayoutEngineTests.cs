using Moq;
using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class LayoutEngineTests
{
	private class ProxyLayoutEngine : BaseProxyLayoutEngine
	{
		public ProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
			: base(innerLayoutEngine) { }

		protected override ILayoutEngine Update(ILayoutEngine newLayoutEngine) =>
			newLayoutEngine == InnerLayoutEngine ? this : new ProxyLayoutEngine(newLayoutEngine);

		public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			InnerLayoutEngine.DoLayout(location, monitor);
	}

	private class TestLayoutEngine : ILayoutEngine
	{
		public LayoutEngineIdentity Identity => throw new System.NotImplementedException();
		public string Name => throw new System.NotImplementedException();

		public int Count => throw new System.NotImplementedException();

		public ILayoutEngine AddWindow(IWindow window) => throw new System.NotImplementedException();

		public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) =>
			throw new System.NotImplementedException();

		public bool ContainsWindow(IWindow window) => throw new System.NotImplementedException();

		public IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			throw new System.NotImplementedException();

		public void FocusWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();

		public IWindow? GetFirstWindow() => throw new System.NotImplementedException();

		public void HidePhantomWindows() => throw new System.NotImplementedException();

		public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) =>
			throw new System.NotImplementedException();

		public ILayoutEngine RemoveWindow(IWindow window) => throw new System.NotImplementedException();

		public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();
	}

	internal interface ITestLayoutEngine : ILayoutEngine { }

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
		ILayoutEngine proxyInner = new ProxyLayoutEngine(engine);
		ILayoutEngine proxyOuter = new ProxyLayoutEngine(proxyInner);

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
		ILayoutEngine proxyInner = new ProxyLayoutEngine(engine);
		ILayoutEngine proxyOuter = new ProxyLayoutEngine(proxyInner);

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
