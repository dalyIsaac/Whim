using System;

namespace Whim.FocusIndicator;

[Serializable]
public class FocusIndicatorException : Exception
{
	public FocusIndicatorException() { }
	public FocusIndicatorException(string message) : base(message) { }
	public FocusIndicatorException(string message, Exception inner) : base(message, inner) { }
	protected FocusIndicatorException(
		System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}