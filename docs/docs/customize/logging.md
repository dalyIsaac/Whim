# Logging

Whim provides logging by wrapping the [Serilog](https://serilog.net/) library. It can be configured using the <xref:Whim.LoggerConfig> class. For example:

```csharp
// The logger will only log messages with a level of `Debug` or higher.
context.Logger.Config = new LoggerConfig() { BaseMinLogLevel = LogLevel.Debug };

// The logger will log messages with a level of `Debug` or higher to a file.
if (context.Logger.Config.FileSink is FileSinkConfig fileSinkConfig)
{
    fileSinkConfig.MinLogLevel = LogLevel.Debug;
}

// The logger will log messages with a level of `Error` or higher to the debug console.
// The debug sink is only available in debug builds, and can slow down Whim.
if (context.Logger.Config.DebugSink is SinkConfig debugSinkConfig)
{
    debugSinkConfig.MinLogLevel = LogLevel.Error;
}
```

> [!NOTE]
> Logging can be changed during runtime to be more restrictive, but cannot be made more permissive than the initial configuration.

## Logs Location

Logs will be located in the `.whim/logs` directory, with respect to actual location of the `.whim` directory.
