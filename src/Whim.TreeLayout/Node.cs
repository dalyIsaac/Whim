namespace Whim.TreeLayout;

public abstract class Node
{
	public SplitNode? Parent { get; set; }

	private double _weight = 1;
	public double Weight
	{
		get => Parent?.EqualWeight == true ? 1d / Parent.Children.Count : _weight;
		set
		{
			_weight = value;
			if (Parent != null)
			{
				Parent.EqualWeight = false;
			}
		}
	}
}
