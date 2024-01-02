using System.Collections.Generic;
using System.Collections.Immutable;

namespace Whim;

// TODO: Describe SwapWindowInDirection and FocusWindowInDirection
// NOTE: This LayoutEngine can have 0 or 1 windows shown
public record FocusLayoutEngine : ILayoutEngine
{
	private readonly ImmutableList<IWindow> _list;
	private readonly int _focusedIndex;
	private readonly bool _hideFocusedWindow;
	private readonly bool _maximized;

	/// <inheritdoc/>
	public string Name { get; init; } = "Focus";

	/// <inheritdoc/>
	public int Count => _list.Count;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="FocusLayoutEngine"/> class.
	/// </summary>
	/// <param name="identity">The identity of the layout engine.</param>
	public FocusLayoutEngine(LayoutEngineIdentity identity)
	{
		Identity = identity;
		_list = ImmutableList<IWindow>.Empty;
	}

	private FocusLayoutEngine(
		FocusLayoutEngine layoutEngine,
		ImmutableList<IWindow> list,
		int focusedIndex,
		bool maximized,
		bool hideFocusedWindow
	)
	{
		Name = layoutEngine.Name;
		Identity = layoutEngine.Identity;
		_list = list;
		_focusedIndex = focusedIndex < 0 || focusedIndex >= _list.Count ? 0 : focusedIndex;
		_maximized = maximized;
		_hideFocusedWindow = hideFocusedWindow;
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		if (_list.Contains(window))
		{
			Logger.Debug($"Window {window} already exists in layout engine {Name}");
			return this;
		}

		ImmutableList<IWindow> newList = _list.Add(window);
		return new FocusLayoutEngine(this, newList, newList.Count - 1, _maximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		if (!_list.Contains(window))
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		return new FocusLayoutEngine(this, _list.Remove(window), _focusedIndex, _maximized, _hideFocusedWindow);
	}

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _list.Contains(window);
	}

	/// <inheritdoc/>
	public IWindow? GetFirstWindow()
	{
		Logger.Debug($"Getting first window in layout engine {Name}");
		return _list.Count == 0 ? null : _list[0];
	}

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Performing a focus layout");

		if (_list.Count == 0)
		{
			yield break;
		}

		for (int idx = 0; idx < _list.Count; idx++)
		{
			IWindow window = _list[idx];
			bool focused = idx == _focusedIndex;
			bool maximized = _maximized && focused;

			if (!focused)
			{
				yield return new WindowState()
				{
					Window = window,
					Rectangle = rectangle,
					WindowSize = WindowSize.Minimized
				};
			}
			else
			{
				WindowSize windowSize;
				if (maximized)
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
					Window = window,
					Rectangle = rectangle,
					WindowSize = windowSize,
				};
			}
		}
	}

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Focusing window {window} in direction {direction} in layout engine {Name}");

		if (!_list.Contains(window))
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		int index = _list.IndexOf(window);
		int newIndex = direction switch
		{
			Direction.Left => index - 1,
			Direction.Up => index - 1,
			_ => index + 1
		};
		newIndex = newIndex.Mod(_list.Count);

		return new FocusLayoutEngine(this, _list, newIndex, _maximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window)
	{
		Logger.Debug($"Swapping window {window} in direction {direction} in layout engine {Name}");

		if (!_list.Contains(window))
		{
			Logger.Debug($"Window {window} does not exist in layout engine {Name}");
			return this;
		}

		int index = _list.IndexOf(window);
		int newIndex = direction switch
		{
			Direction.Left => index - 1,
			Direction.Up => index - 1,
			_ => index + 1
		};
		newIndex = newIndex.Mod(_list.Count);

		IWindow oldWindow = _list[index];
		IWindow newWindow = _list[newIndex];

		ImmutableList<IWindow> newList = _list.SetItem(index, newWindow).SetItem(newIndex, oldWindow);
		return new FocusLayoutEngine(this, newList, newIndex, _maximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) => AddWindow(window);

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window)
	{
		Logger.Debug($"Minimizing window {window} in layout engine {Name}");

		if (window.Equals(_list[_focusedIndex]))
		{
			return new FocusLayoutEngine(this, _list, _focusedIndex, _maximized, true);
		}

		if (!_list.Contains(window))
		{
			return new FocusLayoutEngine(this, _list.Add(window), _list.Count, _maximized, _hideFocusedWindow);
		}

		return this;
	}

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window)
	{
		Logger.Debug($"Restoring window {window} in layout engine {Name}");

		ImmutableList<IWindow> newList = _list;
		int idx;
		if (!newList.Contains(window))
		{
			newList = newList.Add(window);
			idx = newList.Count - 1;
		}
		else
		{
			idx = newList.IndexOf(window);
		}

		return new FocusLayoutEngine(this, newList, idx, _maximized, false);
	}

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action)
	{
		if (action.Name == $"{Name}.toggle_maximized")
		{
			return new FocusLayoutEngine(this, _list, _focusedIndex, !_maximized, _hideFocusedWindow);
		}

		return this;
	}
}
