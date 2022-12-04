using Serilog.Core;

namespace Whim;

/// <summary>
/// Default configuration options for a logging sink.
/// </summary>
public record SinkConfig
{
	/// <summary>
	/// What the <see cref="Logger"/> looks at.
	/// </summary>
	internal LoggingLevelSwitch MinLogLevelSwitch { get; } = new LoggingLevelSwitch();

	/// <summary>
	/// The sink's minimum log level.
	/// </summary>
	public required LogLevel MinLogLevel
	{
		get => LogLevelExtensions.ToSink(MinLogLevelSwitch.MinimumLevel);
		set => MinLogLevelSwitch.MinimumLevel = value.ToSerilog();
	}
}
