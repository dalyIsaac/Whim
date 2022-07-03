using System;

namespace Whim.Runner;

/// <summary>
/// Exception thrown by Whim.Runner.
/// </summary>
[Serializable]
public class RunnerException : Exception
{
	public RunnerException() { }
	public RunnerException(string message) : base(message) { }
	public RunnerException(string message, Exception inner) : base(message, inner) { }
	protected RunnerException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
