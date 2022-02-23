namespace Whim.TreeLayout.Tests;

public static class NodeExtensions
{
	/// <summary>
	/// Returns the weight of the node. This functionality is only needed
	/// in tests.
	/// </summary>
	public static double GetWeight(this Node node)
	{
		return node.Parent?.GetChildWeight(node) ?? -1d;
	}
}
