using Microsoft.Win32;
using System;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal partial class Screen
{
	private readonly HMONITOR _hmonitor;

	private readonly ILocation<int> _bounds;

	private ILocation<int> _workingArea = Location.Empty();

	public readonly bool Primary;

	private readonly int _bitDepth;

	/// <summary>
	///  Instance-based counter used to invalidate <see cref="WorkingArea"/>.
	/// </summary>
	private int _currentDesktopChangedCount = -1;

	/// <summary>
	///  Device name associated with this monitor
	/// </summary>
	public string DeviceName { get; }

	internal Screen(HMONITOR monitor) : this(monitor, default) { }

	internal unsafe Screen(HMONITOR monitor, HDC hdc)
	{
		HDC screenDC = hdc;

		if (!s_multiMonitorSupport || monitor == PRIMARY_MONITOR)
		{
			// Single monitor system.
			_bounds = new Location(
				x: PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_XVIRTUALSCREEN),
				y: PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_YVIRTUALSCREEN),
				width: PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CXVIRTUALSCREEN),
				height: PInvoke.GetSystemMetrics(SYSTEM_METRICS_INDEX.SM_CYVIRTUALSCREEN)
			);

			Primary = true;
			DeviceName = "DISPLAY";
		}
		else
		{
			// Multiple monitor system.
			MONITORINFOEXW info = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
			MonitorInfo.GetMonitorInfo(monitor, ref info);
			_bounds = info.GetLocation();
			Primary = info.IsPrimary();

			DeviceName = info.GetDeviceName();

			if (hdc.IsNull)
			{
				fixed (char* p = DeviceName)
				{
					screenDC = (HDC)PInvoke.CreateDCW((PCWSTR)p, null, null).Value;
				}
			}
		}

		_hmonitor = monitor;

		_bitDepth = PInvoke.GetDeviceCaps(screenDC, GET_DEVICE_CAPS_INDEX.BITSPIXEL);
		_bitDepth *= PInvoke.GetDeviceCaps(screenDC, GET_DEVICE_CAPS_INDEX.PLANES);

		if (hdc != screenDC)
		{
			PInvoke.DeleteDC((CreatedHDC)screenDC.Value);
		}
	}

	public int ScaleFactor
	{
		get
		{
			HRESULT result = PInvoke.GetScaleFactorForMonitor(_hmonitor, out DEVICE_SCALE_FACTOR scaleFactor);
			return result.Succeeded ? (int)scaleFactor : 100;
		}
	}

	/// <summary>
	///  Gets the
	///  primary display.
	/// </summary>
	public static Screen? PrimaryScreen
	{
		get
		{
			if (s_multiMonitorSupport)
			{
				Screen[] screens = AllScreens;
				for (int i = 0; i < screens.Length; i++)
				{
					if (screens[i].Primary)
					{
						return screens[i];
					}
				}

				return null;
			}
			else
			{
				return new Screen(PRIMARY_MONITOR, default);
			}
		}
	}

	/// <summary>
	///  Gets the working area of the screen.
	/// </summary>
	public unsafe ILocation<int> WorkingArea
	{
		get
		{
			// If the static Screen class has a different desktop change count than this instance
			// then update the count and recalculate our working area.
			if (_currentDesktopChangedCount != DesktopChangedCount)
			{
				Interlocked.Exchange(ref _currentDesktopChangedCount, DesktopChangedCount);

				if (!s_multiMonitorSupport || _hmonitor == (IntPtr)PRIMARY_MONITOR)
				{
					// Single monitor system
					unsafe
					{
						RECT rect = new();
						PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA, 0, &rect, 0);
						_workingArea = rect.ToLocation();
					}
				}
				else
				{
					// Multiple monitor System
					MONITORINFO info = new()
					{
						cbSize = (uint)sizeof(MONITORINFO)
					};
					PInvoke.GetMonitorInfo(_hmonitor, ref info);
					_workingArea = info.rcWork.ToLocation();
				}
			}

			return _workingArea;
		}
	}

	/// <summary>
	///  Screen instances call this property to determine
	///  if their WorkingArea cache needs to be invalidated.
	/// </summary>
	private static int DesktopChangedCount
	{
		get
		{
			if (s_desktopChangedCount == -1)
			{
				lock (s_syncLock)
				{
					//now that we have a lock, verify (again) our changecount...
					if (s_desktopChangedCount == -1)
					{
						//sync the UserPreference.Desktop change event.  We'll keep count
						//of desktop changes so that the WorkingArea property on Screen
						//instances know when to invalidate their cache.
						SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

						s_desktopChangedCount = 0;
					}
				}
			}

			return s_desktopChangedCount;
		}
	}

	/// <summary>
	///  Called by the SystemEvents class when our display settings have
	///  changed.  Here, we increment a static counter that Screen instances
	///  can check against to invalidate their cache.
	/// </summary>
	private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
	{
		if (e.Category == UserPreferenceCategory.Desktop)
		{
			Interlocked.Increment(ref s_desktopChangedCount);
		}
	}

	/// <summary>
	///  Specifies a value that indicates whether the specified object is equal to
	///  this one.
	/// </summary>
	public override bool Equals(object? obj)
	{
		if (obj is Screen comp)
		{
			if (_hmonitor == comp._hmonitor)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	///  Computes and retrieves a hash code for an object.
	/// </summary>
	public override int GetHashCode() => (int)(nint)_hmonitor;

	/// <summary>
	///  Called by the SystemEvents class when our display settings are
	///  changing.  We cache screen information and at this point we must
	///  invalidate our cache.
	/// </summary>
	private static void OnDisplaySettingsChanging(object? sender, EventArgs e)
	{
		// Now that we've responded to this event, we don't need it again until
		// someone re-queries. We will re-add the event at that time.
		SystemEvents.DisplaySettingsChanging -= new EventHandler(OnDisplaySettingsChanging);

		// Display settings changed, so the set of screens we have is invalid.
		s_screens = null;
	}

	/// <summary>
	///  Retrieves a string representing this object.
	/// </summary>
	public override string ToString()
	{
		return GetType().Name + "[Bounds=" + _bounds.ToString() + " WorkingArea=" + WorkingArea.ToString() + " Primary=" + Primary.ToString() + " DeviceName=" + DeviceName;
	}

	/// <summary>
	///  Retrieves a <see cref="Screen"/>
	///  for the monitor that contains the specified point.
	/// </summary>
	public static Screen FromPoint(IPoint<int> point)
	{
		if (s_multiMonitorSupport)
		{
			return new Screen(PInvoke.MonitorFromPoint(point.ToSystemPoint(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST));
		}
		else
		{
			return new Screen(PRIMARY_MONITOR);
		}
	}
}
