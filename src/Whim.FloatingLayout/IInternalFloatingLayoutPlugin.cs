using System.Collections.Generic;

namespace Whim.FloatingLayout;

internal interface IInternalFloatingLayoutPlugin
{
	/// <summary>
	/// Mapping of floating windows to the layout engines that they are floating in.
	/// This is not exposed outside of this namespace to prevent mutation of the dictionary and
	/// sets.
	/// </summary>
	IDictionary<IWindow, ISet<LayoutEngineIdentity>> FloatingWindows { get; }
}
