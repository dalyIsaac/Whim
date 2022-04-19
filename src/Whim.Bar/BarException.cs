using System;

namespace Whim.Bar;

public class BarException : Exception
{
	public BarException()
	{
	}

	public BarException(string message) : base(message)
	{
	}

	public BarException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
