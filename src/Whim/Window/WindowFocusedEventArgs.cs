using System;

namespace Whim;

/// <summary>
/// Event arguments for when the focused window has changed. When <see cref="Window"/> is
/// <see langword="null"/>, then Whim does not track the focused window.
/// </summary>
public class WindowFocusedEventArgs : EventArgs
{
	/// <summary>
	/// The <see cref="IWindow"/> that is now focused. If <see langword="null"/>, then Whim does not
	/// track the focused window.
	/// </summary>
	public IWindow? Window { get; init; }
}
