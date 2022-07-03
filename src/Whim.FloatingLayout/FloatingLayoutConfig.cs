using System.Collections.Generic;

namespace Whim.FloatingLayout;

/// <summary>
/// Configuration for the floating layout plugin.
/// </summary>
public class FloatingLayoutConfig
{
	private readonly HashSet<IWindow> _floatingWindows = new();

	/// <summary>
	/// Mark the given <paramref name="window"/> as a floating window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsFloating(IWindow window)
	{
		Logger.Debug($"Marking window {window} as floating");
		_floatingWindows.Add(window);
	}

	/// <summary>
	/// Mark the given <paramref name="window"/> as a docked window
	/// </summary>
	/// <param name="window"></param>
	public void MarkWindowAsDocked(IWindow window)
	{
		Logger.Debug($"Marking window {window} as docked");
		_floatingWindows.Remove(window);
	}

	/// <summary>
	/// Indicate if the given <paramref name="window"/> is a floating window.
	/// </summary>
	/// <param name="window"></param>
	/// <returns></returns>
	public bool IsWindowFloating(IWindow window) => _floatingWindows.Contains(window);
}
