namespace Whim.FloatingLayout;

/// <summary>
/// A proxy layout engine to allow windows to be free-floating.
/// </summary>
public interface IFloatingLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window.
	/// </summary>
	/// <param name="window"></param>
	void MarkWindowAsFloating(IWindow? window = null);

	/// <summary>
	/// Mark the given <paramref name="window"/> as a docked window.
	/// </summary>
	/// <param name="window"></param>
	void MarkWindowAsDocked(IWindow? window = null);

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	void ToggleWindowFloating(IWindow? window = null);

	/// <summary>
	/// Updates the internally stored location of the given <paramref name="window"/>.
	/// If the window is not floating, the window is ignored.
	/// </summary>
	/// <param name="window"></param>
	void UpdateWindowLocation(IWindow window);
}
