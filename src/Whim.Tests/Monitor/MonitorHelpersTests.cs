using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class MonitorHelpersTests
{
	public static IEnumerable<object[]> IntToUnitSquareData()
	{
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Point<int>() { X = 192, Y = 108 },
			new Point<double>() { X = 0.1, Y = 0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Point<int>() { X = 960, Y = 270 },
			new Point<double>() { X = 0.5, Y = 0.25 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = 108 },
			new Point<double>() { X = 0.1, Y = 0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 960, Y = 270 },
			new Point<double>() { X = 0.5, Y = 0.25 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = -100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = 108 },
			new Point<double>() { X = 0.1, Y = 0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = -100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 960, Y = 270 },
			new Point<double>() { X = 0.5, Y = 0.25 }
		};
	}

	[Theory]
	[MemberData(nameof(IntToUnitSquareData))]
	public void ToUnitSquarePointTheory(IRectangle<int> monitor, IPoint<int> point, IPoint<double> expected)
	{
		// When
		IPoint<double> actual = monitor.ToUnitSquare(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> IntToUnitSquareRespectSignData()
	{
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Point<int>() { X = -192, Y = 108 },
			new Point<double>() { X = -0.1, Y = 0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Point<int>() { X = 960, Y = -270 },
			new Point<double>() { X = 0.5, Y = -0.25 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = -192, Y = -108 },
			new Point<double>() { X = -0.1, Y = -0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = -960, Y = 270 },
			new Point<double>() { X = -0.5, Y = 0.25 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = -100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = -108 },
			new Point<double>() { X = 0.1, Y = -0.1 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = -100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = -960, Y = -270 },
			new Point<double>() { X = -0.5, Y = -0.25 }
		};
	}

	[Theory]
	[MemberData(nameof(IntToUnitSquareRespectSignData))]
	public void ToUnitSquarePointRespectSignTheory(IRectangle<int> monitor, IPoint<int> point, IPoint<double> expected)
	{
		// When
		IPoint<double> actual = monitor.ToUnitSquare(point, respectSign: true);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> DoubleToUnitSquareData()
	{
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Rectangle<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Rectangle<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			}
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Rectangle<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Rectangle<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			}
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = -100,
				Y = -100,
				Width = 1920,
				Height = 1080
			},
			new Rectangle<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Rectangle<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			}
		};
	}

	[Theory]
	[MemberData(nameof(DoubleToUnitSquareData))]
	public void ToUnitSquareRectangleTheory(IRectangle<int> monitor, IRectangle<int> rect, IRectangle<double> expected)
	{
		// When
		IRectangle<double> actual = monitor.ToUnitSquare(rect);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> ToMonitorData()
	{
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Rectangle<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			},
			new Rectangle<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			}
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Rectangle<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			},
			new Rectangle<int>()
			{
				X = 192 + 100,
				Y = 108 + 100,
				Width = 192,
				Height = 108
			}
		};
	}

	[Theory]
	[MemberData(nameof(ToMonitorData))]
	public void ToMonitorTheory(IRectangle<int> monitor, IRectangle<double> rect, IRectangle<int> expected)
	{
		// When
		IRectangle<int> actual = monitor.ToMonitor(rect);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> ToMonitorCoordinatesData()
	{
		yield return new object[]
		{
			new Rectangle<int>() { Width = 1920, Height = 1080 },
			new Point<int>() { X = 192, Y = 108 },
			new Point<int>() { X = 192, Y = 108 }
		};
		yield return new object[]
		{
			new Rectangle<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = 108 },
			new Point<int>() { X = 92, Y = 8 }
		};
	}

	[Theory]
	[MemberData(nameof(ToMonitorCoordinatesData))]
	public void ToMonitorCoordinatesTheory(IRectangle<int> monitor, IPoint<int> point, IPoint<int> expected)
	{
		// When
		IPoint<int> actual = monitor.ToMonitorCoordinates(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}
}
