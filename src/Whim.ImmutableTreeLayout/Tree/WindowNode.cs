namespace Whim.ImmutableTreeLayout;

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
	public override string? ToString() => Window?.ToString();

	/// <inheritdoc />
	public override bool Equals(object? obj)
	{
		//
		// See the full list of guidelines at
		//   http://go.microsoft.com/fwlink/?LinkID=85237
		// and also the guidance for operator== at
		//   http://go.microsoft.com/fwlink/?LinkId=85238
		//

		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}

		return obj is WindowNode node && node.Window.Equals(Window);
	}

	/// <inheritdoc />
	public override int GetHashCode() => Window.GetHashCode();
}
