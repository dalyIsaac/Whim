using System;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

/// <summary>
/// A replacement for <see cref="Microsoft.Win32.SystemEvents"/>, where we can subscribe to
/// <c>WM_MESSAGE</c> events.
/// </summary>
internal interface IWindowMessageMonitor : IDisposable
{
	/// <summary>
	/// The <see cref="PInvoke.WM_DISPLAYCHANGE"/> message is sent to all windows when the display
	/// resolution has changed for a monitor.
	/// </summary>
	public event EventHandler<WindowMessageMonitorEventArgs>? DisplayChanged;

	/// <summary>
	/// This event is raised when the work area has changed for a monitor.
	/// </summary>
	/// <remarks>
	/// The <see cref="SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETWORKAREA"/> is as the <c>uiAction</c>
	/// parameter of the <see cref="PInvoke.SystemParametersInfo"/> function. Whim detects this
	/// message by listening for the <see cref="PInvoke.WM_SETTINGCHANGE"/> message.
	/// </remarks>
	public event EventHandler<WindowMessageMonitorEventArgs>? WorkAreaChanged;

	/// <summary>
	/// This event is raised when the DPI has changed for a monitor.
	/// </summary>
	/// <remarks>
	/// The <see cref="SYSTEM_PARAMETERS_INFO_ACTION.SPI_SETLOGICALDPIOVERRIDE"/> is as the <c>uiAction</c>
	/// parameter of the <see cref="PInvoke.SystemParametersInfo"/> function. Whim detects this
	/// message by listening for the <see cref="PInvoke.WM_SETTINGCHANGE"/> message.
	/// </remarks>
	public event EventHandler<WindowMessageMonitorEventArgs>? DpiChanged;
}
