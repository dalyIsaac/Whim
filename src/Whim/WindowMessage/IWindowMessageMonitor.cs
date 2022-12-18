using System;
using Windows.Win32;

namespace Whim;

internal interface IWindowMessageMonitor : IDisposable
{
	/// <summary>
	/// The <see cref="PInvoke.WM_DISPLAYCHANGE"/> message is sent to all windows when the display resolution has changed.
	/// </summary>
	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChange;
}
