namespace Whim;

/// <summary>
/// <see cref="SinkConfig"/> with custom functionality for the file sink.
/// </summary>
public record FileSinkConfig : SinkConfig
{
	/// <summary>
	/// <c>RollingInterval</c> for the file.
	/// </summary>
	public required FileSinkConfigRollingInterval RollingInterval { get; init; }

	/// <summary>
	/// The filename of the file log. The path is determined by <see cref="IContext"/>.
	/// </summary>
	public required string FileName { get; init; }
}
