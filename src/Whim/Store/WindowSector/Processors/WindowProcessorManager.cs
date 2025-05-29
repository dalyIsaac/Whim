using Windows.Win32.UI.Accessibility;

namespace Whim;

internal class WindowProcessorManager
{
	private readonly IContext _ctx;

	private readonly List<Func<IContext, IWindow, IWindowProcessor?>> _processorCreators = [];

	private readonly Dictionary<HWND, IWindowProcessor> _processors = [];

	public WindowProcessorManager(IContext ctx)
	{
		_ctx = ctx;
		_processorCreators.Add(FirefoxWindowProcessor.Create);
		_processorCreators.Add(TeamsWindowProcessor.Create);
	}

	internal bool ShouldBeIgnored(
		IWindow window,
		HWINEVENTHOOK hWinEventHook,
		uint eventType,
		int idObject,
		int idChild,
		uint idEventThread,
		uint dwmsEventTime
	)
	{
		if (!_processors.TryGetValue(window.Handle, out IWindowProcessor? processor))
		{
			if ((processor = CreateProcessor(window)) == null)
			{
				// No processor was created for this window, so we don't have any special handling for it.
				return false;
			}
		}

		WindowProcessorResult result = processor.ProcessEvent(
			eventType,
			idObject,
			idChild,
			idEventThread,
			dwmsEventTime
		);
		switch (result)
		{
			case WindowProcessorResult.Ignore:
				return true;
			case WindowProcessorResult.IgnoreAndLayout:
				_ctx.Store.WhimDispatch(new LayoutAllActiveWorkspacesTransform());
				return true;
			case WindowProcessorResult.Process:
				return false;
			case WindowProcessorResult.ProcessAndRemove:
				_processors.Remove(window.Handle);
				return false;
			default:
				Logger.Error($"Unhandled WindowProcessorResult {result}");
				return false;
		}
	}

	private IWindowProcessor? CreateProcessor(IWindow window)
	{
		foreach (Func<IContext, IWindow, IWindowProcessor?> creator in _processorCreators)
		{
			IWindowProcessor? processor = creator(_ctx, window);
			if (processor != null)
			{
				_processors.Add(window.Handle, processor);
				return processor;
			}
		}

		return null;
	}
}
