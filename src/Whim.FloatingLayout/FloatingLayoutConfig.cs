using System.Collections.Generic;

namespace Whim.FloatingLayout;
public class FloatingLayoutConfig
{
	private readonly HashSet<IWindow> _floatingWindows = new();

	public void MarkWindowAsFloating(IWindow window)
	{
		Logger.Debug($"Marking window {window} as floating");
		_floatingWindows.Add(window);
	}

	public void MarkWindowAsDocked(IWindow window)
	{
		Logger.Debug($"Marking window {window} as docked");
		_floatingWindows.Remove(window);
	}

	public bool IsWindowFloating(IWindow window) => _floatingWindows.Contains(window);
}
