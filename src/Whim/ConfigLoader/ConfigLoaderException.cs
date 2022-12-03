using System;

namespace Whim;

/// <summary>
/// Exception thrown by Whim when loading the config.
/// </summary>
[Serializable]
public class ConfigLoaderException : Exception
{
	/// <inheritdoc/>
	public ConfigLoaderException() { }

	/// <inheritdoc/>
	public ConfigLoaderException(string message) : base(message) { }

	/// <inheritdoc/>
	public ConfigLoaderException(string message, Exception inner) : base(message, inner) { }

	/// <inheritdoc/>
	protected ConfigLoaderException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context
	) : base(info, context) { }
}
