using System;

namespace Whim.Bar;

[Serializable]
public class BarException : Exception
{
	public BarException() { }
	public BarException(string message) : base(message) { }
	public BarException(string message, Exception inner) : base(message, inner) { }
	protected BarException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
