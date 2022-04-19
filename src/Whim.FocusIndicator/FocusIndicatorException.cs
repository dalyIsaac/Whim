using System;

namespace Whim.FocusIndicator;

public class FocusIndicatorException : Exception
{
	public FocusIndicatorException()
	{
	}

	public FocusIndicatorException(string message) : base(message)
	{
	}

	public FocusIndicatorException(string message, Exception innerException) : base(message, innerException)
	{
	}
}