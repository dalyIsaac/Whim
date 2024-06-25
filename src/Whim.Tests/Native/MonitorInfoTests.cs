namespace Whim.Tests;

public class MonitorInfoTests
{
	[Fact]
	public void GetDeviceName()
	{
		// Given
		MONITORINFOEXW monitor = new() { szDevice = "Test" };

		// When
		string result = monitor.GetDeviceName();

		// Then
		Assert.Equal("Test", result);
	}

	[Fact]
	public void IsPrimary()
	{
		// Given
		MONITORINFOEXW monitor =
			new() { monitorInfo = new MONITORINFO { dwFlags = (uint)MONITOR_FROM_FLAGS.MONITOR_DEFAULTTOPRIMARY } };

		// When
		bool result = monitor.IsPrimary();

		// Then
		Assert.True(result);
	}

	[Fact]
	public void IsNotPrimary()
	{
		// Given
		MONITORINFOEXW monitor = new() { monitorInfo = new MONITORINFO { dwFlags = 0 } };

		// When
		bool result = monitor.IsPrimary();

		// Then
		Assert.False(result);
	}
}
