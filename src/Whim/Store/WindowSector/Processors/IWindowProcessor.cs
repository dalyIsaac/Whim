namespace Whim;

/// <summary>
/// Represents a processor for events from Windows. This is for windows with non-standard behavior.
/// For example, Firefox will try reset the window position on startup. The <see cref="FirefoxWindowProcessor"/>
/// will ignore these events.
/// </summary>
public interface IWindowProcessor
{
	/// <summary>
	/// The window that this processor is for.
	/// </summary>
	public IWindow Window { get; }

	/// <summary>
	/// Processes the given event.
	///
	/// For more about the arguments, see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nc-winuser-wineventproc
	/// </summary>
	/// <param name="eventType"></param>
	/// <param name="idObject"></param>
	/// <param name="idChild"></param>
	/// <param name="idEventThread"></param>
	/// <param name="dwmsEventTime"></param>
	/// <returns>
	/// Whether the event should be ignored by Whim.
	/// </returns>
	public abstract WindowProcessorResult ProcessEvent(
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	);
}
