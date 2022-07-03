using System;

namespace Whim.TreeLayout;

/// <summary>
/// WindowNodes represent the location of a window within the layout engine.
/// Unlike <see cref="PhantomNode"/>s, <see cref="WindowNode"/>s are common
/// to all layout engines within a <see cref="IWorkspace"/>.
/// </summary>
public class WindowNode : LeafNode
{
	public WindowNode(IWindow window, SplitNode? parent = null) : base(window, parent) { }
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

		return obj is WindowNode node &&
			node.Window.Equals(Window) &&
			// Checking for parent equality is too dangerous, as there are cycles.
			((node.Parent == null) == (Parent == null));
	}

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(Window, Parent);
}
