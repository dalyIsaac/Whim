using System;
using System.Collections.Generic;

namespace Whim.FloatingLayout;
public class FloatingLayoutConfig
{
	private readonly Dictionary<IWorkspace, HashSet<IWindowLocation>> _windows = new Dictionary<IWorkspace, HashSet<IWindowLocation>>();

	public void AddWindow(IWorkspace workspace, IWindowLocation windowLocation)
	{
		if (!_windows.TryGetValue(workspace, out HashSet<IWindowLocation>? windows))
		{
			windows = new HashSet<IWindowLocation>();
			_windows.Add(workspace, windows);
		}

		windows.Add(windowLocation);
	}

	public IEnumerable<IWindowLocation> GetWindows(IWorkspace workspace)
	{
		if (_windows.TryGetValue(workspace, out HashSet<IWindowLocation>? windows))
		{
			return windows;
		}

		return new HashSet<IWindowLocation>();
	}
}
