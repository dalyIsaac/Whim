namespace Whim.Core.Logging
{
    public class FileSinkConfig : SinkConfig
    {
        public FileSinkConfigRollingInterval RollingInterval { get; }
        public string FilePath { get; }

        public FileSinkConfig(string filePath, SinkLogLevel logLevel,
            FileSinkConfigRollingInterval rollingInterval = FileSinkConfigRollingInterval.Day
            ) : base(logLevel)
        {
            FilePath = filePath;
            RollingInterval = rollingInterval;
        }
    }
}