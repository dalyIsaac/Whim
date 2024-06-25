using Serilog;

namespace Whim.Tests;

public class FileSinkConfigRollingIntervalExtensionsTests
{
	[Theory]
	[InlineData(FileSinkConfigRollingInterval.Infinite, RollingInterval.Infinite)]
	[InlineData(FileSinkConfigRollingInterval.Month, RollingInterval.Month)]
	[InlineData(FileSinkConfigRollingInterval.Day, RollingInterval.Day)]
	public void ToSerilog(FileSinkConfigRollingInterval interval, RollingInterval expected) =>
		Assert.Equal(expected, interval.ToSerilog());
}
