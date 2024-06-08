using System;
using System.Collections.Generic;

namespace Whim.TestUtils;

/// <summary>
/// A test layout engine that always returns itself or empty.
/// </summary>
public record ImmutableTestLayoutEngine : ILayoutEngine
{
	public string Name => "Immutable Test Layout Engine";

	public int Count => 0;

	public LayoutEngineIdentity Identity => new();

	public ILayoutEngine AddWindow(IWindow window) => this;

	public bool ContainsWindow(IWindow window) => false;

	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor) =>
		Array.Empty<IWindowState>();

	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window) => this;

	public IWindow? GetFirstWindow() => null;

	public ILayoutEngine MinimizeWindowEnd(IWindow window) => this;

	public ILayoutEngine MinimizeWindowStart(IWindow window) => this;

	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) => this;

	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) => this;

	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) => this;

	public ILayoutEngine RemoveWindow(IWindow window) => this;

	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) => this;
}
