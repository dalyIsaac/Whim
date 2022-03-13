using System.Collections.Generic;

namespace Whim;

/// <summary>
/// Layout engines dictate how windows are laid out.
/// </summary>
public interface ILayoutEngine : ICollection<IWindow>, ICommandable
{
	/// <summary>
	/// The name of the layout engine.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Performs a layout inside the available <see cref="ILocation"/> region.
	/// </summary>
	/// <param name="location">The available area to do a layout inside.</param>
	/// <returns></returns>
	public IEnumerable<IWindowLocation> DoLayout(ILocation<int> location);

	/// <summary>
	/// Retrieves the first window in the layout engine.
	/// </summary>
	public IWindow? GetFirstWindow();

	/// <summary>
	/// Focuses the <see paramref="window"/> in the <see paramref="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through the <see cref="IWorkspace.FocusWindowInDirection"/> method.
	/// </summary>
	/// <param name="direction">The direction to focus in.</param>
	/// <param name="window">The origin window</param>
	public void FocusWindowInDirection(WindowDirection direction, IWindow window);

	/// <summary>
	/// Swaps the <see paramref="window"/> in the <see paramref="direction"/>.
	/// It's recommended that this method is not called directly, but rather
	/// through <see cref="IWorkspace.SwapWindowInDirection"/>.
	/// </summary>
	/// <param name="direction">The direction to swap the window in.</param>
	/// <param name="window">The window to swap.</param>
	public void SwapWindowInDirection(WindowDirection direction, IWindow window);
}
