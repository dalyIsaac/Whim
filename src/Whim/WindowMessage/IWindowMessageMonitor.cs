using System;
using Windows.Win32;

namespace Whim;

/// <summary>
/// A replacement for <see cref="Microsoft.Win32.SystemEvents"/>, where we can subscribe to
/// <c>WM_MESSAGE</c> events.
/// </summary>
internal interface IWindowMessageMonitor : IDisposable
{
	/// <summary>
	/// The <see cref="PInvoke.WM_DISPLAYCHANGE"/> message is sent to all windows when the display resolution has changed.
	/// </summary>
	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChange;
}
