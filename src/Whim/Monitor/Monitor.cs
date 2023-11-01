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

	// Bounds and WorkingArea are lazily evaluated because sometimes they return incorrect values
	// inside RDP sessions, during display changes. This is a workaround for that.
	public ILocation<int> Bounds => GetBounds();

	public ILocation<int> WorkingArea => GetWorkingArea();

	public int ScaleFactor { get; private set; }

	[MemberNotNull(nameof(IsPrimary), nameof(Name), nameof(ScaleFactor))]
	internal unsafe void Update(bool isPrimaryHMonitor)
	{
		IsPrimary = !_internalContext.CoreNativeManager.HasMultipleMonitors() || isPrimaryHMonitor;
		if (IsPrimary)
		{
			Name = "DISPLAY";
		}
		else if (_internalContext.CoreNativeManager.GetMonitorInfoEx(_hmonitor) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			Name = infoEx.GetDeviceName();
		}
		else
		{
			Name = "NOT A DISPLAY";
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

	private ILocation<int> GetBounds()
	{
		if (IsPrimary)
		{
			return new Location<int>()
			{
				X = _internalContext.CoreNativeManager.GetVirtualScreenLeft(),
				Y = _internalContext.CoreNativeManager.GetVirtualScreenTop(),
				Width = _internalContext.CoreNativeManager.GetVirtualScreenWidth(),
				Height = _internalContext.CoreNativeManager.GetVirtualScreenHeight()
			};
		}
		else if (_internalContext.CoreNativeManager.GetMonitorInfoEx(_hmonitor) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			return infoEx.monitorInfo.rcMonitor.ToLocation();
		}
		else
		{
			return new Location<int>();
		}
	}

	private ILocation<int> GetWorkingArea()
	{
		if (IsPrimary)
		{
			_internalContext.CoreNativeManager.GetPrimaryDisplayWorkArea(out RECT rect);
			return rect.ToLocation();
		}
		else if (_internalContext.CoreNativeManager.GetMonitorInfoEx(_hmonitor) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			return infoEx.monitorInfo.rcWork.ToLocation();
		}
		else
		{
			return new Location<int>();
		}
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
