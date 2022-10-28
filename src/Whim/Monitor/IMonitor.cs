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

	/// <summary>
	/// The scale factor of this monitor.
	/// </summary>
	public int ScaleFactor { get; }
}

/// <summary>
/// Helper methods for converting between the coordinate systems.
/// </summary>
public static class MonitorHelpers
{
	/// <summary>
	/// Translated the <paramref name="point"/> from the system's coordinate system to the
	/// <paramref name="monitor"/>'s coordinate system.
	/// The <paramref name="monitor"/>'s coordinate system is defined in terms of the monitor's
	/// width and height, <b>not</b> the unit square.
	/// This does not take into account the monitor's scale factor.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="point"></param>
	/// <returns>
	/// The converted point, where x and y are in the range [0, width) and [0, height).
	/// </returns>
	public static IPoint<int> ToMonitorCoordinates(this ILocation<int> monitor, IPoint<int> point)
	{
		return new Point<int>(
			point.X - monitor.X,
			point.Y - monitor.Y
		);
	}

	/// <summary>
	/// Translate the <paramref name="point"/> from the <paramref name="monitor"/>'s coordinate
	/// system to the unit square.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="point">The point to translate.</param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static IPoint<double> ToUnitSquare(this ILocation<int> monitor, IPoint<int> point)
	{
		double x = Math.Abs((double)point.X / monitor.Width);
		double y = Math.Abs((double)point.Y / monitor.Height);
		return new Point<double>(x, y);
	}

	/// <summary>
	/// Translate the <paramref name="location"/> from the unit square to the
	/// <paramref name="monitor"/>'s coordinate system.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="location">The point to translate.</param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static ILocation<double> ToUnitSquare(this ILocation<int> monitor, ILocation<int> location)
	{
		double x = Math.Abs((double)location.X / monitor.Width);
		double y = Math.Abs((double)location.Y / monitor.Height);
		double width = Math.Abs((double)location.Width / monitor.Width);
		double height = Math.Abs((double)location.Height / monitor.Height);
		return new DoubleLocation(x, y, width, height);
	}

	/// <summary>
	/// Translate the <paramref name="location"/> from the unit square to the
	/// <paramref name="monitor"/>'s coordinate system.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="location">The location to translate.</param>
	/// <returns>The converted location.</returns>
	public static ILocation<int> ToMonitor(this ILocation<int> monitor, ILocation<double> location)
	{
		return new Location(
			x: Math.Abs(Convert.ToInt32(monitor.X + (location.X * monitor.Width))),
			y: Math.Abs(Convert.ToInt32(monitor.Y + (location.Y * monitor.Height))),
			width: Math.Abs(Convert.ToInt32(location.Width * monitor.Width)),
			height: Math.Abs(Convert.ToInt32(location.Height * monitor.Height))
		);
	}
}

