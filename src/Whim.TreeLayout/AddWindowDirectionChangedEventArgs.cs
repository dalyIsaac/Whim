using System;

namespace Whim.TreeLayout;

/// <summary>
/// Event arguments for the <see cref="ITreeLayoutPlugin.AddWindowDirectionChanged"/> event.
/// </summary>
public class AddWindowDirectionChangedEventArgs : EventArgs
{
	/// <summary>
	/// The tree layout engine for which the direction in which to add new windows has changed.
	/// </summary>
	public required ILayoutEngine TreeLayoutEngine { get; init; }

	/// <summary>
	/// The direction in which to add new windows.
	/// </summary>
	public required Direction CurrentDirection { get; init; }

	/// <summary>
	/// The previous direction in which to add new windows.
	/// </summary>
	public required Direction PreviousDirection { get; init; }
}
