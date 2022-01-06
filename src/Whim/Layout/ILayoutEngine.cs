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
	public IEnumerable<IWindowLocation> DoLayout(ILocation location);

	/// <summary>
	/// Retrieves the first window in the layout engine.
	/// </summary>
	public IWindow? GetFirstWindow();
}
