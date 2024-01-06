using System;
using System.Diagnostics;

namespace Whim;

/// <summary>
/// Represents a single display device.
/// </summary>
public interface IMonitor
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

	/// <summary>
	/// The working area of this monitor.
	/// </summary>
	/// <remarks>
	/// The working area is the desktop area of the display, excluding taskbars,
	/// docked windows, and docked tool bars.
	/// </remarks>
	public IRectangle<int> WorkingArea { get; }

	/// <summary>
	/// The bounds of the monitor.
	/// </summary>
	public IRectangle<int> Bounds { get; }
}

/// <summary>
/// Helper methods for converting between the coordinate systems.
/// </summary>
public static class MonitorHelpers
{
	/// <summary>
	/// Converts the <paramref name="point"/> from the system's coordinate system to the
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
	public static IPoint<int> ToMonitorCoordinates(this IRectangle<int> monitor, IPoint<int> point)
	{
		return new Point<int>() { X = point.X - monitor.X, Y = point.Y - monitor.Y };
	}

	/// <summary>
	/// Converts the <paramref name="point"/> from the <paramref name="monitor"/>'s coordinate
	/// system to the unit square.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="point">The point to translate.</param>
	/// <param name="respectSign">
	/// Whether to respect the sign. For example, values in [-infinity, 0] will become [-1, 0].
	/// </param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static IPoint<double> ToUnitSquare(this IRectangle<int> monitor, IPoint<int> point, bool respectSign = false)
	{
		Debug.Assert(monitor.Width != 0);
		Debug.Assert(monitor.Height != 0);

		int translatedX = point.X - monitor.X;
		int translatedY = point.Y - monitor.Y;

		double x = (double)translatedX / monitor.Width;
		double y = (double)translatedY / monitor.Height;

		return respectSign
			? new Point<double>() { X = x, Y = y }
			: new Point<double>() { X = Math.Abs(x), Y = Math.Abs(y) };
	}

	/// <summary>
	/// Converts the <paramref name="rectangle"/> from the unit square to the
	/// <paramref name="monitor"/>'s coordinate system.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="rectangle">The point to translate.</param>
	/// <returns>The converted point, where x and y are in the range [0, 1].</returns>
	public static IRectangle<double> ToUnitSquare(this IRectangle<int> monitor, IRectangle<int> rectangle)
	{
		Debug.Assert(monitor.Width != 0);
		Debug.Assert(monitor.Height != 0);

		int translatedX = rectangle.X - monitor.X;
		int translatedY = rectangle.Y - monitor.Y;

		double x = Math.Abs((double)translatedX / monitor.Width);
		double y = Math.Abs((double)translatedY / monitor.Height);
		double width = Math.Abs((double)rectangle.Width / monitor.Width);
		double height = Math.Abs((double)rectangle.Height / monitor.Height);
		return new Rectangle<double>()
		{
			X = x,
			Y = y,
			Width = width,
			Height = height
		};
	}

	/// <summary>
	/// Converts the <paramref name="rectangle"/> from the unit square to the
	/// <paramref name="monitor"/>'s coordinate system.
	/// </summary>
	/// <param name="monitor"></param>
	/// <param name="rectangle">The rectangle to translate.</param>
	/// <returns>The converted rectangle.</returns>
	public static IRectangle<int> ToMonitor(this IRectangle<int> monitor, IRectangle<double> rectangle)
	{
		return new Rectangle<int>()
		{
			X = Math.Abs(Convert.ToInt32(monitor.X + (rectangle.X * monitor.Width))),
			Y = Math.Abs(Convert.ToInt32(monitor.Y + (rectangle.Y * monitor.Height))),
			Width = Math.Abs(Convert.ToInt32(rectangle.Width * monitor.Width)),
			Height = Math.Abs(Convert.ToInt32(rectangle.Height * monitor.Height))
		};
	}
}
