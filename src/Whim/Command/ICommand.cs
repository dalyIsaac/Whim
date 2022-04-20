namespace Whim;

/// <summary>
/// Command specification.
/// </summary>
public interface ICommand
{
	/// <summary>
	/// The <see cref="CommandType"/>
	/// </summary>
	public CommandType CommandType { get; }

	/// <summary>
	/// When <see langword="true"/>, the command will be handled by the handler of the first
	/// matching <see cref="Commander"/>. <br/>
	///
	/// Otherwise, the command will be handled by <em>all</em> matching <see cref="Commander"/>
	/// handlers.
	/// </summary>
	public bool? PreventCascade { get; }

	/// <summary>
	/// The maximum depth to search for a handler.
	/// </summary>
	public int? MaxDepth { get; }
}

/// <summary>
/// Command specification with payload.
/// </summary>
/// <typeparam name="TCommandPayloadType">Generic payload for the command.</typeparam>
public interface ICommand<TCommandPayloadType> : ICommand
{
	/// <summary>
	/// Payload for the specification.
	/// </summary>
	public TCommandPayloadType? Payload { get; }
}
