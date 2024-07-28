using Windows.Win32;

namespace Whim;

/// <summary>
/// Custom logic to handle events from Firefox.
/// </summary>
public class FirefoxWindowProcessor : IWindowProcessor
{
	private bool _hasSeenFirstCloaked;

	/// <inheritdoc/>
	public IWindow Window { get; }

	private FirefoxWindowProcessor(IWindow window)
	{
		Window = window;
	}

	/// <summary>
	/// Creates a new instance of the implementing class, if the given window matches the processor.
	/// </summary>
	public static IWindowProcessor? Create(IContext ctx, IWindow window) =>
		window.ProcessFileName == "firefox.exe" ? new FirefoxWindowProcessor(window) : null;

	/// <summary>
	/// Indicates whether the event should be ignored by Whim.
	/// </summary>
	/// <remarks>
	/// Firefox has some irregular behavior:
	///
	/// <list type="bullet">
	///
	/// <item>
	/// <description>
	/// Firefox will move a window after an interdeterminate timeout on startup <see href="https://searchfox.org/mozilla-central/source/browser/components/sessionstore/SessionStore.sys.mjs#5587">[Source]</see>
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// Firefox can perform actions 500ms after an event is received - e.g., after <c>WindowMoved</c> <see href="https://searchfox.org/mozilla-central/source/xpfe/appshell/AppWindow.cpp#2588">[Source]</see>
	/// </description>
	/// </item>
	///
	/// <item>
	/// <description>
	/// Firefox will cloak the window when showing for the first time <see href="https://searchfox.org/mozilla-central/source/widget/windows/nsWindow.cpp#1639">[Source]</see>
	/// </description>
	/// </item>
	/// </list>
	///
	/// To deal with these issues, we ignore all events until the first <see cref="PInvoke.EVENT_OBJECT_CLOAKED"/> event is received.
	/// </remarks>
	/// <param name="eventType"></param>
	/// <param name="idObject"></param>
	/// <param name="idChild"></param>
	/// <param name="idEventThread"></param>
	/// <param name="dwmsEventTime"></param>
	/// <returns></returns>
	public WindowProcessorResult ShouldBeIgnored(
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	)
	{
		if (eventType == PInvoke.EVENT_OBJECT_CLOAKED)
		{
			if (!_hasSeenFirstCloaked)
			{
				_hasSeenFirstCloaked = true;
				return WindowProcessorResult.Ignore;
			}
		}

		if (eventType == PInvoke.EVENT_OBJECT_DESTROY)
		{
			return WindowProcessorResult.RemoveProcessor;
		}

		if (!_hasSeenFirstCloaked)
		{
			return WindowProcessorResult.Ignore;
		}

		return WindowProcessorResult.Process;
	}
}
