using System;

namespace Whim;

public class MonitorFocusedEventArgs : EventArgs
{
	/// <summary>
	/// The previous focused monitor.
	/// </summary>
	public IMonitor PreviousMonitor { get; private set; }

	/// <summary>
	/// The new focused monitor.
	/// </summary>
	public IMonitor CurrentMonitor { get; private set; }

	public MonitorFocusedEventArgs(IMonitor previousMonitor, IMonitor currentMonitor)
	{
		PreviousMonitor = previousMonitor;
		CurrentMonitor = currentMonitor;
	}
}
