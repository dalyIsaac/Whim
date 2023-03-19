using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal class MonitorEnumCallback
{
	public List<HMONITOR> Monitors { get; } = new();

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
	public unsafe BOOL Callback(HMONITOR monitor, HDC _hdc, RECT* _rect, LPARAM _param)
	{
		Monitors.Add(monitor);
		return (BOOL)true;
	}
}
