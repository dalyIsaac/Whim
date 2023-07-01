using System.Collections.Generic;

namespace Whim.FloatingLayout;

internal interface IInternalFloatingLayoutPlugin
{
	IDictionary<IWindow, IWorkspace> MutableFloatingWindows { get; }
}
