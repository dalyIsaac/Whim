using System;

namespace Whim;

public class ShutdownEventArgs : EventArgs
{
	public ShutdownReason Reason { get; }
	public string? Message { get; }

	public ShutdownEventArgs(ShutdownReason reason, string? message = null)
	{
		Reason = reason;
		Message = message;
	}
}
