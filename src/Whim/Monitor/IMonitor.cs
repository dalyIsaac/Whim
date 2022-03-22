using System;

namespace Whim;

/// <summary>
/// Represents a single display device.
/// </summary>
public interface IMonitor : ILocation<int>
{
	/// <summary>
	/// The name of the monitor.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// <see langword="true"/> if the monitor is the primary monitor.
	/// </summary>
	public bool IsPrimary { get; }
}

public static class MonitorHelpers
{
	/// <summary>
	/// Translate the <paramref name="point"/> from the <see cref="IMonitor"/>'s coordinate system to the
	/// unit square.
	/// </summary>
	/// <param name="point">The point to translate.</param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static IPoint<double> ToUnitSquare(this ILocation<int> monitor, IPoint<int> point)
	{
		double x = (double)point.X / monitor.Width;
		double y = (double)point.Y / monitor.Height;
		return new Point<double>(x, y);
	}

	/// <summary>
	/// Translate the <paramref name="point"/> from the unit square to the <see cref="IMonitor"/>'s coordinate system.
	/// </summary>
	/// <param name="point">The point to translate.</param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static ILocation<double> ToUnitSquare(this ILocation<int> monitor, ILocation<int> location)
	{
		double x = (double)location.X / monitor.Width;
		double y = (double)location.Y / monitor.Height;
		double width = (double)location.Width / monitor.Width;
		double height = (double)location.Height / monitor.Height;
		return new DoubleLocation(x, y, width, height);
	}

	/// <summary>
	/// Translate the <paramref name="location"/> from the unit square to the <see cref="IMonitor"/>'s coordinate system.
	/// </summary>
	/// <param name="location">The location to translate.</param>
	/// <returns>The converted location.</returns>
	public static ILocation<int> ToMonitor(this ILocation<int> monitor, ILocation<double> location)
	{
		return new Location(
			x: Convert.ToInt32(monitor.X + location.X * monitor.Width),
			y: Convert.ToInt32(monitor.Y + location.Y * monitor.Height),
			width: Convert.ToInt32(location.Width * monitor.Width),
			height: Convert.ToInt32(location.Height * monitor.Height)
		);
	}
}

