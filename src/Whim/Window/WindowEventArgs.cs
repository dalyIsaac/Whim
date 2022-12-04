using System;

namespace Whim;

/// <summary>
/// Event arguments for when a <see cref="IWindow"/> has had something happen to it.
/// </summary>
public class WindowEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that had something happen to it.
	/// </summary>
	public required IWindow Window { get; init; }
}
