using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class BaseProxyLayoutEngineTests
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

	internal interface ITestLayoutEngine : ILayoutEngine { }

	[Fact]
	public void Name()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Name).Returns("Inner Layout Engine");

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		string name = proxyLayoutEngine.Name;

		// Then
		Assert.Equal("Inner Layout Engine", name);
	}

	[Fact]
	public void Count()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Count).Returns(42);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		int count = proxyLayoutEngine.Count;

		// Then
		Assert.Equal(42, count);
	}

	[Fact]
	public void Add()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Add(It.IsAny<IWindow>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = proxyLayoutEngine.Add(new Mock<IWindow>().Object);

		// Then
		Assert.NotSame(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
		innerLayoutEngine.Verify(x => x.Add(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void Remove()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Remove(It.IsAny<IWindow>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = proxyLayoutEngine.Remove(new Mock<IWindow>().Object);

		// Then
		Assert.NotSame(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
		innerLayoutEngine.Verify(x => x.Remove(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void GetFirstWindow()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetFirstWindow()).Returns(new Mock<IWindow>().Object);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		IWindow? window = proxyLayoutEngine.GetFirstWindow();

		// Then
		Assert.NotNull(window);
		innerLayoutEngine.Verify(x => x.GetFirstWindow(), Times.Once);
	}

	[Fact]
	public void FocusWindowInDirection()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.FocusWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.FocusWindowInDirection(Direction.Left, new Mock<IWindow>().Object);

		// Then
		innerLayoutEngine.Verify(x => x.FocusWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void SwapWindowInDirection()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.SwapWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = proxyLayoutEngine.SwapWindowInDirection(Direction.Left, new Mock<IWindow>().Object);

		// Then
		Assert.NotSame(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
		innerLayoutEngine.Verify(x => x.SwapWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void Contains()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Contains(It.IsAny<IWindow>())).Returns(true);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.Contains(new Mock<IWindow>().Object);

		// Then
		Assert.True(contains);
		innerLayoutEngine.Verify(x => x.Contains(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void MoveWindowEdgesInDirection()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(
			x => x.MoveWindowEdgesInDirection(It.IsAny<Direction>(), It.IsAny<IPoint<double>>(), It.IsAny<IWindow>())
		);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = proxyLayoutEngine.MoveWindowEdgesInDirection(
			Direction.Left,
			new Point<double>(),
			new Mock<IWindow>().Object
		);

		// Then
		Assert.NotSame(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
		innerLayoutEngine.Verify(
			x => x.MoveWindowEdgesInDirection(It.IsAny<Direction>(), It.IsAny<IPoint<double>>(), It.IsAny<IWindow>()),
			Times.Once
		);
	}

	[Fact]
	public void DoLayout()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine
			.Setup(x => x.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()))
			.Returns(Array.Empty<IWindowState>());

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		IEnumerable<IWindowState> windows = proxyLayoutEngine.DoLayout(
			new Mock<ILocation<int>>().Object,
			new Mock<IMonitor>().Object
		);

		// Then
		Assert.Empty(windows);
		innerLayoutEngine.Verify(x => x.DoLayout(It.IsAny<ILocation<int>>(), It.IsAny<IMonitor>()), Times.Once);
	}

	[Fact]
	public void HidePhantomWindows()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.HidePhantomWindows());

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.HidePhantomWindows();

		// Then
		innerLayoutEngine.Verify(x => x.HidePhantomWindows(), Times.Once);
	}

	[Fact]
	public void AddAtPoint()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.AddAtPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<double>>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine newEngine = proxyLayoutEngine.AddAtPoint(new Mock<IWindow>().Object, new Point<double>());

		// Then
		Assert.NotSame(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
		innerLayoutEngine.Verify(x => x.AddAtPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<double>>()), Times.Once);
	}

	#region GetLayoutEngine
	[Fact]
	public void GetLayoutEngine_IsT()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ProxyLayoutEngine? newEngine = proxyLayoutEngine.GetLayoutEngine<ProxyLayoutEngine>();

		// Then
		Assert.Same(proxyLayoutEngine, newEngine);
		Assert.IsType<ProxyLayoutEngine>(newEngine);
	}

	[Fact]
	public void GetLayoutEngine_IsInner()
	{
		// Given
		Mock<ITestLayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetLayoutEngine<ITestLayoutEngine>()).Returns(innerLayoutEngine.Object);
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

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
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

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
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

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
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

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
		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(new Mock<ILayoutEngine>().Object);

		// Then
		Assert.False(contains);
	}
	#endregion
}
