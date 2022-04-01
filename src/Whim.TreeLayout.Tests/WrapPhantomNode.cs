namespace Whim.TreeLayout.Tests;

internal class WrapPhantomNode : PhantomNode
{
	public WrapPhantomNode(IWindow windowModel, SplitNode? parent = null) : base(windowModel, null, parent)
	{
	}
}
