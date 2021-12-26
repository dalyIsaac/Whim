using System;

namespace Whim;

public class ShutdownEventArgs : EventArgs
{
	public readonly ShutdownReason Reason;
	public readonly string? Message;

	public ShutdownEventArgs(ShutdownReason reason, string? message = null)
	{
		Reason = reason;
		Message = message;
	}
}
