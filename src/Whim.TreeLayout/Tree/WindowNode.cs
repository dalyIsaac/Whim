namespace Whim.TreeLayout;

/// <summary>
/// WindowNodes represent the location of a window within the layout engine.
/// </summary>
/// <remarks>
/// Creates a new window node for the given <paramref name="window"/>/
/// </remarks>
/// <param name="window"></param>
internal class WindowNode(IWindow window) : INode
{
	/// <summary>
	/// The window contained by the node.
	/// </summary>
	public IWindow Window { get; } = window;

	/// <summary>
	/// Moves the focus to this node.
	/// </summary>
	public void Focus() => Window.Focus();

	/// <summary>
	/// Gets the string representation of the window.
	/// </summary>
	public override string? ToString() => Window.ToString();
}
