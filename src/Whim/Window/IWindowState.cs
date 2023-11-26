namespace Whim;

/// <summary>
/// The state of a window.
/// </summary>
public interface IWindowState
{
	/// <summary>
	/// The dimensions of the bounding rectangle of the specified window.
	/// </summary>
	public IRectangle<int> Rectangle { get; set; }

	/// <summary>
	/// The state of the window.
	/// </summary>
	public WindowSize WindowSize { get; set; }

	/// <summary>
	/// The window in question.
	/// </summary>
	public IWindow Window { get; }
}
