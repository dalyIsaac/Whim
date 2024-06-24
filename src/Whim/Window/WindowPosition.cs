namespace Whim;

/// <summary>
/// A window's position at a point in time.
/// </summary>
/// <param name="WindowSize">
/// Whether the window is minimized, maximized, or normal.
/// </param>
/// <param name="LastWindowRectangle">
/// The last rectangle of the window's position and dimensions.
/// </param>
public record WindowPosition(WindowSize WindowSize, IRectangle<int> LastWindowRectangle)
{
	/// <summary>
	/// Creates a window's position. Defaults to <see cref="Whim.WindowSize.Minimized"/> and
	/// a <see cref="IRectangle{T}"/> with all zero values.
	/// </summary>
	public WindowPosition()
		: this(WindowSize.Minimized, new Rectangle<int>()) { }
}
