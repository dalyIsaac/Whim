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
	/// <returns>The translated point, where x and y are in the range [0, 1].</returns>
	public static IPoint<double> ToUnitSquare(this IMonitor monitor, IPoint<int> point)
	{
		var x = (double)point.X / monitor.Width;
		var y = (double)point.Y / monitor.Height;
		return new Point<double>(x, y);
	}
}

