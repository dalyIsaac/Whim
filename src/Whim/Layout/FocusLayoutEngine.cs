namespace Whim;

/// <summary>
/// A layout engine that displays one window at a time. <see cref="FocusLayoutEngine"/> supports showing windows as:
///
/// <list type="bullet">
/// <item>
/// <description>
/// <see cref="WindowSize.Normal"/>: The focused window is shown in its normal size.
/// </description>
/// </item>
///
/// <item>
/// <description>
/// <see cref="WindowSize.Maximized"/>: The focused window is shown in its maximized size.
/// </description>
/// </item>
///
/// <item>
/// <description>
/// <see cref="WindowSize.Minimized"/>: The focused window can be hidden. Additionally, all non-focused windows are shown as minimized
/// (minimized to the taskbar).
/// </description>
/// </item>
/// </list>
///
/// <br />
///
/// The <see cref="SwapWindowInDirection(Direction, IWindow)"/> and <see cref="FocusWindowInDirection(Direction, IWindow)"/>
/// behaviour are a bit different to other layout engines:
///
/// <list type="bullet">
/// <item>
/// <description>
/// When <see cref="SwapWindowInDirection(Direction, IWindow)"/> is called, the window in the direction of the given window
/// will be swapped with the given window in the list of windows. This does not change the focused window.
/// </description>
/// </item>
///
/// <item>
/// <description>
/// When <see cref="FocusWindowInDirection(Direction, IWindow)"/> is called, the focused window will be changed to the window
/// in the direction of the given window. This does not change the order of the windows.
/// </description>
/// </item>
/// </list>
/// </summary>
public record FocusLayoutEngine : ILayoutEngine
{
	private readonly Stack _stack;
	private readonly bool _hideFocusedWindow;
	private readonly bool _isMaximized;

	/// <inheritdoc/>
	public string Name { get; init; } = "Focus";

	/// <inheritdoc/>
	public bool SupportsStacking => true;

	/// <inheritdoc/>
	public int Count => _stack.Count;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="FocusLayoutEngine"/> class.
	/// </summary>
	/// <param name="identity">The identity of the layout engine.</param>
	/// <param name="isMsMaximized">Whether the focused window should be maximized.</param>
	public FocusLayoutEngine(LayoutEngineIdentity identity, bool isMsMaximized = false)
	{
		Identity = identity;
		_isMaximized = isMsMaximized;
		_stack = new Stack();
	}

	private FocusLayoutEngine(FocusLayoutEngine layoutEngine, Stack stack, bool isMaximized, bool hideFocusedWindow)
	{
		Name = layoutEngine.Name;
		Identity = layoutEngine.Identity;
		_stack = stack;
		_isMaximized = isMaximized;
		_hideFocusedWindow = hideFocusedWindow;
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		Stack newStack = _stack.AddWindow(window);
		if (newStack.Equals(_stack))
		{
			return this;
		}

		return new FocusLayoutEngine(this, newStack, _isMaximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Stack newStack = _stack.RemoveWindow(window);
		if (newStack.Equals(_stack))
		{
			return this;
		}

		return new FocusLayoutEngine(this, newStack, _isMaximized, _hideFocusedWindow);
	}

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _stack.Contains(window);
	}

	/// <inheritdoc/>
	public IWindow? GetFirstWindow() => _stack.GetFirstWindow();

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Performing a focus layout");

		if (_stack.Count == 0)
		{
			yield break;
		}

		WindowSize windowSize;
		if (_isMaximized)
		{
			windowSize = WindowSize.Maximized;
		}
		else if (_hideFocusedWindow)
		{
			windowSize = WindowSize.Minimized;
		}
		else
		{
			windowSize = WindowSize.Normal;
		}

		yield return new WindowState()
		{
			Window = _stack.Top,
			Rectangle = rectangle,
			WindowSize = windowSize,
		};

		foreach (IWindow window in _stack.GetNonTopWindows())
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = rectangle,
				WindowSize = WindowSize.Minimized,
			};
		}
	}

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in direction {direction} in layout engine {Name}");

		int index = _stack.IndexOf(window);
		if (index == -1)
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		int newIndex = direction switch
		{
			Direction.Left => index - 1,
			Direction.Up => index - 1,
			_ => index + 1,
		};
		newIndex = newIndex.Mod(_stack.Count);

		Stack newStack = _stack with { TopIndex = newIndex };
		newStack.Top.Focus();
		return new FocusLayoutEngine(this, newStack, _isMaximized, _hideFocusedWindow);
	}

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		int index = _stack.IndexOf(window);
		if (index == -1)
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		Stack newStack = direction switch
		{
			Direction.Left => _stack.SwapWithNext(index),
			Direction.Up => _stack.SwapWithPrevious(index),
			_ => _stack.SwapWithNext(index),
		};

		return new FocusLayoutEngine(this, newStack, _isMaximized, _hideFocusedWindow);
	}

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) => AddWindow(window);

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in layout engine {Name}");

		int idx = _stack.IndexOf(window);
		if (idx == -1)
		{
			return new FocusLayoutEngine(
				this,
				_stack.AddWindow(window),
				_isMaximized,
				_stack.Count == 0 || _hideFocusedWindow
			);
		}

		if (idx == _stack.TopIndex)
		{
			return new FocusLayoutEngine(this, _stack, _isMaximized, true);
		}

		return this;
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window)
	{
		Logger.Debug($"Restoring window {window} in layout engine {Name}");

		Stack newStack = _stack;
		int idx = _stack.IndexOf(window);
		if (idx == -1)
		{
			idx = _stack.Count - 1;
			newStack = _stack.AddWindow(window);
		}

		newStack = newStack with { TopIndex = idx };
		return new FocusLayoutEngine(this, newStack, _isMaximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action)
	{
		if (action.Name == $"{Name}.toggle_maximized")
		{
			return new FocusLayoutEngine(this, _stack, !_isMaximized, _hideFocusedWindow);
		}
		if (action.Name == $"{Name}.set_maximized")
		{
			return new FocusLayoutEngine(this, _stack, true, _hideFocusedWindow);
		}
		if (action.Name == $"{Name}.unset_maximized")
		{
			return new FocusLayoutEngine(this, _stack, false, _hideFocusedWindow);
		}

		return this;
	}
}
