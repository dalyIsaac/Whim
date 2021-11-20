using System;
using Serilog;

namespace Whim.Core.Logging
{
    public class Logger
    {
        private static Logger? instance;
        private readonly ILogger _logger;
        private readonly LoggerConfiguration _loggerConfiguration;

        private Logger(LoggerConfig config)
        {
            var fileSink = config.FileSink;
            var debugSink = config.DebugSink;

            _loggerConfiguration = new LoggerConfiguration().WriteTo.File(config.FileSink.FilePath, restrictedToMinimumLevel: fileSink.GetLogLevel());
            if (debugSink != null)
            {
                _loggerConfiguration = _loggerConfiguration.WriteTo.Debug(restrictedToMinimumLevel: debugSink.GetLogLevel());
            }

            _logger = _loggerConfiguration.CreateLogger();
        }

        public static Logger Initialize(LoggerConfig config)
        {
            if (instance == null)
            {
                instance = new Logger(config);
            }
            return instance;
        }

        public static void Verbose(string message, params object[] args) => instance?._logger.Verbose(message, args);
        public static void Verbose(Exception exception, string message, params object[] args) => instance?._logger.Verbose(exception, message, args);

        public static void Debug(string message, params object[] args) => instance?._logger.Debug(message, args);
        public static void Debug(Exception exception, string message, params object[] args) => instance?._logger.Debug(exception, message, args);

        public static void Information(string message, params object[] args) => instance?._logger.Information(message, args);
        public static void Information(Exception exception, string message, params object[] args) => instance?._logger.Information(exception, message, args);

        public static void Warning(string message, params object[] args) => instance?._logger.Warning(message, args);
        public static void Warning(Exception exception, string message, params object[] args) => instance?._logger.Warning(exception, message, args);

        public static void Error(string message, params object[] args) => instance?._logger.Error(message, args);
        public static void Error(Exception exception, string message, params object[] args) => instance?._logger.Error(exception, message, args);

        public static void Fatal(string message, params object[] args) => instance?._logger.Fatal(message, args);
        public static void Fatal(Exception exception, string message, params object[] args) => instance?._logger.Fatal(exception, message, args);

    }
}