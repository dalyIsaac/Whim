using Xunit;

namespace Whim.LayoutPreview.Tests;

public class NonNegativeValueConverterTests
{
	[InlineData(-1, 0)]
	[InlineData(0, 0)]
	[InlineData(1, 1)]
	[Theory]
	public void Convert_Int(int input, int expected)
	{
		// Given
		NonNegativeValueConverter converter = new();

		// When
		int actual = (int)converter.Convert(input, typeof(int), new object(), "")!;

		// Then
		Assert.Equal(expected, actual);
	}

	[InlineData(-1.0, 0)]
	[InlineData(0.0, 0)]
	[InlineData(1.0, 1)]
	[Theory]
	public void Convert_Double(double input, double expected)
	{
		// Given
		NonNegativeValueConverter converter = new();

		// When
		double actual = (double)converter.Convert(input, typeof(double), new object(), "")!;

		// Then
		Assert.Equal(expected, actual);
	}

	[Fact]
	public void ConvertBack()
	{
		// Given
		NonNegativeValueConverter converter = new();

		// When
		object actual()
		{
			return converter.ConvertBack(0, typeof(int), new object(), "");
		}

		// Then
		Assert.Throws<NotImplementedException>(actual);
	}
}
