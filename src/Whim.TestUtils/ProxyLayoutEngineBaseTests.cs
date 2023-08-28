using Moq;
using System;
using Xunit;

namespace Whim.TestUtils;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// Tests for <see cref="ILayoutEngine"/> implementations.
/// </summary>
public abstract class ProxyLayoutEngineBaseTests
{
	/// <summary>
	/// The factory method for creating a new <see cref="ILayoutEngine"/> instance.
	/// </summary>
	public abstract Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine { get; }

	private static ILayoutEngine CreateInnerLayoutEngine()
	{
		Mock<ILayoutEngine> inner = new();

		inner.Setup(x => x.AddWindow(It.IsAny<IWindow>())).Returns(CreateInnerLayoutEngine);
		inner.Setup(x => x.RemoveWindow(It.IsAny<IWindow>())).Returns(CreateInnerLayoutEngine);
		inner
			.Setup(x => x.MoveWindowToPoint(It.IsAny<IWindow>(), It.IsAny<IPoint<double>>()))
			.Returns(CreateInnerLayoutEngine);
		inner.Setup(x => x.ContainsWindow(It.IsAny<IWindow>())).Returns(true);
		inner
			.Setup(x => x.SwapWindowInDirection(It.IsAny<Direction>(), It.IsAny<IWindow>()))
			.Returns(CreateInnerLayoutEngine);
		return inner.Object;
	}

	[Fact]
	public void AddWindow_NewWindow()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.AddWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void AddWindow_WindowAlreadyIncluded()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window.Object);

		// When
		layoutEngine.AddWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void RemoveWindow_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window.Object);

		// When
		layoutEngine.RemoveWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void RemoveWindow_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.RemoveWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void ContainsWindow_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window.Object);

		// When
		layoutEngine.ContainsWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void ContainsWindow_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.ContainsWindow(window.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void SwapWindowInDirection_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object).AddWindow(window2.Object);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void SwapWindowInDirection_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window2.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void FocusWindowInDirection_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object).AddWindow(window2.Object);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void FocusWindowInDirection_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		Mock<IWindow> window2 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window2.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowToPoint_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object);

		// When
		layoutEngine.MoveWindowToPoint(window1.Object, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowToPoint_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.MoveWindowToPoint(window1.Object, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowEdgesInDirection_WindowIsPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1.Object);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1.Object);

		// Then it shouldn't throw.
	}

	[Fact]
	public void MoveWindowEdgesInDirection_WindowIsNotPresent()
	{
		// Given
		ILayoutEngine inner = CreateInnerLayoutEngine();
		Mock<IWindow> window1 = new();
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1.Object);

		// Then it shouldn't throw.
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
