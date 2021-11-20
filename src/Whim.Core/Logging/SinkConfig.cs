using Serilog.Events;

namespace Whim.Core.Logging
{
    public class SinkConfig
    {
        public SinkLogLevel LogLevel { get; private set; } = SinkLogLevel.Warning;

        public SinkConfig(SinkLogLevel logLevel)
        {
            LogLevel = logLevel;
        }

        internal LogEventLevel GetLogLevel()
        {
            return LogLevel switch
            {
                SinkLogLevel.Verbose => LogEventLevel.Verbose,
                SinkLogLevel.Debug => LogEventLevel.Debug,
                SinkLogLevel.Information => LogEventLevel.Information,
                SinkLogLevel.Warning => LogEventLevel.Warning,
                SinkLogLevel.Error => LogEventLevel.Error,
                _ => LogEventLevel.Fatal,
            };
        }
    }
}
