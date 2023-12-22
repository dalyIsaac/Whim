using System;
using Xunit;

namespace Whim.TestUtils;

/// <summary>
/// Tests for <see cref="ILayoutEngine"/> implementations.
/// </summary>
public abstract class ProxyLayoutEngineBaseTests
{
	/// <summary>
	/// The factory method for creating a new <see cref="ILayoutEngine"/> instance.
	/// </summary>
	public abstract Func<ILayoutEngine, BaseProxyLayoutEngine> CreateLayoutEngine { get; }

	[Theory, AutoSubstituteData]
	public void AddWindow_NewWindow(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.AddWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowAlreadyIncluded(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window);

		// When
		layoutEngine.AddWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowIsPresent(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window);

		// When
		layoutEngine.RemoveWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowIsNotPresent(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.RemoveWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_WindowIsPresent(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window);

		// When
		layoutEngine.ContainsWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_WindowIsNotPresent(ILayoutEngine inner, IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.ContainsWindow(window);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowIsPresent(ILayoutEngine inner, IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1).AddWindow(window2);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowIsNotPresent(ILayoutEngine inner, IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window2);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowIsPresent(ILayoutEngine inner, IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1).AddWindow(window2);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowIsNotPresent(ILayoutEngine inner, IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window2);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_WindowIsPresent(ILayoutEngine inner, IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1);

		// When
		layoutEngine.MoveWindowToPoint(window1, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_WindowIsNotPresent(ILayoutEngine inner, IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.MoveWindowToPoint(window1, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_WindowIsPresent(ILayoutEngine inner, IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_WindowIsNotPresent(ILayoutEngine inner, IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void PerformCustomAction_WindowIsPresent(ILayoutEngine inner, IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine(inner).AddWindow(window1);

		// When
		layoutEngine.PerformCustomAction("action", 1, null);

		// Then it shouldn't throw.
	}
}
