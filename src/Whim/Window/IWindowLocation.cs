namespace Whim.Core;

public interface IWindowLocation
{
	/// <summary>
	/// The dimensions of the bounding rectangle of the specified window.
	/// </summary>
	public ILocation Location { get; set; }

	/// <summary>
	/// The state of the window.
	/// </summary>
	public WindowState WindowState { get; set; }

	/// <summary>
	/// The window in question.
	/// </summary>
	public IWindow Window { get; }
}
