using System;
using System.Collections.Generic;

namespace Whim;

public class MonitorEventArgs : EventArgs
{
	public IEnumerable<IMonitor> PreviousMonitors { get; private set; }
	public IEnumerable<IMonitor> CurrentMonitors { get; private set; }

	public MonitorEventArgs(IEnumerable<IMonitor> previousMonitors, IEnumerable<IMonitor> currentMonitors)
	{
		PreviousMonitors = previousMonitors;
		CurrentMonitors = currentMonitors;
	}
}
