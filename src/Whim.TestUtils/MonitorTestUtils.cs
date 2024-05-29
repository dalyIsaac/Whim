using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using NSubstitute;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;

namespace Whim;

internal static class MonitorTestUtils
{
	public static void SetupMultipleMonitors(IInternalContext internalCtx, (RECT Rect, HMONITOR HMonitor)[] monitors)
	{
		// Get the primary rect.
		RECT[] potentialPrimaryMonitors = monitors.Select(m => m.Rect).Where(r => r.left == 0 && r.top == 0).ToArray();
		if (potentialPrimaryMonitors.Length != 1)
		{
			throw new Exception("No primary monitor found");
		}
		RECT primaryRect = potentialPrimaryMonitors[0];

		// Set up monitor creation.
		foreach ((RECT rect, HMONITOR hMonitor) in monitors)
		{
			// Set up the primary monitor, if it's the primary monitor.
			if (primaryRect.Equals(rect))
			{
				internalCtx
					.CoreNativeManager.MonitorFromPoint(new Point(0, 0), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY)
					.Returns(hMonitor);
			}

			// Set up the fetching of metadata in a multiple monitor setup.
			internalCtx
				.CoreNativeManager.GetMonitorInfoEx(hMonitor)
				.Returns(
					new MONITORINFOEXW()
					{
						monitorInfo = new MONITORINFO()
						{
							cbSize = (uint)Marshal.SizeOf<MONITORINFO>(),
							rcMonitor = rect,
							rcWork = rect,
							dwFlags = 0
						},
						szDevice = $"DISPLAY {(int)hMonitor}"
					}
				);
		}

		// Set up multiple monitors.
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(monitors.Length > 1);
		internalCtx
			.CoreNativeManager.EnumDisplayMonitors(
				Arg.Any<SafeHandle>(),
				Arg.Any<RECT?>(),
				Arg.Any<MONITORENUMPROC>(),
				Arg.Any<LPARAM>()
			)
			.Returns(
				(callInfo) =>
				{
					foreach ((RECT rect, HMONITOR hMonitor) in monitors)
					{
						unsafe
						{
							callInfo.ArgAt<MONITORENUMPROC>(2).Invoke(hMonitor, (HDC)0, &rect, (LPARAM)0);
						}
					}

					return (BOOL)true;
				}
			);
	}
}
