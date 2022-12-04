using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class TestMonitor
{
	public static IEnumerable<object[]> IntToUnitSquareData()
	{
		yield return new object[]
		{
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = 108 },
			new Point<double>() { X = 0.1, Y = 0.1 }
		};
		yield return new object[]
		{
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 960, Y = 270 },
			new Point<double>() { X = 0.5, Y = 0.25 }
		};
		yield return new object[]
		{
			new Location<int>()
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
			new Location<int>()
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
			new Location<int>()
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
			new Location<int>()
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
	public void IntToUnitSquareTheory(ILocation<int> monitor, IPoint<int> point, IPoint<double> expected)
	{
		// When
		IPoint<double> actual = monitor.ToUnitSquare(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> DoubleToUnitSquareData()
	{
		yield return new object[]
		{
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			new Location<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Location<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			}
		};
		yield return new object[]
		{
			new Location<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Location<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Location<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			}
		};
		yield return new object[]
		{
			new Location<int>()
			{
				X = -100,
				Y = -100,
				Width = 1920,
				Height = 1080
			},
			new Location<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			},
			new Location<double>()
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
	public void DoubleToUnitSquareTheory(ILocation<int> monitor, ILocation<int> location, ILocation<double> expected)
	{
		// When
		ILocation<double> actual = monitor.ToUnitSquare(location);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> ToMonitorData()
	{
		yield return new object[]
		{
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			new Location<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			},
			new Location<int>()
			{
				X = 192,
				Y = 108,
				Width = 192,
				Height = 108
			}
		};
		yield return new object[]
		{
			new Location<int>()
			{
				X = 100,
				Y = 100,
				Width = 1920,
				Height = 1080
			},
			new Location<double>()
			{
				X = 0.1,
				Y = 0.1,
				Width = 0.1,
				Height = 0.1
			},
			new Location<int>()
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
	public void ToMonitorTheory(ILocation<int> monitor, ILocation<double> location, ILocation<int> expected)
	{
		// When
		ILocation<int> actual = monitor.ToMonitor(location);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}

	public static IEnumerable<object[]> ToMonitorCoordinatesData()
	{
		yield return new object[]
		{
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			new Point<int>() { X = 192, Y = 108 },
			new Point<int>() { X = 192, Y = 108 }
		};
		yield return new object[]
		{
			new Location<int>()
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
	public void ToMonitorCoordinatesTheory(ILocation<int> monitor, IPoint<int> point, IPoint<int> expected)
	{
		// When
		IPoint<int> actual = monitor.ToMonitorCoordinates(point);

		// Then
		Assert.Equal(expected.X, actual.X);
		Assert.Equal(expected.Y, actual.Y);
	}
}
