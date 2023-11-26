using Serilog.Events;
using Xunit;

namespace Whim.Tests;

public class LogLevelExtensionsTests
{
	[Theory]
	[InlineData(LogLevel.Verbose, LogEventLevel.Verbose)]
	[InlineData(LogLevel.Debug, LogEventLevel.Debug)]
	[InlineData(LogLevel.Information, LogEventLevel.Information)]
	[InlineData(LogLevel.Warning, LogEventLevel.Warning)]
	[InlineData(LogLevel.Error, LogEventLevel.Error)]
	[InlineData(LogLevel.Fatal, LogEventLevel.Fatal)]
	public void ToSerilog(LogLevel level, LogEventLevel expected) => Assert.Equal(expected, level.ToSerilog());
}
