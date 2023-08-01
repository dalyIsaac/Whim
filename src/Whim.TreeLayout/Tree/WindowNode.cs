namespace Whim.TreeLayout;

/// <summary>
/// WindowNodes represent the location of a window within the layout engine.
/// </summary>
internal class WindowNode : INode
{
	/// <summary>
	/// The window contained by the node.
	/// </summary>
	public IWindow Window { get; }

	/// <summary>
	/// Creates a new window node for the given <paramref name="window"/>/
	/// </summary>
	/// <param name="window"></param>
	public WindowNode(IWindow window)
	{
		Window = window;
	}

	/// <summary>
	/// Moves the focus to this node.
	/// </summary>
	public void Focus() => Window.Focus();

	/// <summary>
	/// Gets the string representation of the window.
	/// </summary>
	public override string? ToString() => Window.ToString();
}
