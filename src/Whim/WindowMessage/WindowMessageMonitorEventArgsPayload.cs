namespace Whim;

/// <summary>
/// Represents a message passed to a window procedure.
/// </summary>
/// <remarks>
/// See https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-msg
/// </remarks>
public class WindowMessageMonitorEventArgsPayload
{
	/// <summary>
	/// A handle to the window.
	/// </summary>
	public required HWND HWnd { get; init; }

	/// <summary>
	/// The message.
	/// </summary>
	public required uint UMsg { get; init; }

	/// <summary>
	/// Additional message information.
	/// </summary>
	public required nuint WParam { get; init; }

	/// <summary>
	/// Additional message information.
	/// </summary>
	public required nint LParam { get; init; }

	/// <inheritdoc />
	public override string ToString() => $"HWnd: {HWnd}, UMsg: {UMsg}, WParam: {WParam}, LParam: {LParam}";
}
