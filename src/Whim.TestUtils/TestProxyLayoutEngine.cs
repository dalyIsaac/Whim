using System;
using System.Collections.Generic;

namespace Whim.TestUtils;

/// <summary>
/// A non-functional proxy layout engine that can be used for testing.
/// </summary>
public class TestProxyLayoutEngine : BaseProxyLayoutEngine
{
	/// <summary>
	/// Creates a new <see cref="TestProxyLayoutEngine"/> with the given <paramref name="innerLayoutEngine"/>.
	/// </summary>
	public TestProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine) { }

	/// <inheritdoc/>
	public override int Count => 0;

	/// <inheritdoc/>
	public override ILayoutEngine AddWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public override bool ContainsWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public override IEnumerable<IWindowState> DoLayout(ILocation<int> location, IMonitor monitor) =>
		InnerLayoutEngine.DoLayout(location, monitor);

	/// <inheritdoc/>
	public override void FocusWindowInDirection(Direction direction, IWindow window) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public override IWindow? GetFirstWindow() => null;

	/// <inheritdoc/>
	public override ILayoutEngine MoveWindowEdgesInDirection(Direction edge, IPoint<double> deltas, IWindow window) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public override ILayoutEngine MoveWindowToPoint(IWindow window, IPoint<double> point) =>
		throw new NotImplementedException();

	/// <inheritdoc/>
	public override ILayoutEngine RemoveWindow(IWindow window) => throw new NotImplementedException();

	/// <inheritdoc/>
	public override ILayoutEngine SwapWindowInDirection(Direction direction, IWindow window) =>
		throw new NotImplementedException();
}
