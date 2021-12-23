namespace Whim.Core;

/// <summary>
/// <see cref="SinkConfig"/> with custom functionality for the file sink.
/// </summary>
public class FileSinkConfig : SinkConfig
{
	/// <summary>
	/// <c>RollingInterval</c> for the file.
	/// </summary>
	public FileSinkConfigRollingInterval RollingInterval { get; }

	/// <summary>
	/// The filename of the file log. The path is determined by <see cref="IConfigContext"/>.
	/// </summary>
	public string FileName { get; }

	/// <summary>
	///
	/// </summary>
	/// <param name="fileName">The file name of the file log. See <see cref="FileName"/>.</param>
	/// <param name="logLevel">The minimum log level.</param>
	/// <param name="rollingInterval">
	/// The rolling interval for the file. See <see cref="RollingInterval"/>
	/// </param>
	public FileSinkConfig(
		string fileName,
		LogLevel logLevel,
		FileSinkConfigRollingInterval rollingInterval = FileSinkConfigRollingInterval.Day
	) : base(logLevel)
	{
		FileName = fileName;
		RollingInterval = rollingInterval;
	}
}
