using System.Collections.Generic;

namespace Whim.Core.Layout;

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
	/// Calculate the layout of the workspace, for the given windows.
	/// </summary>
	/// <param name="windows">Windows to be organized</param>
	/// <param name="spaceWidth">Width of the available space</param>
	/// <param name="spaceHeight">Height of the available space</param>
	/// <returns></returns>
	public IEnumerable<ILocation> CalcLayout(IEnumerable<IWindow> windows, int spaceWidth, int spaceHeight);
}
