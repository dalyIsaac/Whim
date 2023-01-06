using Windows.Win32.UI.WindowsAndMessaging;
using Xunit;

namespace Whim.Tests;

public class HDWPTests
{
	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Method(int value1, int value2, bool expected)
	{
		// Given
		HDWP hdwp1 = new(value1);
		HDWP hdwp2 = new(value2);

		// When
		bool result = hdwp1.Equals(hdwp2);

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HDWP hdwp1 = new(value1);
		HDWP hdwp2 = new(value2);

		// When
		bool result = hdwp1 == hdwp2;

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, false)]
	[InlineData(1, 2, true)]
	public void NotEquals_Operator(int value1, int value2, bool expected)
	{
		// Given
		HDWP hdwp1 = new(value1);
		HDWP hdwp2 = new(value2);

		// When
		bool result = hdwp1 != hdwp2;

		// Then
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData(1, 1, true)]
	[InlineData(1, 2, false)]
	public void Equals_Object(int value1, int value2, bool expected)
	{
		// Given
		HDWP hdwp1 = new(value1);
		HDWP hdwp2 = new(value2);

		// When
		bool result = hdwp1.Equals((object)hdwp2);

		// Then
		Assert.Equal(expected, result);
	}
}
