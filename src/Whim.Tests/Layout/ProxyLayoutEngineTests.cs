using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Whim.Tests;

public class ProxyLayoutEngineTests
{
	private class ProxyLayoutEngine : BaseProxyLayoutEngine
	{
		public ProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
			: base(innerLayoutEngine) { }

		public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			Array.Empty<IWindowState>();
	}

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
	public void IsReadOnly()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.IsReadOnly).Returns(true);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool isReadOnly = proxyLayoutEngine.IsReadOnly;

		// Then
		Assert.True(isReadOnly);
	}

	[Fact]
	public void Add()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Add(It.IsAny<IWindow>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.Add(new Mock<IWindow>().Object);

		// Then
		innerLayoutEngine.Verify(x => x.Add(It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void Remove()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Remove(It.IsAny<IWindow>())).Returns(true);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool removed = proxyLayoutEngine.Remove(new Mock<IWindow>().Object);

		// Then
		Assert.True(removed);
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
		proxyLayoutEngine.SwapWindowInDirection(Direction.Left, new Mock<IWindow>().Object);

		// Then
		innerLayoutEngine.Verify(x => x.SwapWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()), Times.Once);
	}

	[Fact]
	public void Clear()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.Clear());

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.Clear();

		// Then
		innerLayoutEngine.Verify(x => x.Clear(), Times.Once);
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
	public void CopyTo()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.CopyTo(It.IsAny<IWindow[]>(), It.IsAny<int>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.CopyTo(Array.Empty<IWindow>(), 0);

		// Then
		innerLayoutEngine.Verify(x => x.CopyTo(It.IsAny<IWindow[]>(), It.IsAny<int>()), Times.Once);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetEnumerator()).Returns(Mock.Of<IEnumerator<IWindow>>());

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		IEnumerator<IWindow> enumerator = proxyLayoutEngine.GetEnumerator();

		// Then
		Assert.NotNull(enumerator);
		innerLayoutEngine.Verify(x => x.GetEnumerator(), Times.Once);
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
		proxyLayoutEngine.MoveWindowEdgesInDirection(Direction.Left, new Point<double>(), new Mock<IWindow>().Object);

		// Then
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
		IWindowState[] windowStates = proxyLayoutEngine
			.DoLayout(new Location<int>(), new Mock<IMonitor>().Object)
			.ToArray();

		// Then
		Assert.Empty(windowStates);
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
	public void AddWindowAtPoint()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.AddWindowAtPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<double>>()));

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		proxyLayoutEngine.AddWindowAtPoint(new Mock<IWindow>().Object, new Point<double>());

		// Then
		innerLayoutEngine.Verify(x => x.AddWindowAtPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<double>>()), Times.Once);
	}

	[Fact]
	public void GetLayoutEngine_Immediate()
	{
		// Given
		ILayoutEngine columnLayoutEngine = new ColumnLayoutEngine();

		ProxyLayoutEngine proxyLayoutEngine = new(columnLayoutEngine);

		// When
		ILayoutEngine? layoutEngine = proxyLayoutEngine.GetLayoutEngine<ColumnLayoutEngine>();

		// Then
		Assert.Equal(columnLayoutEngine, layoutEngine);
	}

	[Fact]
	public void GetLayoutEngine_Proxy()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetLayoutEngine<ColumnLayoutEngine>()).Returns(new ColumnLayoutEngine());

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine? layoutEngine = proxyLayoutEngine.GetLayoutEngine<ColumnLayoutEngine>();

		// Then
		Assert.NotNull(layoutEngine);
		innerLayoutEngine.Verify(x => x.GetLayoutEngine<ColumnLayoutEngine>(), Times.Once);
	}

	[Fact]
	public void GetLayoutEngine_Null()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
		innerLayoutEngine.Setup(x => x.GetLayoutEngine<ColumnLayoutEngine>()).Returns((ColumnLayoutEngine?)null);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		ILayoutEngine? layoutEngine = proxyLayoutEngine.GetLayoutEngine<ColumnLayoutEngine>();

		// Then
		Assert.Null(layoutEngine);
		innerLayoutEngine.Verify(x => x.GetLayoutEngine<ColumnLayoutEngine>(), Times.Once);
	}

	[Fact]
	public void ContainsEqual_Immediate()
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
	public void ContainsEqual_Proxy()
	{
		// Given
		Mock<ILayoutEngine> innerLayoutEngine = new();
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
		innerLayoutEngine.Setup(x => x.ContainsEqual(innerLayoutEngine.Object)).Returns(false);

		ProxyLayoutEngine proxyLayoutEngine = new(innerLayoutEngine.Object);

		// When
		bool contains = proxyLayoutEngine.ContainsEqual(innerLayoutEngine.Object);

		// Then
		Assert.False(contains);
		innerLayoutEngine.Verify(x => x.ContainsEqual(innerLayoutEngine.Object), Times.Once);
	}
}
