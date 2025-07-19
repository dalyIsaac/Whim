namespace Whim;

/// <summary>
/// Represents a processor for events from Windows. This is for windows with non-standard behavior.
/// For example, Firefox will try reset the window position on startup. The <see cref="FirefoxWindowProcessor"/>
/// will ignore these events.
/// </summary>
/// <remarks>
/// Window processors are expected to implement a method which accepts an <see cref="IContext"/> and an <see cref="IWindow"/>.
/// If the window matches the processor, it should return an instance of the processor.
/// Otherwise, it should return null.
/// The processor will then be used to handle events for that window.
/// </remarks>
public interface IWindowProcessor
{
	/// <summary>
	/// The window that this processor is for.
	/// </summary>
	IWindow Window { get; }

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
	WindowProcessorResult ProcessEvent(
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	);
}
