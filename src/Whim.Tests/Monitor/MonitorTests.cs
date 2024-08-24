using AutoFixture;
using Windows.Win32.UI.HiDpi;

namespace Whim.Tests;

internal class MonitorCustomization : ICustomization
{
	public void Customize(IFixture fixture)
	{
		IInternalContext internalCtx = fixture.Freeze<IInternalContext>();

		internalCtx
			.CoreNativeManager.GetMonitorInfoEx(Arg.Any<HMONITOR>())
			.Returns(
				(_) =>
				{
					MONITORINFOEXW infoEx = default;
					infoEx.monitorInfo.rcMonitor = new RECT(0, 0, 1920, 1080);
					infoEx.monitorInfo.rcWork = new RECT(10, 10, 1910, 1070);
					infoEx.monitorInfo.dwFlags = 0;
					infoEx.szDevice = "DISPLAY";
					return infoEx;
				}
			);

		uint effectiveDpiX = 144;
		uint effectiveDpiY = 144;
		internalCtx
			.CoreNativeManager.GetDpiForMonitor(
				Arg.Any<HMONITOR>(),
				Arg.Any<MONITOR_DPI_TYPE>(),
				out uint _,
				out uint _
			)
			.Returns(
				(callInfo) =>
				{
					callInfo[2] = effectiveDpiX;
					callInfo[3] = effectiveDpiY;
					return (HRESULT)0;
				}
			);

		fixture.Inject(new HMONITOR(1));
	}
}

public class MonitorTests
{
	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void CreateMonitor_SingleMonitor(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(false);
		bool isPrimaryHMonitor = true;

		// When
		Monitor monitor = new(internalCtx, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.True(monitor.IsPrimary);
		Assert.Equal(new Rectangle<int>() { Width = 1920, Height = 1080 }, monitor.Bounds);
		Assert.Equal(
			new Rectangle<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060,
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void CreateMonitor_IsPrimaryHMonitor(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(true);
		bool isPrimaryHMonitor = true;

		// When
		Monitor monitor = new(internalCtx, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.True(monitor.IsPrimary);
		Assert.Equal(new Rectangle<int>() { Width = 1920, Height = 1080 }, monitor.Bounds);
		Assert.Equal(
			new Rectangle<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060,
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void CreateMonitor_MultipleMonitors(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(true);

		uint effectiveDpiX = 144;
		uint effectiveDpiY = 144;
		internalCtx
			.CoreNativeManager.GetDpiForMonitor(
				Arg.Any<HMONITOR>(),
				Arg.Any<MONITOR_DPI_TYPE>(),
				out uint _,
				out uint _
			)
			.Returns(
				(callInfo) =>
				{
					callInfo[2] = effectiveDpiX;
					callInfo[3] = effectiveDpiY;
					return (HRESULT)0;
				}
			);

		bool isPrimaryHMonitor = false;

		// When
		Monitor monitor = new(internalCtx, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("DISPLAY", monitor.Name);
		Assert.False(monitor.IsPrimary);
		Assert.Equal(new Rectangle<int>() { Width = 1920, Height = 1080 }, monitor.Bounds);
		Assert.Equal(
			new Rectangle<int>()
			{
				X = 10,
				Y = 10,
				Width = 1900,
				Height = 1060,
			},
			monitor.WorkingArea
		);
		Assert.Equal(150, monitor.ScaleFactor);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void CreateMonitor_CreationFailed(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		bool isPrimaryHMonitor = false;
		internalCtx.CoreNativeManager.HasMultipleMonitors().Returns(true);
		internalCtx.CoreNativeManager.GetMonitorInfoEx(Arg.Any<HMONITOR>()).Returns((_) => null);

		// When
		Monitor monitor = new(internalCtx, hmonitor, isPrimaryHMonitor);

		// Then
		Assert.Equal("NOT A DISPLAY", monitor.Name);
		Assert.False(monitor.IsPrimary);
		Assert.Equal(new Rectangle<int>(), monitor.Bounds);
		Assert.Equal(new Rectangle<int>(), monitor.WorkingArea);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void Equals_Failure(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		HMONITOR hmonitor2 = new(2);

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);
		Monitor monitor2 = new(internalCtx, hmonitor2, false);

		// Then
		Assert.False(monitor.Equals(monitor2));
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void Equals_Failure_Null(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);

		// Then
#pragma warning disable CA1508 // Avoid dead conditional code
		Assert.False(monitor.Equals(null));
#pragma warning restore CA1508 // Avoid dead conditional code
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void Equals_Success(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);
		Monitor monitor2 = new(internalCtx, hmonitor, false);

		// Then
		Assert.True(monitor.Equals(monitor2));
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void Equals_Operator_Success(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);
		Monitor monitor2 = new(internalCtx, hmonitor, false);

		// Then
		Assert.True(monitor == monitor2);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void NotEquals_Operator_Success(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given
		HMONITOR hmonitor2 = new(2);

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);
		Monitor monitor2 = new(internalCtx, hmonitor2, false);

		// Then
		Assert.True(monitor != monitor2);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void ToString_Success(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given

		// When
		Monitor monitor = new(internalCtx, hmonitor, true);

		// Then
		Assert.Equal(
			"Monitor[Bounds=(X: 0, Y: 0, Width: 1920, Height: 1080) WorkingArea=(X: 10, Y: 10, Width: 1900, Height: 1060) Name=DISPLAY ScaleFactor=150 IsPrimary=True]",
			monitor.ToString()
		);
	}

	[Theory, AutoSubstituteData<MonitorCustomization>]
	internal void GetHashCode_Success(IInternalContext internalCtx, HMONITOR hmonitor)
	{
		// Given

		// When
		Monitor monitor = new(internalCtx, hmonitor, false);
		Monitor monitor2 = new(internalCtx, hmonitor, false);

		// Then
		Assert.Equal(monitor.GetHashCode(), monitor2.GetHashCode());
	}
}
