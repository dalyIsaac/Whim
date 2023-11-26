using Whim.TestUtils;
using Xunit;

namespace Whim.Tests;

public class WindowStateTests
{
	[Theory, AutoSubstituteData]
	public void Equals_NotWindowState(IWindow window)
	{
		// Given
		WindowState windowState =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};

		// When
		bool result = windowState.Equals(new object());

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentLocation(IWindow window)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>() { X = 1, Y = 1 },
				WindowSize = WindowSize.Normal,
				Window = window
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentWindowSize(IWindow window)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Maximized,
				Window = window
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Equals_DifferentWindow(IWindow window1, IWindow window2)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window1
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window2
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.False(result);
	}

	[Theory, AutoSubstituteData]
	public void Equals_Success(IWindow window)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};

		// When
		bool result = windowState1.Equals(windowState2);

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData]
	public void Equals_Operator_Success(IWindow window)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window
			};

		// When
		bool result = windowState1 == windowState2;

		// Then
		Assert.True(result);
	}

	[Theory, AutoSubstituteData]
	public void NotEquals_Operator_Success(IWindow window1, IWindow window2)
	{
		// Given
		WindowState windowState1 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window1
			};
		WindowState windowState2 =
			new()
			{
				Rectangle = new Rectangle<int>(),
				WindowSize = WindowSize.Normal,
				Window = window2
			};

		// When
		bool result = windowState1 != windowState2;

		// Then
		Assert.True(result);
	}
}
