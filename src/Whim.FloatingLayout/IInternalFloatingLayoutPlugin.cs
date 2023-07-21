using System.Collections.Generic;

namespace Whim.FloatingLayout;

internal interface IInternalFloatingLayoutPlugin
{
	ISet<IWindow> MutableFloatingWindows { get; }
}
