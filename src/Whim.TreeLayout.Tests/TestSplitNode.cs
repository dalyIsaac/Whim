namespace Whim.TreeLayout.Tests;

/// <summary>
/// TestSplitNode is used only to wrap the <see cref="SplitNode"/> class, so
/// we can initialize the nodes and weights.
/// </summary>
public class TestSplitNode : SplitNode
{
	public TestSplitNode(SplitNodeDirection direction, SplitNode? parent = null) : base(direction, parent)
	{
	}

	public void Initialize(List<Node> nodes, List<double> weights)
	{
		if (nodes.Count != weights.Count)
		{
			throw new ArgumentException("The number of weights must match the number of children.");
		}

		if (weights.Sum() != 1d)
		{
			throw new ArgumentException("The weights must sum to 1.");
		}

		if (_children.Count > 0 || _weights.Count > 0)
		{
			throw new InvalidOperationException("The node has already been initialized.");
		}

		// If the weights are not equal, set EqualWeight to false.
		EqualWeight = weights.All(w => w == weights[0]);

		_children.AddRange(nodes);
		_weights.AddRange(weights);
	}
}
