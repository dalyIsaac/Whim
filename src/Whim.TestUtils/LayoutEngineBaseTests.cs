using Moq;
using System;
using Xunit;

namespace Whim.TestUtils;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Tests for <see cref="ILayoutEngine"/> implementations.
/// </summary>
public abstract class LayoutEngineBaseTests
{
	/// <summary>
	/// The factory method for creating a new <see cref="ILayoutEngine"/> instance.
	/// </summary>
	public abstract Func<ILayoutEngine> CreateLayoutEngine { get; }

	[Fact]
	public void AddWindow_NewWindow()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		ILayoutEngine result = layoutEngine.AddWindow(window.Object);

		// Then
		Xunit.Assert.NotSame(layoutEngine, result);
		Xunit.Assert.True(result.ContainsWindow(window.Object));
	}

	[Fact]
	public void AddWindow_WindowAlreadyIncluded()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window.Object);

		// When
		ILayoutEngine result = layoutEngine.AddWindow(window.Object);

		// Then
		Xunit.Assert.Same(layoutEngine, result);
		Xunit.Assert.True(result.ContainsWindow(window.Object));
		Xunit.Assert.Equal(1, result.Count);
	}

	[Fact]
	public void RemoveWindow_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window.Object);

		// When
		ILayoutEngine result = layoutEngine.RemoveWindow(window.Object);

		// Then
		Xunit.Assert.NotSame(layoutEngine, result);
		Xunit.Assert.False(result.ContainsWindow(window.Object));
		Xunit.Assert.Equal(0, result.Count);
	}

	[Fact]
	public void RemoveWindow_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		ILayoutEngine result = layoutEngine.RemoveWindow(window.Object);

		// Then
		Xunit.Assert.Same(layoutEngine, result);
		Xunit.Assert.False(result.ContainsWindow(window.Object));
		Xunit.Assert.Equal(0, result.Count);
	}

	[Fact]
	public void ContainsWindow_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window.Object);

		// When
		bool result = layoutEngine.ContainsWindow(window.Object);

		// Then
		Xunit.Assert.True(result);
	}

	[Fact]
	public void ContainsWindow_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		bool result = layoutEngine.ContainsWindow(window.Object);

		// Then
		Xunit.Assert.False(result);
	}

	[Fact]
	public void SwapWindowInDirection_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object).AddWindow(window2.Object);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void SwapWindowInDirection_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window2.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void FocusWindowInDirection_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object).AddWindow(window2.Object);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void FocusWindowInDirection_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window2.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowToPoint_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object);

		// When
		layoutEngine.MoveWindowToPoint(window1.Object, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowToPoint_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		layoutEngine.MoveWindowToPoint(window1.Object, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowEdgesInDirection_WindowIsPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1.Object);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowEdgesInDirection_WindowIsNotPresent()
	{
		// Given
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1.Object);

		// Then it shouldn't throw.
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
