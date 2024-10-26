using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Whim.FloatingWindow;

/// <summary>
/// Layout engine that lays out all windows as free-floating.
/// </summary>
public record FloatingLayoutEngine : ILayoutEngine
{
	private readonly IContext _context;
	private readonly ImmutableDictionary<IWindow, IRectangle<double>> _dict;

	/// <inheritdoc/>
	public string Name { get; init; } = "Floating";

	/// <inheritdoc/>
	public int Count => _dict.Count;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity { get; }

	/// <summary>
	/// Creates a new instance of the <see cref="FloatingLayoutEngine"/> class.
	/// </summary>
	/// <param name="context">The identity of the layout engine.</param>
	/// <param name="identity">The context of the layout engine.</param>
	public FloatingLayoutEngine(IContext context, LayoutEngineIdentity identity)
	{
		Identity = identity;
		_context = context;
		_dict = ImmutableDictionary<IWindow, IRectangle<double>>.Empty;
	}

	private FloatingLayoutEngine(
		FloatingLayoutEngine layoutEngine,
		ImmutableDictionary<IWindow, IRectangle<double>> dict
	)
	{
		Name = layoutEngine.Name;
		Identity = layoutEngine.Identity;
		_context = layoutEngine._context;
		_dict = dict;
	}

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window)
	{
		Logger.Debug($"Adding window {window} to layout engine {Name}");

		if (_dict.ContainsKey(window))
		{
			Logger.Debug($"Window {window} already exists in layout engine {Name}");
			return this;
		}

		return UpdateWindowRectangle(window);
	}

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window)
	{
		Logger.Debug($"Removing window {window} from layout engine {Name}");

		ImmutableDictionary<IWindow, IRectangle<double>> newDict = _dict.Remove(window);

		return new FloatingLayoutEngine(this, newDict);
	}

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window)
	{
		Logger.Debug($"Checking if layout engine {Name} contains window {window}");
		return _dict.ContainsKey(window);
	}

	/// <inheritdoc/>
	public IWindow? GetFirstWindow() => _dict.Keys.FirstOrDefault();

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor)
	{
		Logger.Debug($"Doing layout for engine {Name}");

		foreach ((IWindow window, IRectangle<double> loc) in _dict)
		{
			yield return new WindowState()
			{
				Window = window,
				Rectangle = monitor.WorkingArea.ToMonitor(loc),
				WindowSize =
					window.IsMaximized ? WindowSize.Maximized
					: window.IsMinimized ? WindowSize.Minimized
					: WindowSize.Normal,
			};
		}
	}

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) => this;

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) => UpdateWindowRectangle(window);

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) =>
		UpdateWindowRectangle(window);

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window) => this;

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) => this;

	private FloatingLayoutEngine UpdateWindowRectangle(IWindow window)
	{
		ImmutableDictionary<IWindow, IRectangle<double>>? newDict = FloatingUtils.UpdateWindowRectangle(
			_context,
			_dict,
			window
		);

		if (newDict == null || newDict == _dict)
		{
			return this;
		}

		return new FloatingLayoutEngine(this, newDict);
	}
}
