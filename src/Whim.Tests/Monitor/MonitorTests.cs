using Moq;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.HiDpi;
using Xunit;

namespace Whim.Tests;

public class MonitorTests
{
	private static (Mock<ICoreNativeManager>, HMONITOR) CreatePrimaryMonitorMocks()
	{
		Mock<ICoreNativeManager> nativeManagerMock = new();
		nativeManagerMock.Setup(nm => nm.GetVirtualScreenLeft()).Returns(0);
		nativeManagerMock.Setup(nm => nm.GetVirtualScreenTop()).Returns(0);
		nativeManagerMock.Setup(nm => nm.GetVirtualScreenWidth()).Returns(1920);
		nativeManagerMock.Setup(nm => nm.GetVirtualScreenHeight()).Returns(1080);

		RECT rect =
			new()
			{
				left = 10,
				top = 10,
				right = 1910,
				bottom = 1070
			};
		nativeManagerMock.Setup(nm => nm.GetPrimaryDisplayWorkArea(out rect)).Returns((BOOL)true);

		uint effectiveDpiX = 144;
		uint effectiveDpiY = 144;
		nativeManagerMock
			.Setup(
				nm =>
					nm.GetDpiForMonitor(
						It.IsAny<HMONITOR>(),
						It.IsAny<MONITOR_DPI_TYPE>(),
						out effectiveDpiX,
						out effectiveDpiY
					)
			)
			.Returns((HRESULT)0);

		HMONITOR hmonitor = new(1);

		return (nativeManagerMock, hmonitor);
	}

	[Fact]
	public void CreateMonitor_SingleMonitor()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();
		nativeManagerMock.Setup(nm => nm.HasMultipleMonitors()).Returns(false);
		bool isPrimaryHMonitor = true;

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.True(monitor.IsPrimary);
		Assert.Equal(
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			monitor.Bounds
		);
		Assert.Equal(
			new Location<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Fact]
	public void CreateMonitor_IsPrimaryHMonitor()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();
		nativeManagerMock.Setup(nm => nm.HasMultipleMonitors()).Returns(true);
		bool isPrimaryHMonitor = true;

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.True(monitor.IsPrimary);
		Assert.Equal(
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			monitor.Bounds
		);
		Assert.Equal(
			new Location<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Fact]
	public void CreateMonitor_MultipleMonitors()
	{
		// Given
		Mock<ICoreNativeManager> nativeManagerMock = new();
		nativeManagerMock.Setup(nm => nm.HasMultipleMonitors()).Returns(true);

		nativeManagerMock
			.Setup(nm => nm.GetMonitorInfo(It.IsAny<HMONITOR>(), ref It.Ref<MONITORINFOEXW>.IsAny))
			.Callback(
				(HMONITOR hmonitor, ref MONITORINFOEXW infoEx) =>
				{
					infoEx.monitorInfo.rcMonitor = new RECT(0, 0, 1920, 1080);
					infoEx.monitorInfo.rcWork = new RECT(10, 10, 1910, 1070);
					infoEx.monitorInfo.dwFlags = 0;
					infoEx.szDevice = "DISPLAY";
				}
			)
			.Returns((BOOL)true);

		uint effectiveDpiX = 144;
		uint effectiveDpiY = 144;
		nativeManagerMock
			.Setup(
				nm =>
					nm.GetDpiForMonitor(
						It.IsAny<HMONITOR>(),
						It.IsAny<MONITOR_DPI_TYPE>(),
						out effectiveDpiX,
						out effectiveDpiY
					)
			)
			.Returns((HRESULT)0);

		HMONITOR hmonitor = new(1);
		bool isPrimaryHMonitor = false;

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.False(monitor.IsPrimary);
		Assert.Equal(
			new Location<int>()
			{
				X = 0,
				Y = 0,
				Width = 1920,
				Height = 1080
			},
			monitor.Bounds
		);
		Assert.Equal(
			new Location<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Fact]
	public void Equals_Failure()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();
		HMONITOR hmonitor2 = new(2);

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, false);
		Monitor monitor2 = new(nativeManagerMock.Object, hmonitor2, false);

		// Then
		Assert.False(monitor.Equals(monitor2));
	}

	[Fact]
	public void Equals_Failure_Null()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, false);

		// Then
#pragma warning disable CA1508 // Avoid dead conditional code
		Assert.False(monitor.Equals(null));
#pragma warning restore CA1508 // Avoid dead conditional code
	}

	[Fact]
	public void Equals_Success()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, false);
		Monitor monitor2 = new(nativeManagerMock.Object, hmonitor, false);

		// Then
		Assert.True(monitor.Equals(monitor2));
	}

	[Fact]
	public void ToString_Success()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, true);

		// Then
		Assert.Equal(
			"Monitor[Bounds=(X: 0, Y: 0, Width: 1920, Height: 1080) WorkingArea=(X: 10, Y: 10, Width: 1900, Height: 1060) Name=DISPLAY ScaleFactor=150 IsPrimary=True]",
			monitor.ToString()
		);
	}

	[Fact]
	public void GetHashCode_Success()
	{
		// Given
		(Mock<ICoreNativeManager> nativeManagerMock, HMONITOR hmonitor) = CreatePrimaryMonitorMocks();

		// When
		Monitor monitor = new(nativeManagerMock.Object, hmonitor, false);
		Monitor monitor2 = new(nativeManagerMock.Object, hmonitor, false);

		// Then
		Assert.Equal(monitor.GetHashCode(), monitor2.GetHashCode());
	}
}
