using System.Collections.Generic;
using Xunit;

namespace Whim.Tests;

public class TestMonitor
{
	public static IEnumerable<object[]> IntToUnitSquareData()
	{
		yield return new object[]
		{
			new Location<int>(0, 0, 1920, 1080),
			new Point<int>(192, 108),
			new Point<double>(0.1, 0.1)
		};
		yield return new object[]
		{
			new Location<int>(0, 0, 1920, 1080),
			new Point<int>(960, 270),
			new Point<double>(0.5, 0.25)
		};
		yield return new object[]
		{
			new Location<int>(100, 100, 1920, 1080),
			new Point<int>(192, 108),
			new Point<double>(0.1, 0.1)
		};
		yield return new object[]
		{
			new Location<int>(100, 100, 1920, 1080),
			new Point<int>(960, 270),
			new Point<double>(0.5, 0.25)
		};
		yield return new object[]
		{
			new Location<int>(-100, 100, 1920, 1080),
			new Point<int>(192, 108),
			new Point<double>(0.1, 0.1)
		};
		yield return new object[]
		{
			new Location<int>(-100, 100, 1920, 1080),
			new Point<int>(960, 270),
			new Point<double>(0.5, 0.25)
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
			new Location<int>(0, 0, 1920, 1080),
			new Location<int>(192, 108, 192, 108),
			new Location<double>(0.1, 0.1, 0.1, 0.1)
		};
		yield return new object[]
		{
			new Location<int>(100, 100, 1920, 1080),
			new Location<int>(192, 108, 192, 108),
			new Location<double>(0.1, 0.1, 0.1, 0.1)
		};
		yield return new object[]
		{
			new Location<int>(-100, -100, 1920, 1080),
			new Location<int>(192, 108, 192, 108),
			new Location<double>(0.1, 0.1, 0.1, 0.1)
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
			new Location<int>(0, 0, 1920, 1080),
			new Location<double>(0.1, 0.1, 0.1, 0.1),
			new Location<int>(192, 108, 192, 108)
		};
		yield return new object[]
		{
			new Location<int>(100, 100, 1920, 1080),
			new Location<double>(0.1, 0.1, 0.1, 0.1),
			new Location<int>(192 + 100, 108 + 100, 192, 108)
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
			new Location<int>(0, 0, 1920, 1080),
			new Point<int>(192, 108),
			new Point<int>(192, 108)
		};
		yield return new object[]
		{
			new Location<int>(100, 100, 1920, 1080),
			new Point<int>(192, 108),
			new Point<int>(92, 8)
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
