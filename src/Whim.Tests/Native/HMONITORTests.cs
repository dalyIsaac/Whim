using Windows.Win32.Graphics.Gdi;
using Xunit;

namespace Whim.Tests;

public class HMONITORTests
{
	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Method(int value1, int value2, bool expected)
	{
		// Given
		HMONITOR hmonitor1 = new(value1);
		HMONITOR hmonitor2 = new(value2);

		// When
		bool result = hmonitor1.Equals(hmonitor2);

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HMONITOR hmonitor1 = new(value1);
		HMONITOR hmonitor2 = new(value2);

		// When
		bool result = hmonitor1 == hmonitor2;

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, false)]
	[InlineData(1, 2, true)]
	public void NotEquals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HMONITOR hmonitor1 = new(value1);
		HMONITOR hmonitor2 = new(value2);

		// When
		bool result = hmonitor1 != hmonitor2;

		// Then
		Assert.Equal(expected, result);
	}

	[Fact]
	public void ToString_Hex()
	{
		// Given
		HMONITOR hmonitor = new(0x12345);

		// When
		string result = hmonitor.ToString();

		// Then
		Assert.Equal("0x00012345", result);
	}
}
