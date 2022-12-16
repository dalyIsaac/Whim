using Microsoft.Win32;
using System;
using System.Threading;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell.Common;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Whim;

internal partial class Screen
{
	private readonly ICoreNativeManager _coreNativeManager;
	private readonly HMONITOR _hmonitor;

	private readonly ILocation<int> _bounds;

	private ILocation<int> _workingArea = new Location<int>();

	public readonly bool Primary;

	/// <summary>
	///  Instance-based counter used to invalidate <see cref="WorkingArea"/>.
	/// </summary>
	private int _currentDesktopChangedCount = -1;

	/// <summary>
	///  Device name associated with this monitor
	/// </summary>
	public string DeviceName { get; }

	internal unsafe Screen(ICoreNativeManager coreNativeManager, HMONITOR monitor)
	{
		_coreNativeManager = coreNativeManager;

		if (!_coreNativeManager.HasMultipleMonitors() || monitor == PRIMARY_MONITOR)
		{
			// Single monitor system.
			_bounds = new Location<int>()
			{
				X = _coreNativeManager.GetVirtualScreenLeft(),
				Y = _coreNativeManager.GetVirtualScreenTop(),
				Width = _coreNativeManager.GetVirtualScreenWidth(),
				Height = _coreNativeManager.GetVirtualScreenHeight()
			};

			Primary = true;
			DeviceName = "DISPLAY";
		}
		else
		{
			// Multiple monitor system.
			MONITORINFOEXW info = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
			_coreNativeManager.GetMonitorInfo(monitor, ref info);
			_bounds = info.GetLocation();
			Primary = info.IsPrimary();

			DeviceName = info.GetDeviceName();
		}

		_hmonitor = monitor;
	}

	public int ScaleFactor
	{
		get
		{
			HRESULT result = _coreNativeManager.GetScaleFactorForMonitor(
				_hmonitor,
				out DEVICE_SCALE_FACTOR scaleFactor
			);
			return result.Succeeded ? (int)scaleFactor : 100;
		}
	}

	/// <summary>
	///  Gets the
	///  primary display.
	/// </summary>
	public static Screen? GetPrimaryScreen(ICoreNativeManager coreNativeManager)
	{
		if (coreNativeManager.HasMultipleMonitors())
		{
			Screen[] screens = GetAllScreens(coreNativeManager);
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
			return new Screen(coreNativeManager, PRIMARY_MONITOR);
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

				if (!_coreNativeManager.HasMultipleMonitors() || _hmonitor == (IntPtr)PRIMARY_MONITOR)
				{
					// Single monitor system
					unsafe
					{
						RECT rect = new();
						_coreNativeManager.SystemParametersInfo(
							SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETWORKAREA,
							0,
							&rect,
							0
						);
						_workingArea = rect.ToLocation();
					}
				}
				else
				{
					// Multiple monitor System
					MONITORINFO info = new() { cbSize = (uint)sizeof(MONITORINFO) };
					_coreNativeManager.GetMonitorInfo(_hmonitor, ref info);
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
						SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(
							OnUserPreferenceChanged
						);

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
		return GetType().Name
			+ "[Bounds="
			+ _bounds.ToString()
			+ " WorkingArea="
			+ WorkingArea.ToString()
			+ " Primary="
			+ Primary.ToString()
			+ " DeviceName="
			+ DeviceName;
	}

	/// <summary>
	///  Retrieves a <see cref="Screen"/>
	///  for the monitor that contains the specified point.
	/// </summary>
	public static Screen FromPoint(ICoreNativeManager coreNativeManager, IPoint<int> point)
	{
		if (coreNativeManager.HasMultipleMonitors())
		{
			return new Screen(
				coreNativeManager,
				coreNativeManager.MonitorFromPoint(point.ToSystemPoint(), MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST)
			);
		}
		else
		{
			return new Screen(coreNativeManager, PRIMARY_MONITOR);
		}
	}
}
