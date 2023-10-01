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

	[Theory, AutoSubstituteData]
	public void AddWindow_NewWindow(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		ILayoutEngine result = layoutEngine.AddWindow(window);

		// Then
		Xunit.Assert.NotSame(layoutEngine, result);
		Xunit.Assert.True(result.ContainsWindow(window));
	}

	[Theory, AutoSubstituteData]
	public void AddWindow_WindowAlreadyIncluded(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window);

		// When
		ILayoutEngine result = layoutEngine.AddWindow(window);

		// Then
		Xunit.Assert.Same(layoutEngine, result);
		Xunit.Assert.True(result.ContainsWindow(window));
		Xunit.Assert.Equal(1, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowIsPresent(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window);

		// When
		ILayoutEngine result = layoutEngine.RemoveWindow(window);

		// Then
		Xunit.Assert.NotSame(layoutEngine, result);
		Xunit.Assert.False(result.ContainsWindow(window));
		Xunit.Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void RemoveWindow_WindowIsNotPresent(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		ILayoutEngine result = layoutEngine.RemoveWindow(window);

		// Then
		Xunit.Assert.Same(layoutEngine, result);
		Xunit.Assert.False(result.ContainsWindow(window));
		Xunit.Assert.Equal(0, result.Count);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_WindowIsPresent(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window);

		// When
		bool result = layoutEngine.ContainsWindow(window);

		// Then
		Xunit.Assert.True(result);
	}

	[Theory, AutoSubstituteData]
	public void ContainsWindow_WindowIsNotPresent(IWindow window)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		bool result = layoutEngine.ContainsWindow(window);

		// Then
		Xunit.Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowIsPresent(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1).AddWindow(window2);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void SwapWindowInDirection_WindowIsNotPresent(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1);

		// When
		layoutEngine.SwapWindowInDirection(Direction.Right, window2);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowIsPresent(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1).AddWindow(window2);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void FocusWindowInDirection_WindowIsNotPresent(IWindow window1, IWindow window2)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1);

		// When
		layoutEngine.FocusWindowInDirection(Direction.Right, window2);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_WindowIsPresent(IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1);

		// When
		layoutEngine.MoveWindowToPoint(window1, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowToPoint_WindowIsNotPresent(IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		layoutEngine.MoveWindowToPoint(window1, new Point<double>(1, 1));

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_WindowIsPresent(IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine().AddWindow(window1);

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1);

		// Then it shouldn't throw.
	}

	[Theory, AutoSubstituteData]
	public void MoveWindowEdgesInDirection_WindowIsNotPresent(IWindow window1)
	{
		// Given
		ILayoutEngine layoutEngine = CreateLayoutEngine();

		// When
		layoutEngine.MoveWindowEdgesInDirection(Direction.LeftDown, new Point<double>(1, 1), window1);

		// Then it shouldn't throw.
	}
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
