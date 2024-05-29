using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;

namespace Whim;

internal record Monitor : IMonitor
{
	private readonly IInternalContext _internalContext;

	public HMONITOR Handle { get; }
	public string Name { get; }
	public bool IsPrimary { get; }
	public int ScaleFactor { get; }

	// Bounds and WorkingArea are lazily evaluated because sometimes they return incorrect values
	// inside RDP sessions, during display changes. This is a workaround for that.
	public IRectangle<int> Bounds => GetBounds();
	public IRectangle<int> WorkingArea => GetWorkingArea();

	public Monitor(IInternalContext internalContext, HMONITOR monitor, bool isPrimaryHMonitor)
	{
		_internalContext = internalContext;
		Handle = monitor;

		IsPrimary = isPrimaryHMonitor || _internalContext.CoreNativeManager.HasMultipleMonitors() == false;
		if (IsPrimary)
		{
			Name = "DISPLAY";
		}
		else if (_internalContext.CoreNativeManager.GetMonitorInfoEx(Handle) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			Name = infoEx.GetDeviceName();
		}
		else
		{
			Logger.Error($"Failed to get name for monitor {Handle}");
			Name = "NOT A DISPLAY";
		}

		// Get the scale factor.
		// We assume that monitors have the same DPI in the x and y directions.
		_internalContext.CoreNativeManager.GetDpiForMonitor(
			Handle,
			MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI,
			out uint effectiveDpiX,
			out uint _
		);
		ScaleFactor = (int)((double)effectiveDpiX / 96 * 100);
	}

	private IRectangle<int> GetBounds()
	{
		if (_internalContext.CoreNativeManager.GetMonitorInfoEx(Handle) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			return infoEx.monitorInfo.rcMonitor.ToRectangle();
		}
		else
		{
			Logger.Error($"Failed to get bounds for monitor {Handle}");
			return new Rectangle<int>();
		}
	}

	private IRectangle<int> GetWorkingArea()
	{
		if (_internalContext.CoreNativeManager.GetMonitorInfoEx(Handle) is MONITORINFOEXW infoEx)
		{
			// Multiple monitor system.
			return infoEx.monitorInfo.rcWork.ToRectangle();
		}
		else
		{
			Logger.Error($"Failed to get working area for monitor {Handle}");
			return new Rectangle<int>();
		}
	}

	public override int GetHashCode() => (int)(nint)Handle;

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
