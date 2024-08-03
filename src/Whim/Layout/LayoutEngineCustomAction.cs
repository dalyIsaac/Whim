namespace Whim;

/// <summary>
/// The payload for a custom action for a layout engine to perform, via <see cref="ILayoutEngine.PerformCustomAction{T}"/>.
/// </summary>
public record LayoutEngineCustomAction
{
	/// <summary>
	/// The name of the action. This should be unique to the layout engine type.
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// The window that triggered the action, if any. Proxy layout engines may use this for their
	/// own purposes - for example, the ProxyFloatingLayoutEngine.
	///
	/// This is deliberately set to required to force the specification of the triggering window, where possible.
	/// </summary>
	public required IWindow? Window { get; init; }
}

/// <summary>
/// The payload for a custom action for a layout engine to perform, via <see cref="ILayoutEngine.PerformCustomAction{T}"/>.
/// </summary>
/// <typeparam name="T">
/// The type of <see cref="Payload"/>.
/// </typeparam>
public record LayoutEngineCustomAction<T> : LayoutEngineCustomAction
{
	/// <summary>
	/// The payload of the action, which the handler can use to perform the action.
	/// </summary>
	public required T Payload { get; init; }
}
