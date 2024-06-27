using System;
using System.Collections.Generic;

namespace Whim.TestUtils;

/// <summary>
/// A test layout engine that does nothing.
/// </summary>
public record TestLayoutEngine : ILayoutEngine
{
	/// <inheritdoc/>
	public string Name => "Test Layout Engine";

	/// <inheritdoc/>
	public int Count => 0;

	/// <inheritdoc/>
	public LayoutEngineIdentity Identity => new();

	/// <inheritdoc/>
	public ILayoutEngine AddWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) => throw new NotImplementedException();

	/// <inheritdoc/>
	public bool ContainsWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public IEnumerable<IWindowState> DoLayout(IRectangle<int> rectangle, IMonitor monitor) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine FocusWindowInDirection(Direction direction, IWindow window) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public IWindow? GetFirstWindow() => throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine MoveWindowEdgesInDirection(Direction edges, IPoint<double> deltas, IWindow window) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine RemoveWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine PerformCustomAction<T>(LayoutEngineCustomAction<T> action) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowStart(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public ILayoutEngine MinimizeWindowEnd(IWindow window) => throw new NotImplementedException();
}
