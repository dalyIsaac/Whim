using Moq;
using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class BaseStackLayoutEngineTests
{
	private class TestLayoutEngine : BaseStackLayoutEngine
	{
		public override string Name => "Test";

		public override void AddWindowAtPoint(IWindow window, IPoint<double> point) =>
			throw new System.NotImplementedException();

		public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
			throw new System.NotImplementedException();

		public override void FocusWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();

		public override void MoveWindowEdgeInDirection(Direction edge, double delta, IWindow window) =>
			throw new System.NotImplementedException();

		public override void SwapWindowInDirection(Direction direction, IWindow window) =>
			throw new System.NotImplementedException();
	}

	[Fact]
	public void Add()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();

		// When
		engine.Add(window.Object);

		// Then
		Assert.Single(engine);
	}

	[Fact]
	public void Remove()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);

		// When
		bool result = engine.Remove(window.Object);

		// Then
		Assert.True(result);
		Assert.Empty(engine);
	}

	[Fact]
	public void Clear()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);

		// When
		engine.Clear();

		// Then
		Assert.Empty(engine);
	}

	[Fact]
	public void Contains()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);

		// When
		bool result = engine.Contains(window.Object);

		// Then
		Assert.True(result);
	}

	[Fact]
	public void CopyTo()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);
		IWindow[] array = new IWindow[1];

		// When
		engine.CopyTo(array, 0);

		// Then
		Assert.Equal(window.Object, array[0]);
	}

	[Fact]
	public void GetEnumerator()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);

		// When
		IEnumerator<IWindow> enumerator = engine.GetEnumerator();

		// Then
		Assert.True(enumerator.MoveNext());
		Assert.Equal(window.Object, enumerator.Current);
	}

	[Fact]
	public void GetFirstWindow()
	{
		// Given
		TestLayoutEngine engine = new();
		Mock<IWindow> window = new();
		engine.Add(window.Object);

		// When
		IWindow? result = engine.GetFirstWindow();

		// Then
		Assert.Equal(window.Object, result);
	}
}
