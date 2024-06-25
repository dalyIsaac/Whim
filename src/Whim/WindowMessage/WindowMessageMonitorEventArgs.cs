using Windows.Win32.UI.Shell;

namespace Whim;

/// <summary>
/// Args for a <see cref="WindowMessageMonitor"/> event.
/// </summary>
public class WindowMessageMonitorEventArgs : EventArgs
{
	/// <summary>
	/// The message.
	/// </summary>
	public required WindowMessageMonitorEventArgsPayload MessagePayload { get; init; }

	/// <summary>
	/// Set the result after processing the message, so <see cref="WindowMessageMonitor"/> can
	/// return the result from the <see cref="SUBCLASSPROC"/> delegate.
	/// This will only be returned if <see cref="Handled"/> is set to <see langword="true"/>
	/// </summary>
	public nint Result { get; set; } = 0;

	/// <summary>
	/// Indicates whether this message was handled and the whether the <see cref="Result"/> value
	/// should be returned from the <see cref="SUBCLASSPROC"/>.
	/// </summary>
	/// <remarks>
	/// When <see langword="false"/>, the message will continue to be processed by other subclasses.
	/// </remarks>
	public bool Handled { get; set; }

	/// <inheritdoc />
	public override string ToString() => $"Result: {Result}, Handled: {Handled}, {MessagePayload}";
}
