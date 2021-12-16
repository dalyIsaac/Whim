using System.Collections.Generic;

namespace Whim.Core;

/// <summary>
/// Layout engines dictate how windows are laid out.
/// </summary>
public interface ILayoutEngine : ICommandable
{
	/// <summary>
	/// The name of the layout engine.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Performs a layout inside the available <see cref="IArea"/> region.
	/// </summary>
	/// <param name="area">The available area to do a layout inside.</param>
	/// <returns></returns>
	public IEnumerable<IWindowLocation> DoLayout(IArea area);

	/// <summary>
	/// Adds the window to the layout engine.
	/// </summary>
	/// <param name="window"></param>
	public void AddWindow(IWindow window);

	/// <summary>
	/// Removes the window from the layout engine.
	/// </summary>
	/// <param name="window"></param>
	public bool RemoveWindow(IWindow window);
}
