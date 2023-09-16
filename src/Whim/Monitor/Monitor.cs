using System.Diagnostics.CodeAnalysis;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace Whim;

internal class Monitor : IMonitor
{
	private readonly IInternalContext _internalContext;
	internal readonly HMONITOR _hmonitor;

	public Monitor(IInternalContext internalContext, HMONITOR monitor, bool isPrimaryHMonitor)
	{
		_internalContext = internalContext;
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
		if (!_internalContext.CoreNativeManager.HasMultipleMonitors() || isPrimaryHMonitor)
		{
			// Single monitor system.
			Bounds = new Location<int>()
			{
				X = _internalContext.CoreNativeManager.GetVirtualScreenLeft(),
				Y = _internalContext.CoreNativeManager.GetVirtualScreenTop(),
				Width = _internalContext.CoreNativeManager.GetVirtualScreenWidth(),
				Height = _internalContext.CoreNativeManager.GetVirtualScreenHeight()
			};

			IsPrimary = true;
			Name = "DISPLAY";

			_internalContext.CoreNativeManager.GetPrimaryDisplayWorkArea(out RECT rect);
			WorkingArea = rect.ToLocation();
		}
		else
		{
			// Multiple monitor system.
			MONITORINFOEXW infoEx = new() { monitorInfo = new MONITORINFO() { cbSize = (uint)sizeof(MONITORINFOEXW) } };
			_internalContext.CoreNativeManager.GetMonitorInfo(_hmonitor, ref infoEx);

			Bounds = infoEx.monitorInfo.rcMonitor.ToLocation();
			WorkingArea = infoEx.monitorInfo.rcWork.ToLocation();
			IsPrimary = false;
			Name = infoEx.GetDeviceName();
		}

		// Get the scale factor.
		// We assume that monitors have the same DPI in the x and y directions.
		_internalContext.CoreNativeManager.GetDpiForMonitor(
			_hmonitor,
			MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
			out uint effectiveDpiX,
			out uint _
		);
		ScaleFactor = (int)((double)effectiveDpiX / 96 * 100);
	}

	/// <inheritdoc/>
	public override bool Equals(object? other) => other is Monitor monitor && _hmonitor == monitor._hmonitor;

	public static bool operator ==(Monitor? left, Monitor? right) => Equals(left, right);

	public static bool operator !=(Monitor? left, Monitor? right) => !Equals(left, right);

	public override int GetHashCode() => (int)(nint)_hmonitor;

	public override string ToString()
	{
		return GetType().Name
			+ "[Bounds="
			+ Bounds.ToString()
			+ " WorkingArea="
			+ WorkingArea.ToString()
			+ " Name="
			+ Name
			+ " ScaleFactor="
			+ ScaleFactor.ToString()
			+ " IsPrimary="
			+ IsPrimary.ToString()
			+ "]";
	}
}
