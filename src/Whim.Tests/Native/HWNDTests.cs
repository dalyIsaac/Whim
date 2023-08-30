using Windows.Win32.Foundation;
using Xunit;

namespace Whim.Tests;

public class HWNDTests
{
	[Fact]
	public void Null()
	{
		// Given
		HWND hwnd = HWND.Null;

		// When
		bool result = hwnd.IsNull;

		// Then
		Assert.True(result);
		Assert.Equal(0, hwnd.Value);
	}

	[Fact]
	public void NotNull()
	{
		// Given
		HWND hwnd = new(1);

		// When
		bool result = hwnd.IsNull;

		// Then
		Assert.False(result);
		Assert.Equal(1, hwnd.Value);
	}

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Method(int value1, int value2, bool expected)
	{
		// Given
		HWND hwnd1 = new(value1);
		HWND hwnd2 = new(value2);

		// When
		bool result = hwnd1.Equals(hwnd2);

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HWND hwnd1 = new(value1);
		HWND hwnd2 = new(value2);

		// When
		bool result = hwnd1 == hwnd2;

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, false)]
	[InlineData(1, 2, true)]
	public void NotEquals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HWND hwnd1 = new(value1);
		HWND hwnd2 = new(value2);

		// When
		bool result = hwnd1 != hwnd2;

		// Then
		Assert.Equal(expected, result);
	}
}
