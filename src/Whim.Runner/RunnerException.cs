using System;

namespace Whim.Runner;

/// <summary>
/// Exception thrown by Whim.Runner.
/// </summary>
[Serializable]
public class RunnerException : Exception
{
	/// <inheritdoc/>
	public RunnerException() { }

	/// <inheritdoc/>
	public RunnerException(string message) : base(message) { }

	/// <inheritdoc/>
	public RunnerException(string message, Exception inner) : base(message, inner) { }

	/// <inheritdoc/>
	protected RunnerException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
