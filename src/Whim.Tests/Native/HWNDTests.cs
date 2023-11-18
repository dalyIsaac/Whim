using System;
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

	[Fact]
	public void Equals_Method_Object()
	{
		// Given
		HWND hwnd = new(1);
		object obj = "1";

		// When
		bool result = hwnd.Equals(obj);

		// Then
		Assert.False(result);
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

	[Fact]
	public void ToString_Hex()
	{
		// Given
		HWND hwnd = new(0x12345);

		// When
		string result = hwnd.ToString();

		// Then
		Assert.Equal("0x00012345", result);
	}

	[Fact]
	public void CreateFromIntPtr()
	{
		// Given
		HWND hwnd = (HWND)new IntPtr(0x12345);

		// When
		string result = hwnd.ToString();

		// Then
		Assert.Equal("0x00012345", result);
	}
}
