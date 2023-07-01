using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <summary>
/// FloatingLayoutPlugin lets windows escape the layout engine and be free-floating.
/// </summary>
public interface IFloatingLayoutPlugin : IPlugin
{
	/// <summary>
	/// All the floating windows.
	/// </summary>
	IReadOnlyDictionary<IWindow, IWorkspace> FloatingWindows { get; }

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow? window = null);

	/// <summary>
	/// Update the floating window location.
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow? window = null);

	/// <summary>
	/// Toggle the floating state of the given <paramref name="window"/>.
	/// </summary>
	/// <param name="window"></param>
	public void ToggleWindowFloating(IWindow? window = null);
}
