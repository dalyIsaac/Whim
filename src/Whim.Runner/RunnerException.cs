using System;

namespace Whim.Runner;

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
