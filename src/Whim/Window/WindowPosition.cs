namespace Whim;

/// <summary>
/// A window's position at a point in time.
/// </summary>
public record WindowPosition
{
	/// <summary>
	/// Whether the window is minimized, maximized, or normal.
	/// </summary>
	public WindowSize WindowSize { get; }

	/// <summary>
	/// The last rectangle of the window's position and dimensions.
	/// </summary>
	public IRectangle<int> LastWindowRectangle { get; }

	/// <summary>
	/// Creates a window's position. Defaults to <see cref="Whim.WindowSize.Minimized"/> and
	/// a <see cref="IRectangle{T}"/> with all zero values.
	/// </summary>
	public WindowPosition()
	{
		WindowSize = WindowSize.Minimized;
		LastWindowRectangle = new Rectangle<int>();
	}

	/// <summary>
	/// Creates a record of a window's position.
	/// </summary>
	/// <param name="windowSize"></param>
	/// <param name="lastWindowRectangle"></param>
	public WindowPosition(WindowSize windowSize, IRectangle<int> lastWindowRectangle)
	{
		WindowSize = windowSize;
		LastWindowRectangle = lastWindowRectangle;
	}
}
