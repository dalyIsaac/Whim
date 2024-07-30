using System.Collections.Generic;

namespace Whim.FloatingLayout;

internal interface IInternalProxyFloatingLayoutPlugin
{
	/// <summary>
	/// Mapping of floating windows to the layout engines that they are floating in.
	/// This is not exposed outside of this namespace to prevent mutation of the dictionary and
	/// sets.
	/// </summary>
	IReadOnlyDictionary<IWindow, ISet<LayoutEngineIdentity>> FloatingWindows { get; }

	/// <summary>
	/// Removes the given layout engine from the given window.
	/// </summary>
	/// <param name="window"></param>
	/// <param name="layoutEngineIdentity"></param>
	void MarkWindowAsDockedInLayoutEngine(IWindow window, LayoutEngineIdentity layoutEngineIdentity);
}
