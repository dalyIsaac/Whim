using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Shell.Common;

namespace Whim;

internal class Monitor : IMonitor
{
	private readonly ICoreNativeManager _coreNativeManager;
	internal readonly HMONITOR _hmonitor;

	public Monitor(ICoreNativeManager coreNativeManager, HMONITOR monitor, bool isPrimaryHMonitor)
	{
		_coreNativeManager = coreNativeManager;
		_hmonitor = monitor;

		Update(isPrimaryHMonitor);
	}

	public string Name { get; private set; }

	public bool IsPrimary { get; private set; }

	public ILocation<int> Bounds { get; private set; }

	public ILocation<int> WorkingArea { get; private set; }

	public int ScaleFactor { get; private set; }

	[MemberNotNull(nameof(Bounds), nameof(IsPrimary), nameof(Name), nameof(WorkingArea), nameof(ScaleFactor))]
	internal unsafe void Update(bool isPrimaryHMonitor)
	{
		if (!_coreNativeManager.HasMultipleMonitors() || isPrimaryHMonitor)
		{
			// Single monitor system.
			Bounds = new Location<int>()
			{
				X = _coreNativeManager.GetVirtualScreenLeft(),
				Y = _coreNativeManager.GetVirtualScreenTop(),
				Width = _coreNativeManager.GetVirtualScreenWidth(),
				Height = _coreNativeManager.GetVirtualScreenHeight()
			};

			IsPrimary = true;
			Name = "DISPLAY";

			_coreNativeManager.GetPrimaryDisplayWorkArea(out RECT rect);
			WorkingArea = rect.ToLocation();
		}
		else
		{
			// Multiple monitor system.
			MONITORINFOEXW infoEx = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
			_coreNativeManager.GetMonitorInfo(_hmonitor, ref infoEx);

			Bounds = infoEx.GetLocation();
			IsPrimary = infoEx.IsPrimary();
			Name = infoEx.GetDeviceName();

			MONITORINFO info = new() { cbSize = (uint)sizeof(MONITORINFO) };
			_coreNativeManager.GetMonitorInfo(_hmonitor, ref info);
			WorkingArea = info.rcWork.ToLocation();
		}

		HRESULT scaleFactorResult = _coreNativeManager.GetScaleFactorForMonitor(
			_hmonitor,
			out DEVICE_SCALE_FACTOR scaleFactor
		);
		ScaleFactor = scaleFactorResult.Succeeded ? (int)scaleFactor : 100;
	}

	public bool Equals(IMonitor? other) => other is Monitor monitor && _hmonitor == monitor._hmonitor;

	public override int GetHashCode() => (int)(nint)_hmonitor;

	public override string ToString()
	{
		return GetType().Name
			+ "[Bounds="
			+ Bounds.ToString()
			+ " WorkingArea="
			+ WorkingArea.ToString()
			+ " IsPrimary="
			+ IsPrimary.ToString()
			+ " Name="
			+ Name;
	}
}
