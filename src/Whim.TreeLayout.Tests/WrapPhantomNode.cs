namespace Whim.TreeLayout.Tests;

internal class WrapPhantomNode : PhantomNode
{
	public WrapPhantomNode(IContext context, IWindow windowModel, SplitNode? parent = null)
		: base(context, windowModel, null, parent) { }
}
