namespace Whim.TestUtils;

/// <summary>
/// A non-functional proxy layout engine that can be used for testing.
/// </summary>
public abstract record TestProxyLayoutEngine : BaseProxyLayoutEngine
{
	/// <summary>
	/// Creates a new <see cref="TestProxyLayoutEngine"/> with the given <paramref name="innerLayoutEngine"/>.
	/// </summary>
	/// <param name="innerLayoutEngine"></param>
	protected TestProxyLayoutEngine(ILayoutEngine innerLayoutEngine)
		: base(innerLayoutEngine) { }

	/// <summary>
	/// The proxied layout engine.
	/// </summary>
	public new ILayoutEngine InnerLayoutEngine => base.InnerLayoutEngine;
}
