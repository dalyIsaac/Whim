using System;

namespace Whim.Bar;

/// <summary>
/// BarPlugin displays an interactive bar at the top of the screen for Whim. It can be configured
/// with various <see cref="BarComponent"/>s to display on the left, center, and right sides of the bar.
/// </summary>
public interface IBarPlugin : IPlugin, IDisposable
{
	/// <summary>
	/// The configuration for the bar.
	/// </summary>
	BarConfig Config { get; }
}
