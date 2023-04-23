namespace Whim.TreeLayout;

/// <summary>
/// A tree layout engine allows users to create arbitrary window grid layouts.
/// </summary>
public interface ITreeLayoutEngine : ILayoutEngine
{
	/// <summary>
	/// Flip the direction of the <see cref="SplitNode"/> parent of the currently focused window, and merge it with
	/// the grandparent <see cref="SplitNode"/>.
	/// </summary>
	void FlipAndMerge();

	/// <summary>
	/// Split the focused window in two, and insert a phantom window in the direction
	/// of <see cref="TreeLayoutEngine.AddNodeDirection"/>.
	/// </summary>
	void SplitFocusedWindow();
}
