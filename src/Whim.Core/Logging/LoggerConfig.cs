namespace Whim.Core.Logging
{
    public class LoggerConfig
    {
        public FileSinkConfig FileSink;
        public SinkConfig? DebugSink;

        public LoggerConfig(FileSinkConfig fileSinkConfig, SinkConfig? debugSink = null)
        {
            FileSink = fileSinkConfig;
            DebugSink = debugSink;
        }
    }
}