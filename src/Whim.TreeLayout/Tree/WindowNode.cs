namespace Whim.TreeLayout;

/// <summary>
/// WindowNodes represent the location of a window within the layout engine.
/// Unlike <see cref="PhantomNode"/>s, <see cref="WindowNode"/>s are common
/// to all layout engines within a <see cref="IWorkspace"/>.
/// </summary>
internal class WindowNode : LeafNode
{
	/// <summary>
	/// Creates a new window node for the given <paramref name="window"/>/
	/// </summary>
	/// <param name="window"></param>
	public WindowNode(IWindow window)
		: base(window) { }

	/// <summary>
	/// Gets the string representation of the window.
	/// </summary>
	public override string? ToString() => Window.ToString();
}
